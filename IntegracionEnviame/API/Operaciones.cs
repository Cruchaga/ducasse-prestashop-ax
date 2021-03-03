using IntegracionEnviame.Bussiness.Clientes;
using IntegracionEnviame.Bussiness.Util;
using IntegracionEnviame.Schema.Request;
using IntegracionEnviame.Schema.Requests;
using IntegracionEnviame.Schema.Response;
using IntegracionEnviame.Schema.Responses;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IntegracionEnviame.API
{
    public class Operaciones
    {
        private String apiKey;
        private String urlBase;
        private int idCompania;

        public Operaciones(string apiKey, string urlBaseEnviame, int idEmpresa)
        {
            this.apiKey = apiKey;
            this.urlBase = urlBaseEnviame;
            this.idCompania = idEmpresa;
        }

        public CrearEnvioResponse CrearEnvio(CrearEnvioRequest envio)
        {
            try
            {
                if (InitChecked())
                {
                    String json = JsonConvert.SerializeObject(new MakeDeliveryRequest
                    {
                        ShippingOrder = new ShippingOrder
                        {
                            Packages = envio.Orden.CantidadBultos,
                            Description = envio.Orden.DescripcionContenido,
                            PrestashopOrderId = envio.Orden.IdReferencia,
                            TotalPrice = Convert.ToInt32(envio.Orden.Precio),
                            Weight = envio.Orden.Get_PesoTotal(),
                            Volume = envio.Orden.Get_VolumenTotal(),
                            Type = "delivery"
                        },
                        ShippingOrigin = new ShippingOrigin
                        {
                            WarehouseCode = envio.Orden.CodBodegaOrigen
                        },
                        ShippingDestination = new ShippingDetination
                        {
                            Customer = new Schema.Request.Customer
                            {
                                Name = envio.Destino.NombreReceptor,
                                Email = envio.Destino.EmailReceptor,
                                Phone = envio.Destino.TelefonoReceptor
                            },
                            DeliveryAddress = new DeliveryAddress
                            {
                                HomeAddress = new HomeAddress
                                {
                                    Comuna = envio.Destino.Comuna,
                                    FullAddress = envio.Destino.DireccionCompleta,
                                    Information = envio.Destino.Informacion
                                }
                            }
                        },
                        Carrier = new Schema.Request.Carrier
                        {
                            Code = envio.Carrier,
                            Service = envio.CarrierService,
                            TrackingNumber = ""
                        }
                    });
                    String url = $"{urlBase}companies/{idCompania}/deliveries";
                    String method = "POST";
                    String contentType = "application/json";
                    String response = EnviameClient.RequestWithBody(method, url, apiKey, contentType, json);
                    MakeDeliveryResponse deliveryResponse = Parser.ParseDeliveryResponseFromJson(response);

                    return new CrearEnvioResponse
                    {
                        Creado = true,
                        EtiquetaPdf = deliveryResponse.GetEtiquetaPdf(),
                        EtiquetaZebra = deliveryResponse.GetEtiquetaZebra(),
                        Carrier = deliveryResponse.Carrier,
                        Identifier = deliveryResponse.identifier,
                        TrackingNumber = deliveryResponse.TrackingNumber,
                        UrlTracking = (from l in deliveryResponse.Links
                                       where l.Rel == "tracking-web"
                                       select l.Href).FirstOrDefault()
                    };
                }
                else
                {
                    return new CrearEnvioResponse
                    {
                        Creado = false,
                        ErrorCode = 1,
                        ErrorDescription = "No se ha inicializado el objeto 'Operaciones' como corresponde."
                    };
                }
            }
            catch (Exception e)
            {
                return new CrearEnvioResponse
                {
                    Creado = false,
                    ErrorCode = -1,
                    ErrorDescription = $"Error al crear el envío: {{{e.Message}}}"
                };
            }
        }

        public CrearManifiestoResponse GenerarManifiesto(String envios)
        {

            try
            {
                if (InitChecked())
                {
                    var enviosArr = (from e in envios.Split(',')
                                     select Convert.ToInt32(e.Trim())).ToArray();
                    String json = JsonConvert.SerializeObject(new MakeManifestRequest
                    {
                        Deliveries = enviosArr
                    });

                    String url = $"{urlBase}companies/{idCompania}/summary";
                    String method = "POST";
                    String contentType = "application/json";
                    String response = EnviameClient.RequestWithBody(method, url, apiKey, contentType, json);

                    MakeManifestResponse[] manifestResponse = Parser.ParseManifestResponseFromJson(response);
                    return new CrearManifiestoResponse
                    {
                        Creado = true,
                        manifiestos = (from m in manifestResponse
                                       select new Manifiesto
                                       {
                                           Carrier = m.Carrier,
                                           Etiqueta = m.Labels,
                                           SumarioPDF = m.Summary
                                       }).ToList()
                    };
                }
                else
                {
                    return new CrearManifiestoResponse
                    {
                        Creado = false,
                        ErrorCode = 1,
                        ErrorDescription = "No se ha inicializado el objeto 'Operaciones' como corresponde."
                    };
                }
            }
            catch (Exception e)
            {
                return new CrearManifiestoResponse
                {
                    Creado = false,
                    ErrorCode = -1,
                    ErrorDescription = $"Error al generar manifiesto: {{{e.Message}}}"
                };
            }
        }

        public CreateResponse CrearRetiro(CrearRetiroRequest retiro)
        {

            try
            {
                if (InitChecked())
                {
                    var dict = new Dictionary<String, String> {
                    {"carrier_code", retiro.CarrierCod /*"CCH"*/ },
                    {"warehouse_code", retiro.CodigoBodega/*"cod_bod123"*/ },
                    {"qty", retiro.CantidadBultos.ToString()/*"4"*/ },
                    {"contact_name", retiro.NombreContactoRetiro/*"John Doe"*/ },
                    {"contact_phone", retiro.TelefonoContactoRetiro/*"123456789"*/ },
                    {"range_time", retiro.RangoRetiro/*"AM"*/ },
                    {"pickup_date", retiro.FechaRetiro/*"2021-01-14"*/ }
                };
                    String url = $"{urlBase}companies/{idCompania}/pickups";
                    String contentType = "multipart/form-data; boundary=";
                    var response = EnviameClient.RequestWithForm("POST", url, apiKey, contentType, dict);

                    MakeWithdrawalResponse retiroResponse = Parser.ParseWithdrawalResponseFromJson(response);
                    if (retiroResponse.Identifier == null)
                    {
                        return new CreateResponse
                        {
                            Creado = false,
                            ErrorCode = 2,
                            ErrorDescription = retiroResponse.pickupDate[0]
                        };
                    }
                    else
                    {
                        return new CreateResponse();
                    }
                }
                else
                {
                    return new CreateResponse
                    {
                        Creado = false,
                        ErrorCode = 1,
                        ErrorDescription = "No se ha inicializado el objeto 'Operaciones' como corresponde."
                    };
                }
            }
            catch (Exception e)
            {
                return new CreateResponse
                {
                    Creado = false,
                    ErrorCode = -1,
                    ErrorDescription = $"Error al crear retiro: {{{e.Message}}}"
                };
            }
        }


        public VerTrackingResponse VerTracking(int identifier)
        {

            try
            {
                if (InitChecked())
                {
                    String url = $"{urlBase}deliveries/{identifier}/tracking";
                    String contentType = "application/json";
                    var response = EnviameClient.RequestEmpty("GET", url, apiKey, contentType);
                    GetTrackingResponse tracking = Parser.ParseTrackingResponseFromJson(response);

                    return (from t in tracking.Tracking
                            orderby t.Fecha descending
                            select new VerTrackingResponse
                            {
                                CodigoEstado = t.Code,
                                Descripcion = t.Comment
                            }).First();
                }
                else
                {
                    return new VerTrackingResponse
                    {
                        CodigoEstado = "0",
                        Descripcion = "No se ha inicializado el objeto 'Operaciones' como corresponde."
                    };
                }
            }
            catch (Exception e)
            {
                return new VerTrackingResponse
                {
                    CodigoEstado = "0",
                    Descripcion = $"Error al ver tracking: {{{e.Message}}}"
                };
            }
        }

        private bool InitChecked()
        {
            return !string.IsNullOrEmpty(apiKey) && !string.IsNullOrEmpty(urlBase);
        }
    }
}
