using System;
using System.Configuration;
using System.Drawing;
using System.Text;
using System.Threading;
using AForge.Video;
using AForge.Video.DirectShow;
using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json;
using Instagram.Model;

namespace ServidorInstaConsola
{
    class Program
    {
        //Constantes
        private const string ServidorPrincipal_Ip = "ServidorPrincipal.Ip";
        private const string ServidorPrincipal_Puerto = "ServidorPrincipal.Puerto";
        private static FilterInfoCollection WebcamColl;
        private static VideoCaptureDevice Device;
        private static Bitmap Imagen;

        // Main Method 
        static void Main(string[] args)
        {
            CrearServidor();
        }

        /// <summary>
        /// Crear servidor socket
        /// </summary>
        public static void CrearServidor()
        {
            try
            {
                //Variables
                string sIp_ServidorPrincipal = ConfigurationManager.AppSettings[ServidorPrincipal_Ip];
                int iPuerto_ServidorPrincipal = int.Parse(ConfigurationManager.AppSettings[ServidorPrincipal_Puerto]);

                //Configuraciones
                IPAddress objIpServidor = IPAddress.Parse(sIp_ServidorPrincipal);
                IPEndPoint objEndPoint = new IPEndPoint(objIpServidor, iPuerto_ServidorPrincipal);

                //Crear socket
                Socket objSocket = new Socket(objIpServidor.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                objSocket.Bind(objEndPoint);

                //Conectar socket
                objSocket.Listen(10);

                while (true)
                {
                    //Aceptar conexiones
                    Console.WriteLine("Waiting connection ... ");
                    Socket objClienteSocket = objSocket.Accept();

                    //Respuesta
                    RespuestaDTO objRespuesta = new RespuestaDTO();

                    try
                    {
                        try
                        {
                            //Tomar foto con camara
                            TomarFotoWebCam();

                            //Convertir imagen a base64
                            objRespuesta.Imagen = Utilidades.ConvertImageToBase64(Imagen);
                        }
                        catch (Exception ex)
                        {
                            objRespuesta.Error = true;
                            objRespuesta.Mensaje = string.Format("Error al guardar archivo respaldo archivo: {0}", ex.Message);
                        }
                    }
                    catch (Exception ex)
                    {
                        objRespuesta.Error = true;
                        objRespuesta.Mensaje = string.Format("Error al guardar archivo: {0}", ex.Message);
                    }

                    //Enviar mensaje
                    objClienteSocket.Send(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(objRespuesta)));

                    //Cerrar conexion
                    objClienteSocket.Shutdown(SocketShutdown.Both);
                    objClienteSocket.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        /// <summary>
        /// Tomar foto con webcam
        /// </summary>
        private static void TomarFotoWebCam()
        {
            WebcamColl = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            //if you have connected with one more camera choose index as you want 
            Device = new VideoCaptureDevice(WebcamColl[0].MonikerString);
            Device.Start();
            Device.NewFrame += new NewFrameEventHandler(Device_NewFrame);
            Thread.Sleep(3000);
        }

        /// <summary>
        /// Guardar imagen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        static void Device_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            if (Device.IsRunning)
            {
                Imagen = (Bitmap)eventArgs.Frame.Clone();
                Device.SignalToStop();
            }
        }
    }
}
