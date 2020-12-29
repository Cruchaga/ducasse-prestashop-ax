using IntegracionPrestashopLib.Bussiness.Clientes;
using IntegracionPrestashopLib.Bussiness.Util;
using IntegracionPrestashopLib.Schema.Dictionary;
using IntegracionPrestashopLib.Schema.ERP;
using IntegracionPrestashopLib.Schema.PS;
using IntegracionPrestashopLib.Schema.PS.Responses;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace IntegracionPrestashopLib.Operations
{
    public class OrdenDAL
    {
        private String apiKey;
        private String baseUrl;

        private PSRecursoClient<Estado, EstadoResponse> pSEstado = new PSRecursoClient<Estado, EstadoResponse>();
        private PSRecursoClient<Customer, CustomerResponse> psCustomer = new PSRecursoClient<Customer, CustomerResponse>();
        private PSRecursoClient<Address, AddressResponse> pSDelivery = new PSRecursoClient<Address, AddressResponse>();
        private PSRecursoClient<State, StateResponse> psState = new PSRecursoClient<State, StateResponse>();
        private PSRecursoClient<Currency, CurrencyResponse> psCurrency = new PSRecursoClient<Currency, CurrencyResponse>();

        public OrdenDAL(String apiKey, String prestashopUrlBase)
        {
            this.apiKey = apiKey;
            this.baseUrl = prestashopUrlBase;
        }
        /**
         * <summary>
         * Obtiene las ordenes que coincidan dentro del rango de fechas consultadas
         * </summary>
         * <remarks>
         * Valores <b>requeridos</b> como parametros [fecha_inicio(DateTime), fecha_final(DateTime)]
         * </remarks>
         */
        public Schema.dto.OrdenesResponse GetOrdenes(DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(baseUrl))
                {
                    return new Schema.dto.OrdenesResponse
                    {
                        ErrorCode = IntegracionErrorCode.BAD_INITIALIZED,
                        Error = $"No se ha inicializado el objeto como corresponde"
                    };
                }

                Stopwatch sw = new Stopwatch();
                sw.Start();
                var response = PSOrdenClient.GetOrdenes(fechaInicio, fechaFin, apiKey, baseUrl);
                var ordenesList = response.Orders;
                sw.Stop();
                Console.WriteLine($"Tiempo Ordenes: {sw.Elapsed.TotalMilliseconds} ms");
                Console.WriteLine("--------------");
                sw.Reset();
                var resp = (from oc in ordenesList
                            select new OrdenDeVenta
                            {
                                SalesIdPrestaShop = oc.Id,
                                SalesDate = oc.DateAdd,
                                Facturacion = new Facturacion
                                {
                                    CustomerId = oc.InvoiceDeliveryId,
                                    Direccion = new Direccion()
                                },
                                Despacho = new Schema.ERP.Despacho
                                {
                                    AccountNum = oc.AddressDeliveryId,
                                    Direccion = new Direccion()
                                },
                                PaymMode = oc.Payment,
                                Factura = true, //TODO: averiguar con Ivan aguilera
                                TotalWeight = 0,
                                CurrencyId = oc.CurrencyId, //TODO: normalizar currency table con ERP
                                TotalNetAmount = Decimal.Parse(oc.TotalNeto),
                                TaxAmount = Decimal.Parse(oc.TotalFinal) - Decimal.Parse(oc.TotalNeto),
                                TotalAmount = Decimal.Parse(oc.TotalFinal),
                                DetalleDeVentas = new DetalleDeVenta((from d in oc.Asociados.productos
                                                                      select new Detalle
                                                                      {
                                                                          ItemId = d.ProductId,
                                                                          InventDimId = d.Reference,
                                                                          SalesLineQty = Decimal.Parse(d.Cantidad.Replace(".", ",")),
                                                                          SalesLinePriceList = Decimal.Parse(d.PrecioSinImpuesto.Replace(".", ",")) / Decimal.Parse(d.Cantidad.Replace(".", ",")),
                                                                          SalesLineDiscountPercent = 0,
                                                                          SalesLineNetTotal = Decimal.Parse(d.PrecioSinImpuesto.Replace(".", ",")),
                                                                          SalesLineTotalAmount = Decimal.Parse(d.PrecioConImpuesto.Replace(".", ","))
                                                                      }).ToList()),
                                EstadoId = oc.CurrentState
                            }).ToList<OrdenDeVenta>();

                List<Task> tasks = new List<Task>();
                resp.ForEach(o =>
                {
                    Task tEstado = Task.Run(() =>
                    {
                        var estadoResp = pSEstado.GetRecurso("order_states", o.EstadoId, apiKey, baseUrl);
                        if (estadoResp != null)
                        {
                            var estado = estadoResp.estado;
                            o.Estado = estado.Nombres;
                        }
                    });

                    Task tCurrency = Task.Run(() =>
                    {
                        var currencyRep = psCurrency.GetRecurso("currencies", o.CurrencyId, apiKey, baseUrl);
                        if (currencyRep != null)
                        {
                            var currency = currencyRep.Currency;
                            o.CurrencyCode = currency.CurrencyCode;
                            o.DetalleDeVentas.itemsVendidos.ForEach(d =>
                            {
                                d.SalesLineCurrencyCode = currency.CurrencyCode;
                            });
                        }
                    });
                    Task tInvoiceAdd = Task.Run(() =>
                    {
                        var invoiceAddressResp = pSDelivery.GetRecurso("addresses", o.Facturacion.CustomerId, apiKey, baseUrl);
                        if (invoiceAddressResp != null)
                        {
                            var invoiceAddress = invoiceAddressResp.Address;
                            o.Facturacion.Direccion.AddressCity = invoiceAddress.Ciudad;
                            o.Facturacion.Direccion.AddressStreet = invoiceAddress.Direccion1;
                            Task tState = Task.Run(() =>
                            {
                                var comunaResp = psState.GetRecurso("states", invoiceAddress.StateId, apiKey, baseUrl);

                                if (comunaResp != null)
                                {
                                    o.Facturacion.Direccion.AddressState = comunaResp.State.StateCode;
                                }
                            });

                            Task tCustomDel = Task.Run(() =>
                            {
                                var customerDeliveryResp = psCustomer.GetRecurso("customers", invoiceAddress.CustomerId, apiKey, baseUrl);
                                if (customerDeliveryResp != null)
                                {
                                    var customerDelivery = customerDeliveryResp.Clientes;
                                    o.Facturacion.EmailInvoice = customerDelivery.Email;
                                }
                            });
                            o.Facturacion.CustNameInvoice = $"{invoiceAddress.Firstname} {invoiceAddress.Lastname}";
                            o.Facturacion.Rut = Functions.GetNumberFromRut(invoiceAddress.RutEmpresa);
                            //o.Facturacion.DigitMod11 = Functions.GetDvFromRut(invoiceAddress.RutEmpresa);
                            o.Facturacion.LiteralBusiness = invoiceAddress.Company;
                        }
                    });

                    Task tDeliveryAddress = Task.Run(() =>
                    {
                        var deliveryAddressResp = pSDelivery.GetRecurso("addresses", o.Despacho.AccountNum, apiKey, baseUrl);
                        if (deliveryAddressResp != null)
                        {
                            var deliveryAddress = deliveryAddressResp.Address;
                            o.Despacho.ContactTelephoneNumber = deliveryAddress.Phone;
                            o.Despacho.Direccion.AddressCity = deliveryAddress.Ciudad;
                            o.Despacho.Direccion.AddressStreet = deliveryAddress.Direccion1;

                            o.Despacho.CustName = $"{deliveryAddress.Firstname} {deliveryAddress.Lastname}";
                            o.Despacho.AccountNum = Functions.GetNumberFromRut(deliveryAddress.RutEmpresa);
                            o.Despacho.DigitMod11 = Functions.GetNumberFromRut(deliveryAddress.RutEmpresa);
                            o.Despacho.ContactTelephoneNumber = deliveryAddress.Phone;
                            o.Despacho.LiteralBusiness = deliveryAddress.Company;

                            Task tState = Task.Run(() =>
                            {
                                var stateResp = psState.GetRecurso("states", deliveryAddress.StateId, apiKey, baseUrl);
                                if (stateResp != null)
                                    o.Despacho.Direccion.AddressState = stateResp.State.StateCode;
                            });
                            Task tCustomDel = Task.Run(() =>
                            {
                                var customerDeliveryResp = psCustomer.GetRecurso("customers", deliveryAddress.CustomerId, apiKey, baseUrl);
                                if(customerDeliveryResp != null)
                                {
                                    var customerDelivery = customerDeliveryResp.Clientes;
                                    o.Despacho.ContactEmail = customerDelivery.Email;
                                }
                            });
                        }
                    });
                    tEstado.Wait();
                    tCurrency.Wait();
                    tInvoiceAdd.Wait();
                    tDeliveryAddress.Wait();
                });

                var ordenes = new Schema.dto.OrdenesResponse();
                ordenes.SetOrdenesDeVenta(resp);
                return ordenes;
            }
            catch (Exception e)
            {
                return new Schema.dto.OrdenesResponse
                {
                    ErrorCode = IntegracionErrorCode.UNCONTROLATED,
                    Error = e.Message
                };
            }
        }
        /**
         * <summary>
         * Metodo que permite la actualización de las ordenes de venta en Prestashop
         * </summary>
         * <remarks>
         * Esperado para actualizar el tracking del pedido
         * </remarks>
         */
        public UpdateResponse Update(Tracking tracking)
        {
            //Type _type;
            //FieldInfo[] propList;
            //bool existe;
            //ValidateEstadoId(tracking.NuevoEstadoId, out _type, out propList, out existe);
            //if (!existe)
            //{
            //    return new UpdateResponse
            //    {
            //        Actualizado = false,
            //        ErrorCode = IntegracionErrorCode.BAD_ATTRIBUTE,
            //        Error = $"El valor de estado [{tracking.NuevoEstadoId}] no se reconoce. Los valores aceptados son: [{String.Join(",", propList.Select(p => $"{p.GetValue(_type).ToString()}").ToArray<String>())}]"
            //    };
            //}

            return PSOrdenClient.UpdateTracking(new OrderHistory
            {
                EmployeeId = 2,
                EstadoId = tracking.NuevoEstadoId,
                OrderId = tracking.OrdenId
            }, apiKey, baseUrl);

        }

        private static void ValidateEstadoId(int estado, out Type _type, out FieldInfo[] propList, out bool existe)
        {
            _type = typeof(IntegracionOrdenEstados);
            propList = _type.GetFields();
            existe = false;
            foreach (System.Reflection.FieldInfo prop in propList)
            {
                if ((int)prop.GetValue(_type) == estado)
                    existe = true;
            }
        }
    }
}
