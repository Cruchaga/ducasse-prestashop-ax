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
            
            IntegracionPrestashopLib.Operations.OrdenDAL oOrdenDAL = new  IntegracionPrestashopLib.Operations.OrdenDAL("X65SKAWDWMBK4QY5B58Q4ABE9EYPW54F", "http://45.79.240.19/api/");
            IntegracionPrestashopLib.Operations.ProductoDAL oProductoDAL = new IntegracionPrestashopLib.Operations.ProductoDAL();
            Console.WriteLine(oProductoDAL.TestDateTime(DateTime.Now));
            IntegracionPrestashopLib.Schema.dto.OrdenesResponse ordenes = oOrdenDAL.GetOrdenes(DateTime.Parse("2020-12-08"), DateTime.Parse("2020-12-11"));
            
            Console.WriteLine(oProductoDAL.TestDateTime(DateTime.Now));

            Console.WriteLine($"La lista de ordenes contiene {ordenes.GetSize()} filas");
            for(int x=0; x < ordenes.GetSize();x++)
            {
                Console.WriteLine($"Pedido {x+1} tiene nº de orden {ordenes.GetOrden(x).SalesIdPrestaShop} con {ordenes.GetOrden(x).DetalleDeVentas.GetSize()} lineas de detalle");
                Console.WriteLine($"Linea 1 cantidad = {ordenes.GetOrden(x).DetalleDeVentas.GetItemDetalle(0).SalesLineQty}");
            }
            Console.WriteLine(oProductoDAL.TestDateTime(DateTime.Now));
            Console.ReadLine();
        }
    }
}
