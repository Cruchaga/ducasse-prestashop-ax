using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntegracionPrestashopLib.Bussiness.Util
{
    internal class Parser
    {
        internal static void parsePSProdFromERP(Schema.ERP.Producto prodIn, ref Schema.PS.Producto prodOut)
        {
            prodOut.Alto = prodIn.GrossHeight.ToString().Replace(",", ".");
            prodOut.Ancho = prodIn.GrossWidth.ToString().Replace(",", ".");
            prodOut.Profundidad = prodIn.GrossDepth.ToString().Replace(",", ".");
            prodOut.CodeBarEan13 = prodIn.ItemBarCode;
            prodOut.Precio = prodIn.SalesPrice.ToString().Replace(",", ".");
            prodOut.Reference = prodIn.ItemId_InventDimId;
            prodOut.Weight = prodIn.NetWeight.ToString().Replace(",", ".");
            prodOut.Description = new Schema.PS.LangDescriptions
            {
                Languages = new Schema.PS.Lang
                {
                    LangId = "1",
                    Language = prodIn.ItemTxt
                }
            };
            prodOut.DescriptionString = prodIn.ItemTxt;

            prodOut.Name = new Schema.PS.LangDescriptions
            {
                Languages = new Schema.PS.Lang
                {
                    LangId = "1",
                    Language = prodIn.ItemName
                }
            };
            prodOut.NameString = prodIn.ItemName;

            prodOut.ShortDescription = new Schema.PS.LangDescriptions
            {
                Languages = new Schema.PS.Lang
                {
                    LangId = "1",
                    Language = prodIn.NameAlias
                }
            };
            prodOut.ShortDescriptionString = prodIn.NameAlias;

            prodOut.UrlAmigable = new Schema.PS.LangDescriptions
            {
                Languages = new Schema.PS.Lang
                {
                    LangId = "1",
                    Language = prodIn.NameAlias
                }
            };
            prodOut.UrlAmigableString = prodIn.NameAlias;
        }

        internal static void parseERPProdFromPS(Schema.PS.Producto prodIn, ref Schema.ERP.Producto prodOut)
        {
            prodOut.GrossHeight = decimal.Parse(prodIn.Alto.ToString().Replace(".", ","));
            prodOut.GrossWidth = decimal.Parse(prodIn.Ancho.ToString().Replace(".", ","));
            prodOut.GrossDepth = decimal.Parse(prodIn.Profundidad.ToString().Replace(".", ","));
            prodOut.ItemBarCode = prodIn.CodeBarEan13;
            prodOut.SalesPrice = decimal.Parse(prodIn.Precio.ToString().Replace(".", ","));
            prodOut.ItemId_InventDimId = prodIn.Reference;
            prodOut.NetWeight = decimal.Parse(prodIn.Weight.ToString().Replace(".", ","));
            prodOut.ItemTxt = prodIn.DescriptionString;
            prodOut.ItemName = prodIn.NameString;
            prodOut.NameAlias = prodIn.ShortDescriptionString;
        }

        public Schema.ERP.OrdenDeVenta GetOVFromPSOC(Schema.PS.OrdenDeCompra orden)
        {
            try
            {
                return new Schema.ERP.OrdenDeVenta
                {
                    //TODO: Implementar
                };
            }
            catch (Exception e) {
                //TODO: implementar logging
                return null;
            }
        }
    }
}
