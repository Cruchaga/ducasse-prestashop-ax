using IntegracionEnviame.Schema.dto;
using IntegracionEnviame.Schema.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntegracionEnviame.API
{
    public class Operaciones
    {
        private String apyKey;
        private String urlBase;

        public Operaciones(string apiKey, string urlBaseEnviame)
        {
            this.apyKey = apiKey;
            this.urlBase = urlBaseEnviame;
        }

        public void CrearPedido()
        {
            if(InitChecked())
            {
                //TODO: IMPLEMENTAR
            }
            else
            {
                return;//TODO: IMPLEMENTAR
            }
        }

        public void GenerarManifiesto()
        {

            if (InitChecked())
            {
                //TODO: IMPLEMENTAR
            }
            else
            {
                return;//TODO: IMPLEMENTAR
            }
        }

        public void CrearRetiro()
        {

            if (InitChecked())
            {
                //TODO: IMPLEMENTAR
            }
            else
            {
                return;//TODO: IMPLEMENTAR
            }
        }

        public void VerTracking()
        {

            if (InitChecked())
            {
                //TODO: IMPLEMENTAR
            }
            else
            {
                return;//TODO: IMPLEMENTAR
            }
        }

        private bool InitChecked()
        {
            return !string.IsNullOrEmpty(apyKey) && !string.IsNullOrEmpty(urlBase);
        }
    }
}
