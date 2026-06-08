using IntegracionEnviame.API;
using IntegracionEnviame.Schema.Request;
using IntegracionPrestashopLib.Operations;
using IntegracionPrestashopLib.Schema.ERP;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;
using IntegracionEnviame.Schema.Requests;
using IntegracionEnviame.Schema.Responses;

namespace Test
{
    class Program
    {
        private static OrdenDeVenta ord;

        private 

        static void AxSymTest()
        {

            IntegracionPrestashopLib.Operations.OrdenDAL oApiOrdenDal;
            IntegracionPrestashopLib.Schema.dto.OrdenesResponse oOrdenes;
            IntegracionPrestashopLib.Schema.ERP.OrdenDeVenta oSalesOrder;
            IntegracionPrestashopLib.Schema.ERP.Comentarios oComentarios;
            IntegracionPrestashopLib.Schema.ERP.Comentario oComentario;
            IntegracionPrestashopLib.Schema.ERP.DetalleDeVenta oSalesLines;
            IntegracionPrestashopLib.Schema.ERP.Facturacion oFacturacion;
            IntegracionPrestashopLib.Schema.ERP.Direccion oInvoiceAddress;
            IntegracionPrestashopLib.Schema.ERP.Direccion oDeliveryAddress;
            IntegracionPrestashopLib.Schema.ERP.Despacho oDelivery;
            IntegracionPrestashopLib.Schema.ERP.Detalle oSalesLine;
            IntegracionPrestashopLib.Schema.ERP.PaymentInfo oTransbankInfo;

            int iterOrders;
            long salesOrderId;

            String custAccount;
            String custName;
            String invoiceAddress;
            String invoiceCity;
            String invoiceCountyId;
            Boolean storePickUp;
            String deliveryAddress;
            String deliveryCity;
            String deliveryCountyId;

            String contactName;
            String contactEmail;
            String contactPhone;

            String customerRef;

            String strSku;

            int numLines;
            int iterLines;

            Decimal listPrice;
            Decimal netPrice;
            Decimal lineTotal;
            Decimal lineQty;


            String eShopApiKey = "X65SKAWDWMBK4QY5B58Q4ABE9EYPW54F";//dap ducasse
            //String eShopApiKey = "PWYRQDRTIQX125QYBZSI5AV91B32YCNR"; //stock insumos
            //String eShopUri = "https://intermedio.dapducasse.cl/api/";
            //String eShopUri = "https://dapducasse.cl/api/";
            //String eShopUri = "https://dapducasse.cl/api/";
            String eShopUri = "https://bch.dapducasse.cl/api/";
            //String eShopUri = "https://stockinsumos.cl/api/";
            int eShopUser = 8;

            oApiOrdenDal = new IntegracionPrestashopLib.Operations.OrdenDAL(eShopApiKey, eShopUri, eShopUser);

            DateTime _startDateTime = DateTime.Parse("2026-06-06 00:05");
            DateTime _endDateTime = DateTime.Parse("2026-06-07 23:55");
            


            oOrdenes = oApiOrdenDal.GetOrdenes(_startDateTime, _endDateTime);
            int errorCode = oOrdenes.ErrorCode;
            int numOrders = oOrdenes.GetSize();

            for (iterOrders = 0; iterOrders < numOrders; iterOrders++)
            {
                try
                {
                    //- Set object values to local variables
                    oSalesOrder = oOrdenes.GetOrden(iterOrders);

                    decimal sumaneto = 0;
                    decimal sumatotal = 0;

                    for (int prodI = 0; prodI < oSalesOrder.DetalleDeVentas.GetSize(); prodI++)
                    {

                        sumaneto += oSalesOrder.DetalleDeVentas.GetItemDetalle(prodI).SalesLineNetTotalAmount;
                        sumatotal += oSalesOrder.DetalleDeVentas.GetItemDetalle(prodI).SalesLineTotal;
                    }
                    Console.WriteLine(sumaneto.ToString());
                    Console.WriteLine(sumatotal.ToString());    

                    salesOrderId = oSalesOrder.SalesIdPrestaShop;

                    String strPrestaShopRef = oSalesOrder.Referencia;
                    DateTime salesDateTime = oSalesOrder.SalesDate;
                    Boolean isFactura = oSalesOrder.Factura;
                    Decimal totalNetAmount = oSalesOrder.TotalNetAmount;
                    Decimal totalAmount = oSalesOrder.TotalAmount;
                    Decimal totalTax = oSalesOrder.TaxAmount;
                    Decimal shippingCost = oSalesOrder.NetShippingAmount;
                    //- Transaction Code
                    //oTransbankInfo = oSalesOrder.TransactionInfo;
                    //String strTransactionCode = oTransbankInfo.BuyOrder;
                    //- Comentarios

                    oComentarios = oSalesOrder.Mensajes;
                    int numComments = oComentarios.GetSize();
                    if (numComments > 0)
                    {
                        oComentario = oComentarios.GetMensaje(0);
                        customerRef = oComentario.Msg;
                    }
                    //- Invoice data
                    oFacturacion = oSalesOrder.Facturacion;

                    if (isFactura)
                    {
                        custAccount = oFacturacion.Rut;
                        //custAccount = strRem(custAccount, '.');
                        //custAccount = strRem(custAccount, '-');
                        //custAccountSd = substr(custAccount, strLen(custAccount), 1);
                        //custAccount = substr(custAccount, 1, strLen(custAccount) - 1);
                        custName = oFacturacion.CustNameInvoice;
                        String strLiteralBusiness = oFacturacion.LiteralBusiness;
                    }
                    else
                    {
                        custAccount = "777777777";
                        //custAccountSd = DUCValidateSecurityDigit::ValidateDigit(custAccount);
                        custName = "Nombre por defecto";//CustTable::find(custAccount).Name;
                    }
                    //- Invoice Address
                    oInvoiceAddress = oFacturacion.Direccion;
                    invoiceAddress = oInvoiceAddress.AddressStreet;
                    invoiceCity = oInvoiceAddress.AddressCity;
                    invoiceCountyId = oInvoiceAddress.AddressCounty;
                    //- Delivery Address
                    oDelivery = oSalesOrder.Despacho;
                    storePickUp = oDelivery.EsRetiroEnTienda;
                    oDeliveryAddress = oDelivery.Direccion;
                    deliveryAddress = oDeliveryAddress.AddressStreet;
                    deliveryCity = oDeliveryAddress.AddressCity;
                    deliveryCountyId = oDeliveryAddress.AddressCounty;
                    //- Contact
                    if (storePickUp)
                    {
                        contactName = oSalesOrder.CustomerName;
                        contactEmail = oSalesOrder.CustomerEmail;
                        contactPhone = oFacturacion.PhoneNumber;
                    }
                    else
                    {
                        contactName = oDelivery.CustName;
                        contactEmail = oDelivery.ContactEmail;
                        contactPhone = oDelivery.ContactTelephoneNumber;
                    }
                    customerRef = contactName + '-' + contactPhone;

                    //- Create Sales Order Details
                    oSalesLines = oSalesOrder.DetalleDeVentas;
                    numLines = oSalesLines.GetSize();
                    for (iterLines = 0; iterLines < numLines; iterLines++)
                    {
                        //- Set object values
                        oSalesLine = oSalesLines.GetItemDetalle(iterLines);
                        strSku = oSalesLine.Sku;
                        String itemId = oSalesLine.ItemId;
                        //strSku = strRem(strSku, ' ');
                        //strSku = '.' + strSku;
                        //conSku = str2con(strSku, '-');
                        //itemId = conPeek(conSku, 1);
                        //itemId = strRem(itemId, '.');
                        //- Setting InventDimId
                        //inventDimId = conPeek(conSku, 2);
                        //- Amount
                        listPrice = oSalesLine.SalesLineNetPrice;
                        netPrice = oSalesLine.SalesLineNetPrice;
                        lineTotal = oSalesLine.SalesLineNetTotalAmount;
                        lineQty = oSalesLine.SalesLineQty;

                        Console.WriteLine("SKU: " + strSku + " | listPrice: " + listPrice + " | netPrice: " + netPrice + " | lineTotal: " + lineTotal + " | lineQty: " + lineQty);

                        //- Create line
                        //eSalesLines.clear();
                        //eSalesLines.eSalesId = eSalesTable.eSalesId;
                        //eSalesLines.eDimension04 = InventTable::find(itemId).Dimension[4];
                        //- Set InventLocationId
                        /*
                        if (curExt() == 'CMD')
                        {
                            switch (eSalesLines.eDimension04)
                            {
                                case '01','02','03','04','81':
                            eSalesLines.eInventLocationId = '1201';
                                    break;
                                case '1104','1201','1203','1204','1205','1206','1401','1601':
                            eSalesLines.eInventLocationId = '1203-CWD';
                                    break;
                                case '05','06','G5':
                            eSalesLines.eInventLocationId = '1201-DCO';
                                    break;
                                case '99':
                                    break;
                                default:
                                    eSalesLines.eInventLocationId = '1201-CWD';
                                    break;
                            }
                        }
                        else
                        {
                            eSalesLines.eInventLocationId = '1901';
                        }
                        */
                        //curInventLocationId = eSalesLines.eInventLocationId;
                        //eSalesLines.eItemId = itemId;
                        //eSalesLines.eConfigId = InventDim::find(conPeek(conSku, 2)).configId;
                        //eSalesLines.eSizeId = InventDim::find(conPeek(conSku, 2)).InventSizeId;
                        //eSalesLines.eColorId = InventDim::find(conPeek(conSku, 2)).InventColorId;
                        //-
                        //eSalesLines.eInventDimId = inventDimId;
                        //eSalesLines.eCurrencyCode = CompanyInfo::find().CurrencyCode;
                        //eSalesLines.ePriceList = listPrice;
                        //eSalesLines.eLineNetTotal = netPrice;
                        //eSalesLines.eLineTotal = lineTotal;
                        //eSalesLines.eSalesQty = lineQty;
                        //- Values
                        //eSalesLines.insert();
                    }
                    //- Create line for Shipping Cost.
                    /*
                    if (shippingCost > 0 && eSalesLines)
                    {
                        //- Create line
                        eSalesLines.clear();
                        eSalesLines.eSalesId = eSalesTable.eSalesId;
                        eSalesLines.eItemId = '9900000005';
                        eSalesLines.eDimension04 = InventTable::find(eSalesLines.eItemId).Dimension[4];
                        eSalesLines.eInventDimId = InventDim::inventDimIdBlank();
                        eSalesLines.eInventLocationId = curInventLocationId;
                        eSalesLines.eCurrencyCode = CompanyInfo::find().CurrencyCode;
                        eSalesLines.ePriceList = shippingCost;
                        eSalesLines.eLineNetTotal = shippingCost;
                        eSalesLines.eLineTotal = shippingCost;
                        eSalesLines.eSalesQty = 1;
                        //- Values
                        eSalesLines.insert();
                    }
                    */
                    //- Create sales table
                    /*
                    lineNum = 1;
                    curLineBusiness = '';
                    curInventLocationId = '';
                    if (eSalesTable && eSalesLines)
                    {
                        while select eSalesLines order by eSalesLines.eInventLocationId,eSalesLines.eLineNum
                            where       eSalesLines.eSalesId == eSalesTable.eSalesId
                        {
                            if (curInventLocationId != eSalesLines.eInventLocationId)
                            {
                                if (curInventLocationId != '')
                                {
                                    //- Sachip calc
                                    if (!args)
                                    {
                                        args = new args();
                                    }
                                    args.record(salesTable);
                                    SalesShipCalc::main(args);
                                    //- Create picking.
                                    //this.createSalesPickingList(salesTable);
                                }
                                curInventLocationId = eSalesLines.eInventLocationId;
                                salesTable = eSalesImport::createSalesTable(eSalesTable, curInventLocationId);
                                //-
                                eSalesListSalesId.clear();
                                eSalesListSalesId.eSalesTable = eSalesTable.RecId;
                                eSalesListSalesId.SalesId = salesTable.SalesId;
                                eSalesListSalesId.insert();
                                lineNum = 1;
                            }
                            //- Validate if exist onhand available main warehouse
                            curQty = this.currentOnHand(eSalesLines.eItemId, inventDimId, curInventLocationId);
                            if (curQty < eSalesLines.eSalesQty
                               &&
                               InventTable::find(eSalesLines.eItemId).ItemType != ItemType::Service
                               &&
                               curInventLocationId == '1201-DCO')
                            {
                                if (this.currentOnHand(eSalesLines.eItemId, eSalesLines.eInventDimId, 'LAM01-EC01') >= eSalesLines.eSalesQty - curQty)
                                {
                                    //this.createTransferJour(eSalesLines.eItemId,eSalesLines.eInventDimId,'LAM01-EC01',curInventLocationId,eSalesLines.eSalesQty - curQty,salesTable.Dimension);
                                }
                            }
                            //-
                            salesLine = eSalesImport::createSalesLine(salesTable, eSalesLines, eSalesLines.eInventLocationId, lineNum);
                            lineNum++;
                        }
                        if (!args)
                        {
                            args = new args();
                        }
                        if (salesTable)
                        {
                            args.record(salesTable);
                            SalesShipCalc::main(args);
                            //- Create picking
                            this.createSalesPickingList(salesTable);
                        }
                    }
                    ttscommit;
                    */
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.StackTrace);
                }
            }

