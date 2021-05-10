using IntegracionPrestashopLib.Bussiness.Clientes;
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
        private int userId;

        private PSRecursoClient<Estado, EstadoResponse> pSEstado = new PSRecursoClient<Estado, EstadoResponse>("IntegracionPrestashopLib.Schema.PS.Responses.EstadoResponse");
        private PSRecursoClient<Customer, CustomerResponse> psCustomer = new PSRecursoClient<Customer, CustomerResponse>("IntegracionPrestashopLib.Schema.PS.Responses.CustomerResponse");
        private PSRecursoClient<Address, AddressResponse> pSDelivery = new PSRecursoClient<Address, AddressResponse>("IntegracionPrestashopLib.Schema.PS.Responses.AddressResponse");
        private PSRecursoClient<State, StateResponse> psState = new PSRecursoClient<State, StateResponse>("IntegracionPrestashopLib.Schema.PS.Responses.StateResponse");
        private PSRecursoClient<Currency, CurrencyResponse> psCurrency = new PSRecursoClient<Currency, CurrencyResponse>("IntegracionPrestashopLib.Schema.PS.Responses.CurrencyResponse");
        private PSRecursoClient<Payment, PaymentsResponse> psPayment = new PSRecursoClient<Payment, PaymentsResponse>("IntegracionPrestashopLib.Schema.PS.Responses.PaymentsResponse");
        private PSRecursoClient<Message, MessageResponse> psMessage = new PSRecursoClient<Message, MessageResponse>("IntegracionPrestashopLib.Schema.PS.Responses.MessageResponse");
        private PSRecursoClient<tDoc, tDoc> psDocument = new PSRecursoClient<tDoc, tDoc>("IntegracionPrestashopLib.Schema.PS.tDoc");

        public OrdenDAL(String apiKey, String prestashopUrlBase, int userId)
        {
            this.apiKey = apiKey;
            this.baseUrl = prestashopUrlBase;
            this.userId = userId;
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
                                Referencia = oc.Referencia,
                                IdCart = oc.IdCart,
                                SalesDate = oc.DateAdd,
                                CustomerId = oc.CustomerId,
                                Facturacion = new Facturacion
                                {
                                    Direccion = new Direccion
                                    {
                                        AddressId = oc.InvoiceDeliveryId
                                    }
                                },
                                Despacho = new Schema.ERP.Despacho
                                {
                                    Direccion = new Direccion
                                    {
                                        AddressId = oc.AddressDeliveryId
                                    }
                                },
                                PaymMode = oc.Payment,
                                Factura = false, 
                                TotalWeight = 0,
                                CurrencyId = oc.CurrencyId, 
                                TotalNetAmount = Decimal.Parse(oc.TotalNeto.Replace(".", ",")),
                                TaxAmount = Decimal.Parse(oc.TotalFinal.Replace(".", ",")) - Decimal.Parse(oc.TotalNeto.Replace(".", ",")),
                                TotalShippingAmount = Decimal.Parse(oc.TotalShipping.Replace(".", ",")),
                                NetShippingAmount = Decimal.Parse(oc.NetShipping.Replace(".", ",")),
                                TotalAmount = Decimal.Parse(oc.TotalFinal.Replace(".", ",")),
                                DetalleDeVentas = new DetalleDeVenta((from d in oc.Asociados.productos
                                                                      select new Detalle
                                                                      {
                                                                          ItemId = d.ProductId,
                                                                          Sku = d.Reference,
                                                                          SalesLineQty = Decimal.Parse(d.Cantidad.Replace(".", ",")),
                                                                          SalesLineNetPrice = Decimal.Parse(d.PrecioSinImpuesto.Replace(".", ",")), //TODO: agregar dcto
                                                                          SalesLinePrice = Decimal.Parse(d.PrecioConImpuesto.Replace(".", ",")),//TODO: agregar dcto
                                                                          SalesLineNetTotalAmount = Decimal.Parse(d.PrecioSinImpuesto.Replace(".", ",")) * Decimal.Parse(d.Cantidad.Replace(".", ",")),
                                                                          SalesLineTotal = Decimal.Parse(d.PrecioConImpuesto.Replace(".", ",")) * Decimal.Parse(d.Cantidad.Replace(".", ","))
                                                                      }).ToList()),
                                EstadoId = oc.CurrentState
                            }).ToList<OrdenDeVenta>();

                List<Task> tasks = new List<Task>();
                resp.ForEach(o =>
                {
                    Customer customer;
                    Task tEstado = Task.Run(() =>
                    {
                        var estadoResp = pSEstado.GetRecurso("order_states", o.EstadoId, apiKey, baseUrl);
                        if (estadoResp != null)
                        {
                            var estado = estadoResp.estado;
                            o.Estado = estado.Nombres;
                        }
                    });

                    Task tCustomer = Task.Run(() =>
                    {
                        var customerResp = psCustomer.GetRecurso("customers", o.CustomerId, apiKey, baseUrl);
                        if (customerResp != null)
                        {
                            customer = customerResp.Clientes;
                            o.CustomerName = $"{customer.Nombre} {customer.Apellido}";
                            o.CustomerEmail = customer.Email;
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
                        var invoiceAddressResp = pSDelivery.GetRecurso("addresses", o.Facturacion.Direccion.AddressId, apiKey, baseUrl);
                        if (invoiceAddressResp != null)
                        {
                            var invoiceAddress = invoiceAddressResp.Address;
                            o.Facturacion.Direccion.AddressCity = invoiceAddress.Ciudad;
                            o.Facturacion.Direccion.AddressStreet = invoiceAddress.Direccion1;
                            o.Facturacion.PhoneNumber = invoiceAddress.Phone;
                            o.Facturacion.CustNameInvoice = invoiceAddress.Company == "" ? $"{invoiceAddress.Firstname} {invoiceAddress.Lastname}" : invoiceAddress.Company;
                            o.Facturacion.Rut = invoiceAddress.RutEmpresa;
                            o.Facturacion.LiteralBusiness = invoiceAddress.Other;
                            Task tState = Task.Run(() =>
                            {
                                var comunaResp = psState.GetRecurso("states", invoiceAddress.StateId, apiKey, baseUrl);

                                if (comunaResp != null)
                                {
                                    string codComuna = GetStringDecimalFromHexa(comunaResp.State.StateCode);
                                    o.Facturacion.Direccion.AddressCounty = codComuna;
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
                            Task tDoc = Task.Run(() =>
                            {
                                String url = baseUrl.Replace("api", "WS");

                                var document = psDocument.GetRecursoWithFilter("tdoc.php", o.IdCart , apiKey, url, $"&id_carro={o.IdCart}");
                                o.Factura = document != null && document.Tipo == 33;

                            });
                            tState.Wait();
                            tCustomDel.Wait();
                            tDoc.Wait();
                        }
                    });
                    Task tPayments = Task.Run(() =>
                    {
                        var urlPayment = $"{baseUrl.Replace("api/", "WS/webpayinfo.php")}";

                        var parametros = new Dictionary<String, String>();
                        parametros.Add("order_id", o.SalesIdPrestaShop.ToString().Replace(",", "."));
                        parametros.Add("cart_id", o.IdCart);

                        int idCart = 0;
                        if (int.TryParse(o.IdCart, out idCart)) {
                            decimal signature = ((o.SalesIdPrestaShop + 3) * 29) + (idCart * 17);
                            parametros.Add("signature", signature.ToString().Replace(",", "."));
                            var payment = PSOrdenClient.GetOrderPayment(urlPayment, parametros);

                            if (payment != null)
                            {
                                var tbkInfo = payment.transbank_response;

                                o.TransactionInfo = new TransbankInfo
                                {
                                    Token = payment.token,
                                    AccountingDate = tbkInfo.accountingDate,
                                    Amount = tbkInfo.amount,
                                    AuthorizationCode = tbkInfo.authorizationCode,
                                    Balance = tbkInfo.balance == null ? "" : tbkInfo.balance.ToString(),
                                    BuyOrder = tbkInfo.buyOrder,
                                    Card_number = tbkInfo.cardDetail.card_number,
                                    InstallmentsAmount = tbkInfo.installmentsAmount == null ? "" : tbkInfo.installmentsAmount.ToString(),
                                    InstallmentsNumber = tbkInfo.installmentsNumber == null ? "" : tbkInfo.installmentsNumber.ToString(),
                                    PaymentTypeCode = tbkInfo.paymentTypeCode,
                                    ResponseCode = tbkInfo.responseCode,
                                    SessionId = tbkInfo.sessionId,
                                    Status = tbkInfo.status,
                                    TransactionDate = tbkInfo.transactionDate,
                                    Vci = tbkInfo.vci

                                };

                            }
                            else
                            {
                                o.TransactionInfo = new TransbankInfo();
                            }
                        }
                    });

                    Task tDeliveryAddress = Task.Run(() =>
                    {
                        var deliveryAddressResp = pSDelivery.GetRecurso("addresses", o.Despacho.Direccion.AddressId, apiKey, baseUrl);
                        if (deliveryAddressResp != null)
                        {
                            var deliveryAddress = deliveryAddressResp.Address;
                            o.Despacho.EsRetiroEnTienda = deliveryAddress.Alias == "Pickup at Store" || deliveryAddress.Alias == "Recogida en tienda";
                            o.Despacho.ContactTelephoneNumber = deliveryAddress.Phone;
                            o.Despacho.Alias = deliveryAddress.Alias;
                            o.Despacho.Direccion.AddressCity = deliveryAddress.Ciudad;
                            o.Despacho.Direccion.AddressStreet = deliveryAddress.Direccion1;

                            o.Despacho.CustName = $"{deliveryAddress.Firstname} {deliveryAddress.Lastname}";
                            o.Despacho.Rut = deliveryAddress.RutEmpresa;
                            o.Despacho.ContactTelephoneNumber = deliveryAddress.Phone;
                            o.Despacho.LiteralBusiness = deliveryAddress.Company;
                            Task tState = Task.Run(() =>
                            {
                                var stateResp = psState.GetRecurso("states", deliveryAddress.StateId, apiKey, baseUrl);
                                if (stateResp != null)
                                    o.Despacho.Direccion.AddressCounty = GetStringDecimalFromHexa(stateResp.State.StateCode);
                            });

                            Task tCustomDel = Task.Run(() =>
                            {
                                var customerDeliveryResp = psCustomer.GetRecurso("customers", deliveryAddress.CustomerId, apiKey, baseUrl);
                                if (customerDeliveryResp != null)
                                {
                                    var customerDelivery = customerDeliveryResp.Clientes;
                                    o.Despacho.ContactEmail = customerDelivery.Email;
                                }
                            });
                            tState.Wait();
                            tCustomDel.Wait();

                        }
                    });

                    Task tComentario = Task.Run(() =>
                    {
                        var comentariosResponse = psMessage.GetRecursoWithFilter("messages", $"{o.SalesIdPrestaShop}", apiKey, baseUrl, $"&filter[id_order]=[{o.SalesIdPrestaShop}]&display=full");
                        if (comentariosResponse != null && comentariosResponse.OrderPayments != null)
                        {
                            o.Mensajes = new Comentarios(
                                            (from m in comentariosResponse.OrderPayments
                                             select new Comentario
                                             {
                                                 Msg = m.Msg,
                                                 FechaComentario = m.fechaHora
                                             }).ToList());
                        }
                        else
                            o.Mensajes = new Comentarios
                            {
                                Mensajes = new List<Comentario>()
                            };
                    });
                    tComentario.Wait();
                    tEstado.Wait();
                    tCurrency.Wait();
                    tInvoiceAdd.Wait();
                    tDeliveryAddress.Wait();
                    tCustomer.Wait();

                    if (o.Despacho.EsRetiroEnTienda)
                    {
                        o.Despacho.ContactEmail = o.CustomerEmail;
                        o.Despacho.ContactTelephoneNumber = o.Facturacion.PhoneNumber;
                    }
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

        private static string GetStringDecimalFromHexa(String comunaResp)
        {
            String decimalCode = Convert.ToInt64(comunaResp, 16).ToString();
            return decimalCode.Length == 4 ? $"0{decimalCode}" : decimalCode;
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
            if (this.userId != 0)
            {
                return PSOrdenClient.UpdateTracking(new OrderHistory
                {
                    EmployeeId = userId,
                    EstadoId = tracking.NuevoEstadoId,
                    OrderId = tracking.OrdenId,
                    Informar = true
                }, apiKey, baseUrl);
            }
            else
            {
                return new UpdateResponse
                {
                    Actualizado = false,
                    Error = $"No se ha identificado el id de usuario [user id = {userId}].",
                    ErrorCode = IntegracionErrorCode.BAD_ATTRIBUTE
                };
            }
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
