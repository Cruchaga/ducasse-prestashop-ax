using IntegracionPrestashopLib.Schema.dto;
using IntegracionPrestashopLib.Schema.ERP;
using IntegracionPrestashopLib.Schema.PS.Responses;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntegracionPrestashopLib.Operations
{
    public class ProductoDAL
    {
        /**
         * <summary>
         * Método para crear un Producto en PrestaShop
         * </summary>
         */
        public CreateResponse Create(Producto producto)
        {
            return  new CreateResponse
            {
                Creado = true
            };
        }

        /**
         * <summary>
         * Método que permite actualizar tanto los atributos del producto como el stock
         * </summary>
         */
        public UpdateResponse Update(Inventario stockProd)
        {
            return new UpdateResponse
            {
                Actualizado = true
            };
        }

        public String TestDateTime(DateTime date)
        {
            return $"La fecha es: {date.ToString("dd 'de' MMMM 'de' yyyy con 'hora' HH:mm:ss FFF")}";
        }
    }
}
