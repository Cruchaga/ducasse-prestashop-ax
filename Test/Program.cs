using IntegracionEnviame.API;
using IntegracionPrestashopLib.Schema.ERP;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Test
{
    class Program
    {
        private static OrdenDeVenta ord;

        static void Main(string[] args)
        {
            try
            {
                IntegracionPrestashopLib.Operations.OrdenDAL oOrdenDAL = new IntegracionPrestashopLib.Operations.OrdenDAL("X65SKAWDWMBK4QY5B58Q4ABE9EYPW54F", "https://intermedio.dapducasse.cl/api/", 8);
                //IntegracionPrestashopLib.Operations.OrdenDAL oOrdenDAL = new IntegracionPrestashopLib.Operations.OrdenDAL("PWYRQDRTIQX125QYBZSI5AV91B32YCNR", "http://stockinsumos.cl/api/", 8);
                IntegracionPrestashopLib.Operations.ProductoDAL oProductoDAL = new IntegracionPrestashopLib.Operations.ProductoDAL("X65SKAWDWMBK4QY5B58Q4ABE9EYPW54F", "https://intermedio.dapducasse.cl/api/");
                //IntegracionPrestashopLib.Operations.ProductoDAL oProductoDAL = new IntegracionPrestashopLib.Operations.ProductoDAL("PWYRQDRTIQX125QYBZSI5AV91B32YCNR", "http://stockinsumos.cl/api/");
                Console.WriteLine(oProductoDAL.TestDateTime(DateTime.Now));

                //IntegracionPrestashopLib.Schema.dto.OrdenesResponse ordenes = oOrdenDAL.GetOrdenes(DateTime.Parse("2023-01-04 16:00"), DateTime.Parse("2023-01-04 16:30")); // intermedio dap kits
                IntegracionPrestashopLib.Schema.dto.OrdenesResponse ordenes = oOrdenDAL.GetOrdenes(DateTime.Parse("2023-03-01 08:00"), DateTime.Parse("2023-03-01 09:00")); // intermedio dap NO kits
                //IntegracionPrestashopLib.Schema.dto.OrdenesResponse ordenes = oOrdenDAL.GetOrdenes(DateTime.Parse("2023-03-17 16:30"), DateTime.Parse("2023-03-17 17:00"));
                //
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
