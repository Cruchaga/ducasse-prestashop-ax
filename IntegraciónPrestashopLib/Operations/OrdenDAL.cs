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
using System.Text;
using System.Threading.Tasks;

namespace IntegracionPrestashopLib.Operations
{
    public class OrdenDAL
    {
        private const string WEBPAY = "Webpay Plus";
        private const string BCHILE = "Banchile Pagos";
        private const decimal IVA = 1.19m;

        /// <summary>Firma simple esperada por los endpoints WS/*.php de pago (Webpay/BCH).</summary>
        private static decimal CalcularFirmaPago(long orderId, int cartId)
        {
            return ((orderId + 3) * 29) + (cartId * 17);
        }

        /// <summary>Construye la URL de un script WS/*.php a partir de la base del webservice.</summary>
        private string WsUrl(string phpFile)
        {
            return baseUrl.Replace("api/", $"WS/{phpFile}");
        }

        private String apiKey;
        private String baseUrl;
        private int userId;

        private PSRecursoClient<Estado, EstadoResponse> psEstado = new PSRecursoClient<Estado, EstadoResponse>("IntegracionPrestashopLib.Schema.PS.Responses.EstadoResponse");
        private PSRecursoClient<Customer, CustomerResponse> psCustomer = new PSRecursoClient<Customer, CustomerResponse>("IntegracionPrestashopLib.Schema.PS.Responses.CustomerResponse");
        private PSRecursoClient<Address, AddressResponse> psDelivery = new PSRecursoClient<Address, AddressResponse>("IntegracionPrestashopLib.Schema.PS.Responses.AddressResponse");
        private PSRecursoClient<State, StateResponse> psState = new PSRecursoClient<State, StateResponse>("IntegracionPrestashopLib.Schema.PS.Responses.StateResponse");
        private PSRecursoClient<Currency, CurrencyResponse> psCurrency = new PSRecursoClient<Currency, CurrencyResponse>("IntegracionPrestashopLib.Schema.PS.Responses.CurrencyResponse");
        private PSRecursoClient<Payment, PaymentsResponse> psPayment = new PSRecursoClient<Payment, PaymentsResponse>("IntegracionPrestashopLib.Schema.PS.Responses.PaymentsResponse");
        private PSRecursoClient<Message, MessageResponse> psMessage = new PSRecursoClient<Message, MessageResponse>("IntegracionPrestashopLib.Schema.PS.Responses.MessageResponse");

