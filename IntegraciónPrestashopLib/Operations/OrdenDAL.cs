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
using System.Text;
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
            StringBuilder sbTraza = new StringBuilder();
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
                var response = PSOrdenClient.GetOrdenes(fechaInicio, fechaFin, apiKey, baseUrl, ref sbTraza);
                var ordenesList = response.Orders;
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
                                TotalDicount = oc.TotalDiscounts,
                                DetalleDeVentas = new DetalleDeVenta((from d in oc.Asociados.productos
                                                                      select new Detalle
                                                                      {
                                                                          ItemId = d.ProductId,
                                                                          Sku = d.Reference,
                                                                          SalesLineQty = Decimal.Parse(d.Cantidad.Replace(".", ",")),
                                                                          SalesLineNetPrice = Decimal.Parse(d.PrecioSinImpuesto.Replace(".", ",")), 
                                                                          SalesLinePrice = Decimal.Parse(d.PrecioConImpuesto.Replace(".", ",")),
                                                                          SalesLineNetTotalAmount = Decimal.Parse(d.PrecioSinImpuesto.Replace(".", ",")) * Decimal.Parse(d.Cantidad.Replace(".", ",")),
                                                                          SalesLineTotal = Decimal.Parse(d.PrecioConImpuesto.Replace(".", ",")) * Decimal.Parse(d.Cantidad.Replace(".", ","))
                                                                      }).ToList()),
                                EstadoId = oc.CurrentState
                            }).ToList<OrdenDeVenta>();

                List<Task> tasks = new List<Task>();
                resp.ForEach(o =>
                {

                    Task tProducts = Task.Run(() =>
                    {
                        var itemsTemp = o.DetalleDeVentas.itemsVendidos;
                        o.DetalleDeVentas.itemsVendidos = new List<Detalle>();

                        foreach (var prod in itemsTemp)
                        {
                            sbTraza.Append($"Revisando si {prod.ItemId} es un producto compuesto, para orden {o.SalesIdPrestaShop} {Environment.NewLine}");
                            var productResp = psProducto.GetRecurso("products", prod.ItemId, apiKey, baseUrl, ref sbTraza);
                            if (productResp != null)
                            {
                                if(productResp.Product.CacheIsPack != "1")
                                {
                                    o.DetalleDeVentas.itemsVendidos.Add(prod);
                                    prod.Categories = productResp.Product.Associations.categories;
                                }
                                else
                                {
                                    var openedItemsTemp = openPackDetail(prod, sbTraza);
                                    foreach(var item in openedItemsTemp)
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
                        var estadoResp = pSEstado.GetRecurso("order_states", o.EstadoId, apiKey, baseUrl, ref sbTraza);
                        if (estadoResp != null)
                        {
                            var estado = estadoResp.estado;
                            o.Estado = estado.Nombres;
                        }
                    });

                    Task tCustomer = Task.Run(() =>
                    {
                        sbTraza.Append($"Iniciando búsqueda de cliente para orden {o.SalesIdPrestaShop} {Environment.NewLine}");
                        var customerResp = psCustomer.GetRecurso("customers", o.CustomerId, apiKey, baseUrl, ref sbTraza);
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
                        var currencyRep = psCurrency.GetRecurso("currencies", o.CurrencyId, apiKey, baseUrl, ref sbTraza);
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
                        Console.WriteLine($"Iniciando búsqueda de direccion de factura para orden {o.SalesIdPrestaShop} {Environment.NewLine}");
                        sbTraza.Append($"Iniciando búsqueda de direccion de factura para orden {o.SalesIdPrestaShop} {Environment.NewLine}");
                        var invoiceAddressResp = pSDelivery.GetRecurso("addresses", o.Facturacion.Direccion.AddressId, apiKey, baseUrl, ref sbTraza);
                        if (invoiceAddressResp != null)
                        {
                            var invoiceAddress = invoiceAddressResp.Address;
                            o.Facturacion.Direccion.AddressCity = invoiceAddress.Ciudad;
                            o.Facturacion.Direccion.AddressStreet = invoiceAddress.Direccion1;
                            o.Facturacion.PhoneNumber = invoiceAddress.Phone;
                            o.Facturacion.CustNameInvoice = invoiceAddress.Company == "" ? $"{invoiceAddress.Firstname} {invoiceAddress.Lastname}" : invoiceAddress.Company;
                            o.Facturacion.Rut = invoiceAddress.RutEmpresa.Trim();
                            o.Facturacion.LiteralBusiness = invoiceAddress.Other;





                            Task tState = Task.Run(() =>
                            {
                                sbTraza.Append($"Iniciando búsqueda de estado para dirección {o.Facturacion.Direccion.AddressId} de orden {o.SalesIdPrestaShop} {Environment.NewLine}");
                                var comunaResp = psState.GetRecurso("states", invoiceAddress.StateId, apiKey, baseUrl, ref sbTraza);

                                if (comunaResp != null)
                                {
                                    string codComuna = GetStringDecimalFromHexa(comunaResp.State.StateCode);
                                    o.Facturacion.Direccion.AddressCounty = codComuna;
                                }
                            });

                            Task tCustomDel = Task.Run(() =>
                            {
                                sbTraza.Append($"Iniciando búsqueda de cliente en direcccion {o.Facturacion.Direccion.AddressId} para orden {o.SalesIdPrestaShop} {Environment.NewLine}");
                                var customerDeliveryResp = psCustomer.GetRecurso("customers", invoiceAddress.CustomerId, apiKey, baseUrl, ref sbTraza);
                                if (customerDeliveryResp != null)
                                {
                                    var customerDelivery = customerDeliveryResp.Clientes;
                                    o.Facturacion.EmailInvoice = customerDelivery.Email;
                                }


                            });
                            /*Task tDoc = Task.Run(() =>
                            {
                                sbTraza.Append($"Iniciando búsqueda de datos facturacion para orden {o.SalesIdPrestaShop} {Environment.NewLine}");
                                String url = baseUrl.Replace("api", "WS");

                                if (invoiceAddress.Other == "Factura")
                                {
                                    o.Factura = true;
                                    Console.WriteLine("Compra con Factura");

								}
                                else
                                {
                                    o.Factura = false;
                                    Console.WriteLine("Compra con boleta");
                                }

							});*/
                            tState.Wait();
                            tCustomDel.Wait();



                            //tDoc.Wait();
                        }
                    });

                    Task tPayments = Task.Run(() =>
                    {
                        sbTraza.Append($"Iniciando búsqueda de datos transbank para orden {o.SalesIdPrestaShop} {Environment.NewLine}");
                        var urlPayment = $"{baseUrl.Replace("api/", "WS/webpayinfo.php")}";

                        var parametros = new Dictionary<string, string>();
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
                        sbTraza.Append($"Iniciando búsqueda de direccion de despacho para orden {o.SalesIdPrestaShop} {Environment.NewLine}");
                        var deliveryAddressResp = pSDelivery.GetRecurso("addresses", o.Despacho.Direccion.AddressId, apiKey, baseUrl, ref sbTraza);
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
                                var stateResp = psState.GetRecurso("states", deliveryAddress.StateId, apiKey, baseUrl, ref sbTraza);
                                if (stateResp != null)
                                    o.Despacho.Direccion.AddressCounty = GetStringDecimalFromHexa(stateResp.State.StateCode);
                            });

                            Task tCustomDel = Task.Run(() =>
                            {
                                sbTraza.Append($"Iniciando búsqueda de cliente para direccion {o.Despacho.Direccion.AddressId} para orden {o.SalesIdPrestaShop} {Environment.NewLine}");
                                var customerDeliveryResp = psCustomer.GetRecurso("customers", deliveryAddress.CustomerId, apiKey, baseUrl, ref sbTraza);
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
                        sbTraza.Append($"Iniciando búsqueda de comentarios para orden {o.SalesIdPrestaShop} {Environment.NewLine}");
                        var comentariosResponse = psMessage.GetRecursoWithFilter("messages", $"{o.SalesIdPrestaShop}", apiKey, baseUrl, $"&filter[id_order]=[{o.SalesIdPrestaShop}]&display=full", ref sbTraza);
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
                            var urlPayment = $"{baseUrl.Replace("api/", "WS/cart_cart_rules.php")}";

                            var parametros = new Dictionary<String, String>();
                            parametros.Add("id_cart", o.IdCart);

                            int idCart = 0;
                            if (int.TryParse(o.IdCart, out idCart))
                            {
                                decimal signature = (17) + (idCart * 17);
                                parametros.Add("signature", signature.ToString().Replace(",", "."));

                                var reglasAplicadas = PSOrdenClient.GetCartRules(urlPayment, parametros);
                                foreach (var regla in reglasAplicadas)
                                {
                                    if (regla.free_shipping == "1")
                                    {
                                        Console.WriteLine("Free Shipping");
                                        o.NetShippingAmount = 0;
                                        o.TotalShippingAmount = 0;
                                    }
                                    if (regla.TipoRegla == "categories")
                                        {
                                            var prods = o.DetalleDeVentas.itemsVendidos.Where(i => i.Categories.Any(c => c.Id == regla.id_item)).ToList();
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
                                        var prods = o.DetalleDeVentas.itemsVendidos.Where(i => i.ItemId == regla.id_item).ToList();
                                        if (regla.isPercent())
                                        {
                                            DescontarPorcentajeADetalles(regla, prods);
                                            
                                        }
                                        else
                                        {
                                            //logica contar cantidad de prod y descontar monto al pedido 
                                            var cuentaProds = o.DetalleDeVentas.itemsVendidos.Sum(i => i.SalesLineQty);
                                            var proporcion = regla.getAmountToDiscount() / cuentaProds;
                                            DescontarMontoADetalles(prods, proporcion);
                                        }
                                    }
                                    else if (regla.TipoRegla == "total")
                                    {
                                        Decimal discountAmount = regla.getAmountToDiscount();
                                        Decimal normalTotalSale = o.DetalleDeVentas.itemsVendidos.Sum(i => i.SalesLinePrice * i.SalesLineQty);
                                        Decimal discountedTotalSale = normalTotalSale - discountAmount;

                                        Decimal finalPriceProportion = discountedTotalSale / normalTotalSale;
                                        var prods = o.DetalleDeVentas.itemsVendidos;
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
                                            var cuentaProds = o.DetalleDeVentas.itemsVendidos.Sum(i => i.SalesLineQty);
                                            var proporcion = regla.getAmountToDiscount() / cuentaProds;
                                            DescontarMontoADetalles(o.DetalleDeVentas.itemsVendidos, proporcion);
                                        }
                                    }
                                    
                                    
                                }
                            }
                        }
                    });


                    tDiscounts.Wait();
                    tComentario.Wait();
                    tEstado.Wait();
                    tCurrency.Wait();
                    tInvoiceAdd.Wait();
                    tDeliveryAddress.Wait();
                    tCustomer.Wait();

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

                var ordenes = new Schema.dto.OrdenesResponse();
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

        private List<Detalle> openPackDetail(Detalle product, StringBuilder sbTraza)
        {
            ProductResponse productoPadre = psProducto.GetRecurso("products", product.ItemId, apiKey, baseUrl, ref sbTraza);
            var resultado = new List<Detalle>();

            Decimal SumNetPrice = 0;
            Decimal SumPrice = 0;

            foreach (var pb in productoPadre.Product.Associations.product_bundle)
            {
                Decimal precio = new Decimal(0);
                precio = getItemPrice(ref sbTraza, pb);
                SumNetPrice = SumNetPrice + (pb.quantity * precio);
                Console.WriteLine("PB Id: " + pb.Id + " ; cant: " + pb.quantity + " ; precio unitario Neto: " + precio + " ; Precio linea Neto: " + pb.quantity * precio);
            }
            Console.WriteLine("Precio Detalles Neto: " + SumNetPrice);

            SumPrice = SumNetPrice * new Decimal(1.19);

            Decimal factorPrecioPack = Decimal.Parse(productoPadre.Product.Precio.Replace(".", ",")) / SumNetPrice;

            foreach (var pb in productoPadre.Product.Associations.product_bundle)
            {
                var productResp = psProducto.GetRecurso("products", pb.Id, apiKey, baseUrl, ref sbTraza);

                Detalle d = new Detalle();

                Decimal precioSubItem = new Decimal(0);
                precioSubItem = getItemPrice(ref sbTraza, pb);

                d.ItemId = pb.Id;
                //d.Sku = productResp.Product.Reference;
                d.Sku = getItemSKU(ref sbTraza, pb);
                d.SalesLineQty = pb.quantity * product.SalesLineQty;
                d.SalesLineNetPrice = precioSubItem * factorPrecioPack;
                d.SalesLinePrice = d.SalesLineNetPrice * new Decimal(1.19);
                d.SalesLineNetTotalAmount = d.SalesLineNetPrice * d.SalesLineQty;
                d.SalesLineTotal = d.SalesLinePrice * d.SalesLineQty;
                d.Categories = productResp.Product.Associations.categories;
                
                resultado.Add(d);
            }

            return resultado;
        }

        private String getItemSKU(ref StringBuilder sbTraza, ProductBundle pb)
        {
            String sku;
            if (pb.id_product_attribute != "0")
            {
                CombinationResponse combinacionResp = psCombinacion.GetRecurso("combinations", pb.id_product_attribute, apiKey, baseUrl, ref sbTraza);
                sku = combinacionResp.combination.Reference;
            }
            else
            {
                var productResp = psProducto.GetRecurso("products", pb.Id, apiKey, baseUrl, ref sbTraza);
                sku = productResp.Product.Reference;
            }

            return sku;
        }

        private decimal getItemPrice(ref StringBuilder sbTraza, ProductBundle pb)
        {
            decimal precio;
            if (pb.id_product_attribute != "0")
            {
                var combinacionResp = psCombinacion.GetRecurso("combinations", pb.id_product_attribute, apiKey, baseUrl, ref sbTraza);
                precio = Decimal.Parse(combinacionResp.combination.Precio.Replace(".", ","));
            }
            else
            {
                var productResp = psProducto.GetRecurso("products", pb.Id, apiKey, baseUrl, ref sbTraza);
                precio = Decimal.Parse(productResp.Product.Precio.Replace(".", ","));
            }

            return precio;
        }

        private static void DescontarMontoADetallesEspecificos(CartRulesResponse regla, List<Detalle> prods)
        {
            prods.ForEach(p =>
            {
                p.SalesLineTotal -= regla.getAmountToDiscount() * 1.19m;
                p.SalesLinePrice -= regla.getAmountToDiscount()*1.19m;
                p.SalesLineNetTotalAmount -= regla.getAmountToDiscount();
                p.SalesLineNetPrice -= regla.getAmountToDiscount();
            });
        }

        private static void DescontarMontoADetalles(List<Detalle> prods, decimal proporcion)
        {
            prods.ForEach(p =>
            {
                p.SalesLineTotal -= (proporcion * p.SalesLineQty) * 1.19m;
                p.SalesLinePrice -= (proporcion) * 1.19m;
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
