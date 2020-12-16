using IntegracionPrestashopLib.Schema.ERP;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            
            IntegracionPrestashopLib.Operations.OrdenDAL oOrdenDAL = new  IntegracionPrestashopLib.Operations.OrdenDAL("L29CCIPV8FZB57BJWLGGIKDX1SXKN3TS", "http://45.79.15.48/api/");
            IntegracionPrestashopLib.Operations.ProductoDAL oProductoDAL = new IntegracionPrestashopLib.Operations.ProductoDAL("L29CCIPV8FZB57BJWLGGIKDX1SXKN3TS", "http://45.79.15.48/api/");
            Console.WriteLine(oProductoDAL.TestDateTime(DateTime.Now));
            //IntegracionPrestashopLib.Schema.dto.OrdenesResponse ordenes = oOrdenDAL.GetOrdenes(DateTime.Parse("2020-11-19"), DateTime.Parse("2020-12-11"));

            //Console.WriteLine(oProductoDAL.TestDateTime(DateTime.Now));

            //Console.WriteLine($"La lista de ordenes contiene {ordenes.GetSize()} filas");
            //for(int x=0; x < ordenes.GetSize();x++)
            //{
            //    Console.WriteLine($"Pedido {x+1} tiene nº de orden {ordenes.GetOrden(x).SalesIdPrestaShop} con {ordenes.GetOrden(x).DetalleDeVentas.GetSize()} lineas de detalle");
            //    Console.WriteLine($"Linea 1 cantidad = {ordenes.GetOrden(x).DetalleDeVentas.GetItemDetalle(0).SalesLineQty}");
            //}
            //try
            //{
            //    var resp = oOrdenDAL.Update(new Tracking(6, 4));
            //    Console.WriteLine(resp.Actualizado.ToString());
            //}catch(Exception e)
            //{
            //    Console.WriteLine(e.Message);
            //}

            oProductoDAL.UpdateStock(new Inventario
            {
                ProductoId = 6,
                Stock = 260
            });
            Console.WriteLine(oProductoDAL.TestDateTime(DateTime.Now));
            Console.ReadLine();
        }
    }
}