        private PSRecursoClient<Schema.PS.Producto, ProductResponse> psProducto = new PSRecursoClient<Schema.PS.Producto, ProductResponse>("IntegracionPrestashopLib.Schema.PS.Responses.ProductResponse");
        private PSRecursoClient<Schema.PS.Combination, CombinationResponse> psCombinacion = new PSRecursoClient<Schema.PS.Combination, CombinationResponse>("IntegracionPrestashopLib.Schema.PS.Responses.CombinationResponse");
        private PSRecursoClient<tDoc, tDoc> psDocument = new PSRecursoClient<tDoc, tDoc>("IntegracionPrestashopLib.Schema.PS.tDoc");
        private PSRecursoClient<CartRule, CartRuleResponse> psCartRules = new PSRecursoClient<CartRule, CartRuleResponse>("IntegracionPrestashopLib.Schema.PS.Responses.CartRuleResponse");

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
            Traza sbTraza = new Traza();
            try
            {
                sbTraza.Append($"Iniciando obtener ordenes con url =[{baseUrl}] apiKey= [{apiKey}] y userId = [{userId}]{Environment.NewLine}");
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
                OrdenesResponse response = PSOrdenClient.GetOrdenes(fechaInicio, fechaFin, apiKey, baseUrl, sbTraza);
                IList<OrdenDeCompra> ordenesList = response.Orders;
                if (ordenesList.Count > 0) { 
                    Console.WriteLine($"Se encontraron {ordenesList.Count} ordenes, se inicia el completar la info{Environment.NewLine}");
                    sbTraza.Append($"Se encontraron {ordenesList.Count} ordenes, se inicia el completar la info{Environment.NewLine}");
                }
                else
                {
                    Console.WriteLine($"No se encontraron ordenes{Environment.NewLine}");
                    sbTraza.Append($"No se encontraron ordenes{Environment.NewLine}");
                }
               
                sw.Stop();
                Console.WriteLine($"Tiempo Ordenes: {sw.Elapsed.TotalMilliseconds} ms");
                Console.WriteLine("--------------");
                sw.Reset();
                List<OrdenDeVenta> resp = (from oc in ordenesList
                select new OrdenDeVenta
                {
                    SalesIdPrestaShop = oc.Id,
                    Referencia = oc.Referencia,
                    IdCart = oc.IdCart,
                    SalesDate = oc.DateAdd,
                    CustomerId = oc.CustomerId,
                    Facturacion = new Facturacion { Direccion = new Direccion { AddressId = oc.InvoiceDeliveryId } },
                    Despacho = new Schema.ERP.Despacho { Direccion = new Direccion { AddressId = oc.AddressDeliveryId } },
                    PaymMode = oc.Payment,
                    Factura = false,
                    TotalWeight = 0,
                    CurrencyId = oc.CurrencyId,
                    TotalNetAmount = Functions.ParseDecimal(oc.TotalNeto),
                    TaxAmount = Functions.ParseDecimal(oc.TotalFinal) - Functions.ParseDecimal(oc.TotalNeto),
                    TotalShippingAmount = Functions.ParseDecimal(oc.TotalShipping),
                    NetShippingAmount = Functions.ParseDecimal(oc.NetShipping),
                    TotalAmount = Functions.ParseDecimal(oc.TotalFinal),
                    TotalDicount = oc.TotalDiscounts,
                    DetalleDeVentas = new DetalleDeVenta((from d in oc.Asociados.productos
                    select new Detalle
                    {
                        ItemId = d.ProductId,
                        Sku = d.Reference,
                        SalesLineQty = Functions.ParseDecimal(d.Cantidad),
                        SalesLineNetPrice = Functions.ParseDecimal(d.PrecioSinImpuesto),
                        SalesLinePrice = Functions.ParseDecimal(d.PrecioConImpuesto),
                        SalesLineNetTotalAmount = Functions.ParseDecimal(d.PrecioSinImpuesto) * Functions.ParseDecimal(d.Cantidad),
                        SalesLineTotal = Functions.ParseDecimal(d.PrecioConImpuesto) * Functions.ParseDecimal(d.Cantidad)
                    }).ToList()),
                    EstadoId = oc.CurrentState
                }).ToList<OrdenDeVenta>();

                List<Task> tasks = new List<Task>();
                resp.ForEach(o =>
                {

                    Task tProducts = Task.Run(() =>
                    {
                        List<Detalle> itemsTemp = o.DetalleDeVentas.itemsVendidos;
                        o.DetalleDeVentas.itemsVendidos = new List<Detalle>();

                        foreach (Detalle prod in itemsTemp)
                        {
                            sbTraza.Append($"Revisando si {prod.ItemId} es un producto compuesto, para orden {o.SalesIdPrestaShop} {Environment.NewLine}");
                            ProductResponse productResp = psProducto.GetRecurso("products", prod.ItemId, apiKey, baseUrl, sbTraza);
                            if (productResp != null)
                            {
                                if(productResp.Product.CacheIsPack != "1")
                                {
                                    o.DetalleDeVentas.itemsVendidos.Add(prod);
                                    prod.Categories = productResp.Product.Associations.categories;
                                }
                                else
                                {
                                    List<Detalle> openedItemsTemp = OpenPackDetail(prod, sbTraza);
                                    foreach (Detalle item in openedItemsTemp)
                                    {
                                        o.DetalleDeVentas.itemsVendidos.Add(item);
                                    }
                                }
                            }
                        }

                    });

                    Customer customer;
                    Task tEstado = Task.Run(() =>
                    {
                        sbTraza.Append($"Iniciando búsqueda de estado para orden {o.SalesIdPrestaShop} {Environment.NewLine}");
                        EstadoResponse estadoResp = psEstado.GetRecurso("order_states", o.EstadoId, apiKey, baseUrl, sbTraza);
                        if (estadoResp != null)
                        {
                            Estado estado = estadoResp.estado;
                            o.Estado = estado.Nombres;
                        }
                    });

                    Task tCustomer = Task.Run(() =>
                    {
                        sbTraza.Append($"Iniciando búsqueda de cliente para orden {o.SalesIdPrestaShop} {Environment.NewLine}");
                        CustomerResponse customerResp = psCustomer.GetRecurso("customers", o.CustomerId, apiKey, baseUrl, sbTraza);
                        if (customerResp != null)
                        {
                            customer = customerResp.Clientes;
                            o.CustomerName = $"{customer.Nombre} {customer.Apellido}";
                            o.CustomerEmail = customer.Email;
                        }
                    });

                    Task tCurrency = Task.Run(() =>
                    {
                        sbTraza.Append($"Iniciando búsqueda de currency para orden {o.SalesIdPrestaShop} {Environment.NewLine}");
                        CurrencyResponse currencyRep = psCurrency.GetRecurso("currencies", o.CurrencyId, apiKey, baseUrl, sbTraza);
                        if (currencyRep != null)
                        {
                            Currency currency = currencyRep.Currency;
                            o.CurrencyCode = currency.CurrencyCode;
                            o.DetalleDeVentas.itemsVendidos.ForEach(d =>
                            {
                                d.SalesLineCurrencyCode = currency.CurrencyCode;
                            });
                        }
                    });
                    Task tInvoiceAdd = Task.Run(() => 
                    {
                        sbTraza.Log($"Iniciando búsqueda de direccion de factura para orden {o.SalesIdPrestaShop} ");
                        AddressResponse invoiceAddressResp = psDelivery.GetRecurso("addresses", o.Facturacion.Direccion.AddressId, apiKey, baseUrl, sbTraza);
                        if (invoiceAddressResp != null)
                        {
                            Address invoiceAddress = invoiceAddressResp.Address;
                            o.Facturacion.Direccion.AddressCity = invoiceAddress.Ciudad;
                            o.Facturacion.Direccion.AddressStreet = invoiceAddress.Direccion1;
                            o.Facturacion.PhoneNumber = invoiceAddress.Phone;
                            o.Facturacion.CustNameInvoice = invoiceAddress.Company == "" ? $"{invoiceAddress.Firstname} {invoiceAddress.Lastname}" : invoiceAddress.Company;
                            o.Facturacion.Rut = invoiceAddress.RutEmpresa.Trim();
                            o.Facturacion.LiteralBusiness = invoiceAddress.Other;





                            Task tState = Task.Run(() =>
                            {
                                sbTraza.Append($"Iniciando búsqueda de estado para dirección {o.Facturacion.Direccion.AddressId} de orden {o.SalesIdPrestaShop} {Environment.NewLine}");
                                StateResponse comunaResp = psState.GetRecurso("states", invoiceAddress.StateId, apiKey, baseUrl, sbTraza);

                                if (comunaResp != null)
                                {
                                    string codComuna = GetStringDecimalFromHexa(comunaResp.State.StateCode);
                                    o.Facturacion.Direccion.AddressCounty = codComuna;
                                }
                            });

                            Task tCustomDel = Task.Run(() =>
                            {
                                sbTraza.Append($"Iniciando búsqueda de cliente en direcccion {o.Facturacion.Direccion.AddressId} para orden {o.SalesIdPrestaShop} {Environment.NewLine}");
                                CustomerResponse customerDeliveryResp = psCustomer.GetRecurso("customers", invoiceAddress.CustomerId, apiKey, baseUrl, sbTraza);
                                if (customerDeliveryResp != null)
                                {
                                    Customer customerDelivery = customerDeliveryResp.Clientes;
                                    o.Facturacion.EmailInvoice = customerDelivery.Email;
                                }


                            });
                            tState.Wait();
                            tCustomDel.Wait();
                            
                        }
                    });

                    // Tarea de enriquecimiento del pago (Webpay o Banco de Chile). Se ejecuta
                    // de forma asíncrona y se espera junto al resto antes de retornar la orden.
                    Task tPaymentInfo = null;
                    if( o.PaymMode == WEBPAY ) {
                        tPaymentInfo = Task.Run(() =>
                        {
                            sbTraza.Append($"Iniciando búsqueda de datos transbank para orden {o.SalesIdPrestaShop} {Environment.NewLine}");
                            String urlPayment = WsUrl("webpayinfo.php");

                            Dictionary<string, string> parametros = new Dictionary<string, string>();
                            parametros.Add("order_id", o.SalesIdPrestaShop.ToString().Replace(",", "."));
                            parametros.Add("cart_id", o.IdCart);

                            int idCart = 0;
                            if (int.TryParse(o.IdCart, out idCart)) {
                                decimal signature = CalcularFirmaPago(o.SalesIdPrestaShop, idCart);
                                parametros.Add("signature", Functions.ToInvariant(signature));
                                PaymentResponse payment = PSOrdenClient.GetOrderPayment(urlPayment, parametros);

                                if (payment != null)
                                {
                                    WPPayment tbkInfo = payment.transbank_response;

                                    o.TransactionInfo = new PaymentInfo
                                    {
                                        MedioPago = WEBPAY,
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
                                    o.TransactionInfo = new PaymentInfo { MedioPago = WEBPAY };
                                }
                            }
                        });
                    }
                    else if( o.PaymMode == BCHILE)
                    {
                        tPaymentInfo = Task.Run(() =>
                        {
                            sbTraza.Append($"Iniciando búsqueda de datos Banco de Chile para orden {o.SalesIdPrestaShop} {Environment.NewLine}");
                            String urlPayment = WsUrl("banchileinfo.php");

                            Dictionary<string, string> parametros = new Dictionary<string, string>();
                            parametros.Add("order_id", o.SalesIdPrestaShop.ToString().Replace(",", "."));
                            parametros.Add("cart_id", o.IdCart);

                            int idCart = 0;
                            if (int.TryParse(o.IdCart, out idCart))
                            {
                                decimal signature = CalcularFirmaPago(o.SalesIdPrestaShop, idCart);
                                parametros.Add("signature", Functions.ToInvariant(signature));
                                BanchilePayment payment = PSOrdenClient.GetBanchilePayment(urlPayment, parametros);

                                if (payment != null)
                                {
                                    int responseCode = 0;
                                    int.TryParse(payment.Reason, out responseCode);

                                    o.TransactionInfo = new PaymentInfo
                                    {
                                        MedioPago = BCHILE,
                                        // Campos con nombre Transbank (concepto equivalente)
                                        AuthorizationCode = payment.AuthCode,
                                        Amount = (int)payment.Amount,
                                        Card_number = payment.CardLastDigits,
                                        InstallmentsNumber = payment.Installments,
                                        TransactionDate = payment.Date,
                                        Status = payment.ReasonDescription,
                                        ResponseCode = responseCode,
                                        // Campos propios de Banco de Chile
                                        IdPayment = payment.IdPayment,
                                        CartId = payment.IdOrder,
                                        CurrencyId = payment.IdCurrency,
                                        Franchise = payment.Franchise,
                                        FranchiseName = payment.FranchiseName,
                                        Bank = payment.Bank,
                                        Receipt = payment.Receipt,
                                        Conversion = payment.Conversion,
                                        IpAddress = payment.IpAddress,
                                        PayerEmail = payment.PayerEmail,
                                        RequestId = payment.IdRequest,
                                        Reference = payment.Reference
                                    };
                                }
                                else
                                {
                                    o.TransactionInfo = new PaymentInfo { MedioPago = BCHILE };
                                }
                            }
                        });
                    }
                    else {
                        Console.WriteLine("Pago con " +  o.PaymMode);
                    }

                    Task tDeliveryAddress = Task.Run(() =>
                    {
                        sbTraza.Append($"Iniciando búsqueda de direccion de despacho para orden {o.SalesIdPrestaShop} {Environment.NewLine}");
                        AddressResponse deliveryAddressResp = psDelivery.GetRecurso("addresses", o.Despacho.Direccion.AddressId, apiKey, baseUrl, sbTraza);
                        if (deliveryAddressResp != null)
                        {
                            Address deliveryAddress = deliveryAddressResp.Address;
                            o.Despacho.EsRetiroEnTienda = deliveryAddress.Alias == "Pickup at Store" || deliveryAddress.Alias == "Recogida en tienda";
                            o.Despacho.ContactTelephoneNumber = deliveryAddress.Phone;
                            o.Despacho.Alias = deliveryAddress.Alias;
                            o.Despacho.Direccion.AddressCity = deliveryAddress.Ciudad;
                            o.Despacho.Direccion.AddressStreet = deliveryAddress.Direccion1;

                            o.Despacho.CustName = $"{deliveryAddress.Firstname} {deliveryAddress.Lastname}";
                            o.Despacho.Rut = deliveryAddress.RutEmpresa;
                            o.Despacho.ContactTelephoneNumber = deliveryAddress.Phone;
                            o.Despacho.LiteralBusiness = deliveryAddress.Company;


							/*if (deliveryAddress.Other == "Factura")
							{
								o.Factura = true;
								Console.WriteLine("[DEL] Compra con Factura");
								Console.WriteLine(o.Factura);

							}
							else
							{
								o.Factura = false;
								Console.WriteLine("[DEL] Compra con boleta");
								Console.WriteLine(o.Factura);
							}*/

							Task tState = Task.Run(() =>
                            {
                                sbTraza.Append($"Iniciando búsqueda de comuna para direccion {o.Despacho.Direccion.AddressId} de orden {o.SalesIdPrestaShop} {Environment.NewLine}");
                                StateResponse stateResp = psState.GetRecurso("states", deliveryAddress.StateId, apiKey, baseUrl, sbTraza);
                                if (stateResp != null)
                                    o.Despacho.Direccion.AddressCounty = GetStringDecimalFromHexa(stateResp.State.StateCode);
                            });

                            Task tCustomDel = Task.Run(() =>
                            {
                                sbTraza.Append($"Iniciando búsqueda de cliente para direccion {o.Despacho.Direccion.AddressId} para orden {o.SalesIdPrestaShop} {Environment.NewLine}");
                                CustomerResponse customerDeliveryResp = psCustomer.GetRecurso("customers", deliveryAddress.CustomerId, apiKey, baseUrl, sbTraza);
                                if (customerDeliveryResp != null)
                                {
                                    Customer customerDelivery = customerDeliveryResp.Clientes;
                                    o.Despacho.ContactEmail = customerDelivery.Email;
                                }
                            });
                            tState.Wait();
                            tCustomDel.Wait();

                        }
                    });

                    Task tComentario = Task.Run(() =>
                    {
                        sbTraza.Append($"Iniciando búsqueda de comentarios para orden {o.SalesIdPrestaShop} {Environment.NewLine}");
                        MessageResponse comentariosResponse = psMessage.GetRecursoWithFilter("messages", $"{o.SalesIdPrestaShop}", apiKey, baseUrl, $"&filter[id_order]=[{o.SalesIdPrestaShop}]&display=full", sbTraza);
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

                    tProducts.Wait();
                    Task tDiscounts = Task.Run(() => {
                        if (o.TotalDicount > 0)
                        {

                            sbTraza.Append($"Iniciando búsqueda de datos de carrito {o.IdCart} {Environment.NewLine}");
                            string urlPayment = WsUrl("cart_cart_rules.php");

                            Dictionary<String, String> parametros = new Dictionary<String, String>();
                            parametros.Add("id_cart", o.IdCart);

                            int idCart = 0;
                            if (int.TryParse(o.IdCart, out idCart))
                            {
                                decimal signature = (17) + (idCart * 17);
                                parametros.Add("signature", Functions.ToInvariant(signature));

                                CartRulesResponse[] reglasAplicadas = PSOrdenClient.GetCartRules(urlPayment, parametros);
                                foreach (CartRulesResponse regla in reglasAplicadas)
                                {
                                    if (regla.free_shipping == "1")
                                    {
                                        Console.WriteLine("Free Shipping");
                                        o.NetShippingAmount = 0;
                                        o.TotalShippingAmount = 0;
                                    }
                                    if (regla.TipoRegla == "categories")
                                        {
                                            List<Detalle> prods = o.DetalleDeVentas.itemsVendidos.Where(i => i.Categories.Any(c => c.Id == regla.id_item)).ToList();
                                            if (prods.Count > 0)
                                            { 
                                                if (regla.isPercent())
                                                {
                                                    DescontarPorcentajeADetalles(regla, prods);
                                                }
                                                else
                                                {
                                                    DescontarMontoADetallesEspecificos(regla, prods);
                                                }
                                            }
                                        }
                                    else if(regla.TipoRegla == "products")
                                    {
                                        List<Detalle> prods = o.DetalleDeVentas.itemsVendidos.Where(i => i.ItemId == regla.id_item).ToList();
                                        if (regla.isPercent())
                                        {
                                            DescontarPorcentajeADetalles(regla, prods);
                                            
                                        }
                                        else
                                        {
                                            //logica contar cantidad de prod y descontar monto al pedido 
                                            decimal cuentaProds = o.DetalleDeVentas.itemsVendidos.Sum(i => i.SalesLineQty);
                                            decimal proporcion = regla.getAmountToDiscount() / cuentaProds;
                                            DescontarMontoADetalles(prods, proporcion);
                                        }
                                    }
                                    else if (regla.TipoRegla == "total")
                                    {
                                        Decimal discountAmount = regla.getAmountToDiscount();
                                        Decimal normalTotalSale = o.DetalleDeVentas.itemsVendidos.Sum(i => i.SalesLinePrice * i.SalesLineQty);
                                        Decimal discountedTotalSale = normalTotalSale - discountAmount;

                                        Decimal finalPriceProportion = discountedTotalSale / normalTotalSale;
                                        List<Detalle> prods = o.DetalleDeVentas.itemsVendidos;
                                        AplicarFactorADetalles(finalPriceProportion, prods);

                                    }
                                    else
                                    {
                                        if (regla.isPercent())
                                        {
                                            DescontarPorcentajeADetalles(regla, o.DetalleDeVentas.itemsVendidos);

                                        }
                                        else
                                        {
                                            //logica contar cantidad de prod y descontar monto al pedido 
                                            decimal cuentaProds = o.DetalleDeVentas.itemsVendidos.Sum(i => i.SalesLineQty);
                                            decimal proporcion = regla.getAmountToDiscount() / cuentaProds;
                                            DescontarMontoADetalles(o.DetalleDeVentas.itemsVendidos, proporcion);
                                        }
                                    }
                                    
                                    
                                }
                            }
                        }
                    });


                    // Esperar a que TODAS las tareas de la orden terminen antes de continuar
                    // (incluida la del pago: Webpay/Banco de Chile). Así la lista de órdenes
                    // se retorna recién cuando cada orden está completamente enriquecida.
                    List<Task> ordenTasks = new List<Task>
                    {
                        tDiscounts,
                        tComentario,
                        tEstado,
                        tCurrency,
                        tInvoiceAdd,
                        tDeliveryAddress,
                        tCustomer
                    };
                    if (tPaymentInfo != null)
                    {
                        ordenTasks.Add(tPaymentInfo);
                    }
                    Task.WaitAll(ordenTasks.ToArray());

                    if (o.Facturacion.LiteralBusiness == "Factura")
                    {
                        o.Factura = true;
                        Console.WriteLine("Compra con Factura");
                    }
                    else
                    {
                        o.Factura = false;
                        Console.WriteLine("Compra con boleta");
                    }

                    if (o.Despacho.EsRetiroEnTienda)
                    {
                        o.Despacho.ContactEmail = o.CustomerEmail;
                        o.Despacho.ContactTelephoneNumber = o.Facturacion.PhoneNumber;
                    }
                });

                Schema.dto.OrdenesResponse ordenes = new Schema.dto.OrdenesResponse();
                ordenes.SetOrdenesDeVenta(resp);
                ordenes.Traza = sbTraza.ToString();
                return ordenes;
            }
            catch (Exception e)
            {
                sbTraza.Append($"Error al obtener ordenes [{e.Message}]{Environment.NewLine}");
                return new Schema.dto.OrdenesResponse
                {
                    ErrorCode = IntegracionErrorCode.UNCONTROLATED,
                    Error = e.Message,
                    Traza = sbTraza.ToString()
                };
            }
        }



        private void AplicarFactorADetalles(decimal finalPriceProportion, List<Detalle> prods)
        {
            prods.ForEach(p =>
            {
                p.SalesLineTotal *= finalPriceProportion;
                p.SalesLinePrice *= finalPriceProportion;
                p.SalesLineNetTotalAmount *= finalPriceProportion;
                p.SalesLineNetPrice *= finalPriceProportion;
            });
            Console.WriteLine("Applied Price Factor to Details");
        }

        private List<Detalle> OpenPackDetail(Detalle product, Traza sbTraza)
        {
            ProductResponse productoPadre = psProducto.GetRecurso("products", product.ItemId, apiKey, baseUrl, sbTraza);
            List<Detalle> resultado = new List<Detalle>();

            Decimal SumNetPrice = 0;
            Decimal SumPrice = 0;

            foreach (ProductBundle pb in productoPadre.Product.Associations.product_bundle)
            {
                Decimal precio = new Decimal(0);
                precio = GetItemPrice(sbTraza, pb);
                SumNetPrice = SumNetPrice + (pb.quantity * precio);
                Console.WriteLine("PB Id: " + pb.Id + " ; cant: " + pb.quantity + " ; precio unitario Neto: " + precio + " ; Precio linea Neto: " + pb.quantity * precio);
            }
            Console.WriteLine("Precio Detalles Neto: " + SumNetPrice);

            SumPrice = SumNetPrice * IVA;

            Decimal factorPrecioPack = Functions.ParseDecimal(productoPadre.Product.Precio) / SumNetPrice;

            foreach (ProductBundle pb in productoPadre.Product.Associations.product_bundle)
            {
                ProductResponse productResp = psProducto.GetRecurso("products", pb.Id, apiKey, baseUrl, sbTraza);

                Detalle d = new Detalle();

                Decimal precioSubItem = new Decimal(0);
                precioSubItem = GetItemPrice(sbTraza, pb);

                d.ItemId = pb.Id;
                //d.Sku = productResp.Product.Reference;
                d.Sku = GetItemSku(sbTraza, pb);
                d.SalesLineQty = pb.quantity * product.SalesLineQty;
                d.SalesLineNetPrice = precioSubItem * factorPrecioPack;
                d.SalesLinePrice = d.SalesLineNetPrice * IVA;
                d.SalesLineNetTotalAmount = d.SalesLineNetPrice * d.SalesLineQty;
                d.SalesLineTotal = d.SalesLinePrice * d.SalesLineQty;
                d.Categories = productResp.Product.Associations.categories;
                
                resultado.Add(d);
            }

            return resultado;
        }

        private String GetItemSku(Traza sbTraza, ProductBundle pb)
        {
            String sku;
            if (pb.id_product_attribute != "0")
            {
                CombinationResponse combinacionResp = psCombinacion.GetRecurso("combinations", pb.id_product_attribute, apiKey, baseUrl, sbTraza);
                sku = combinacionResp.combination.Reference;
            }
            else
            {
                ProductResponse productResp = psProducto.GetRecurso("products", pb.Id, apiKey, baseUrl, sbTraza);
                sku = productResp.Product.Reference;
            }

            return sku;
        }

        private decimal GetItemPrice(Traza sbTraza, ProductBundle pb)
        {
            decimal precio;
            if (pb.id_product_attribute != "0")
            {
                CombinationResponse combinacionResp = psCombinacion.GetRecurso("combinations", pb.id_product_attribute, apiKey, baseUrl, sbTraza);
                precio = Functions.ParseDecimal(combinacionResp.combination.Precio);
            }
            else
            {
                ProductResponse productResp = psProducto.GetRecurso("products", pb.Id, apiKey, baseUrl, sbTraza);
                precio = Functions.ParseDecimal(productResp.Product.Precio);
            }

            return precio;
        }

        private static void DescontarMontoADetallesEspecificos(CartRulesResponse regla, List<Detalle> prods)
        {
            prods.ForEach(p =>
            {
                p.SalesLineTotal -= regla.getAmountToDiscount() * IVA;
                p.SalesLinePrice -= regla.getAmountToDiscount()*IVA;
                p.SalesLineNetTotalAmount -= regla.getAmountToDiscount();
                p.SalesLineNetPrice -= regla.getAmountToDiscount();
            });
        }

        private static void DescontarMontoADetalles(List<Detalle> prods, decimal proporcion)
        {
            prods.ForEach(p =>
            {
                p.SalesLineTotal -= (proporcion * p.SalesLineQty) * IVA;
                p.SalesLinePrice -= (proporcion) * IVA;
                p.SalesLineNetTotalAmount -= (proporcion * p.SalesLineQty);
                p.SalesLineNetPrice -= (proporcion);
                //Console.Write("");
            });
            return;
        }

        private static void DescontarPorcentajeADetalles(CartRulesResponse regla, List<Detalle> prods)
        {
            prods.ForEach(p =>
            {
                p.SalesLineTotal *= (100 - regla.getPercentToDiscount()) / 100;
                p.SalesLinePrice *= (100 - regla.getPercentToDiscount()) / 100;
                p.SalesLineNetTotalAmount *= (100 - regla.getPercentToDiscount()) / 100;
                p.SalesLineNetPrice *= (100 - regla.getPercentToDiscount()) / 100;
            });
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