            Console.WriteLine("Finished");


        }


        static void testEnviame()
        {
            Operaciones enviame1 = new Operaciones("e929b266be19863968b5f9253a81d14c", "https://api.enviame.io/api/s2/v2/", 3539);

            IntegracionEnviame.Schema.Responses.VerTrackingResponse tracking = enviame1.VerTracking(124442754);

            CrearEnvioRequest oSendRequest = new IntegracionEnviame.Schema.Requests.CrearEnvioRequest();
            Destino oDestino = new IntegracionEnviame.Schema.Requests.Destino();
            Orden oOrden = new IntegracionEnviame.Schema.Requests.Orden();
            
            //- Orden
            oOrden.CantidadBultos = 1;
            oOrden.CodBodegaOrigen = "1201-CWD";
            oOrden.DescripcionContenido = "Material para terminaciones";
            oOrden.IdReferencia = "AAA";
            oOrden.Set_PesoTotal(0, 0);
            oOrden.Set_VolumenTotal(0,0);
            oOrden.Precio = 15000;
            //- Destino
            oDestino.Comuna = "Las Condes";
            oDestino.DireccionCompleta = "Apoquindo 5555";
            oDestino.EmailReceptor = "test@test.com";
            oDestino.NombreReceptor = "Juanito Perez";
            oDestino.Informacion = "Informacion";
            oDestino.TelefonoReceptor = "+56984418520";

            //-
            String serviceType = "nextday";
            oSendRequest.Carrier = "CHX";
            oSendRequest.CarrierService = serviceType;
            oSendRequest.Destino = oDestino;
            oSendRequest.Orden = oOrden;
            //- Create delivery
            CrearEnvioResponse oSendResponse = enviame1.CrearEnvio(oSendRequest);
            Console.WriteLine(oSendResponse.TrackingNumber);
            
        }
        
        
        static void Main(string[] args)
        {
            try
            {

                //testEnviame();
                //return;
                //Operaciones enviame1 = new Operaciones("e929b266be19863968b5f9253a81d14c", "https://api.enviame.io/api/s2/v2/", 3539);
                //  IntegracionEnviame.Schema.Responses.VerTrackingResponse tracking = enviame1.VerTracking(124442754);



                //IntegracionPrestashopLib.Operations.OrdenDAL oOrdenDAL = new IntegracionPrestashopLib.Operations.OrdenDAL("X65SKAWDWMBK4QY5B58Q4ABE9EYPW54F", "https://dapducasse.cl/api/", 8);
                IntegracionPrestashopLib.Operations.OrdenDAL oOrdenDAL = new IntegracionPrestashopLib.Operations.OrdenDAL("PWYRQDRTIQX125QYBZSI5AV91B32YCNR", "http://stockinsumos.cl/api/", 8);
                //IntegracionPrestashopLib.Operations.ProductoDAL oProductoDAL = new IntegracionPrestashopLib.Operations.ProductoDAL("X65SKAWDWMBK4QY5B58Q4ABE9EYPW54F", "https://dapducasse.cl/api/");
                IntegracionPrestashopLib.Operations.ProductoDAL oProductoDAL = new IntegracionPrestashopLib.Operations.ProductoDAL("PWYRQDRTIQX125QYBZSI5AV91B32YCNR", "http://stockinsumos.cl/api/");

                Program pr = new Program();

                AxSymTest();
                return;

                Console.WriteLine(oProductoDAL.TestDateTime(DateTime.Now));

                //IntegracionPrestashopLib.Schema.dto.OrdenesResponse ordenes = oOrdenDAL.GetOrdenes(DateTime.Parse("2023-01-04 16:00"), DateTime.Parse("2023-01-04 16:30")); // intermedio dap kits
                //IntegracionPrestashopLib.Schema.dto.OrdenesResponse ordenes = oOrdenDAL.GetOrdenes(DateTime.Parse("2023-04-19 16:39"), DateTime.Parse("2023-04-19 16:45")); // intermedio dap NO kits
                IntegracionPrestashopLib.Schema.dto.OrdenesResponse ordenes = oOrdenDAL.GetOrdenes(DateTime.Parse("2024-09-28 11:30"), DateTime.Parse("2024-10-17 17:00"));
                




                /*
                Console.WriteLine(oProductoDAL.TestDateTime(DateTime.Now));

                Console.WriteLine($"La lista de ordenes contiene {ordenes.GetSize()} filas");

                for (int x=0; x < ordenes.GetSize();x++)
                {
                    Console.WriteLine($"Pedido {x+1} tiene nº de orden {ordenes.GetOrden(x).SalesIdPrestaShop} con {ordenes.GetOrden(x).DetalleDeVentas.GetSize()} lineas de detalle");
                    Console.WriteLine($"Linea 1 cantidad = {ordenes.GetOrden(x).DetalleDeVentas.GetItemDetalle(0).SalesLineQty}");
                    Console.WriteLine($"Linea 1 SKU = {ordenes.GetOrden(x).DetalleDeVentas.GetItemDetalle(0).Sku}");
                    OrdenDeVenta ord = ordenes.GetOrden(x);
                    Console.WriteLine($"Orden {x}: {JsonConvert.SerializeObject(ord)}");

                    Console.WriteLine($"Orden {x} contiene los siguientes {ord.DetalleDeVentas.GetSize()} items:\n\n");
                    for(int i = 0; i<ord.DetalleDeVentas.GetSize(); i++)
                    {
                        Detalle detalle = ord.DetalleDeVentas.GetItemDetalle(i);
                        Console.WriteLine($"Detalle {i}: {JsonConvert.SerializeObject(detalle)}");
                        Console.WriteLine("----");

                    }
                    {

                    }
                    Console.WriteLine($"Orden {x}: {JsonConvert.SerializeObject(ord)}");

                }
                */

                //try
                //{
                //    var resp = oOrdenDAL.Update(new Tracking(6, 4));
                //    Console.WriteLine(resp.Actualizado.ToString());
                //}catch(Exception e)
                //{
                //    Console.WriteLine(e.Message);
                //}
                

                StringBuilder sb = new StringBuilder(Environment.NewLine);
                List<String> productos = new List<string>();
                
                
                //productos.Add("050501104700-00037892_048");
                //productos.Add("052402033600-00037919_048");//Producto hijo de combinacion
                productos.Add("050502021770-00985083_048");//Producto hijo de combinacion
                //productos.Add("050501095600-00037893_048");//Producto hijo de combinacion
                //productos.Add("04P0220005-00346346_048"); //producto sin imagen de categoria
                //productos.Add("050501105200-00037893_048");
                //productos.Add("050501105200-00037893_048");
                //productos.Add("050201001940-00037858_048"); //primer item de quincalleria
                //productos.Add("050102006200-00037852_048"); //Producto no existe?

                int count = 0;
                foreach (var inventDimId in productos)
                {
                    var stock = oProductoDAL.GetStock(inventDimId);
                    sb.Append($"Stock inicial de {inventDimId} es {stock.Inventario.Stock}{Environment.NewLine}");
                    var inventario = new Inventario
                    {
                        ItemId_InventDimId = inventDimId,
                        Stock = count
                    };

                    var response = oProductoDAL.UpdateStock(inventario);
                    if (response.Actualizado)
                        sb.Append($"Se actualizó el producto {inventDimId} que tenía {stock.Inventario.Stock} y queda en {inventario.Stock}{Environment.NewLine}");
                    else
                        sb.Append($"No se actualizó el producto {inventDimId} que tenía {stock.Inventario.Stock} y debia quedar en {inventario.Stock}{Environment.NewLine}");

                    stock = oProductoDAL.GetStock(inventDimId);
                    sb.Append($"Stock final de {inventDimId} es {stock.Inventario.Stock}{Environment.NewLine}{Environment.NewLine}");
                    //count += 2;
                }
                Console.WriteLine(sb.ToString());
                return;
                /**
                 * ENVIAME
                 *
                 */
                
                Operaciones enviame = new Operaciones("e929b266be19863968b5f9253a81d14c", "https://api.enviame.io/api/s2/v2/", 3539);

                var envio = enviame.CrearEnvio(new IntegracionEnviame.Schema.Requests.CrearEnvioRequest
                {
                    Carrier = "WSP",
                    CarrierService = "",
                    Destino = new IntegracionEnviame.Schema.Requests.Destino
                    {
                        Comuna = "villarrica",
                        DireccionCompleta = "ruta 199 km 3 y medio, lomas de villarrica parcela 48 tercera etapa Lomas de santa lucia. Esta al final del condominio",
                        EmailReceptor = "ivan.jerez@gmail.com",
                        Informacion = "TEST",
                        NombreReceptor = "Joaquin Jerez Martinez",
                        TelefonoReceptor = "223557050"
                    },
                    Orden = new IntegracionEnviame.Schema.Requests.Orden
                    {
                        CantidadBultos = 2,
                        CodBodegaOrigen = "LAM01-EC01XX",
                        DescripcionContenido = "Material para terminaciones",
                        IdReferencia = "972",
                        Precio = 146483
                    }
                });

                Console.WriteLine(oProductoDAL.TestDateTime(DateTime.Now));
                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
                
        }
    }
}
