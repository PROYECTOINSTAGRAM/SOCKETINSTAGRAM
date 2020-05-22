using Instagram.Model;
using InstagramApiSharp.API;
using InstagramApiSharp.Classes.Models;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Instagram
{
    public partial class FrmMenuPricipal : Form
    {
        //Constantes
        public const string ServidorPrincipal_Ip = "ServidorPrincipal.Ip";
        public const string ServidorPrincipal_Puerto = "ServidorPrincipal.Puerto";
        private readonly IInstaApi _instaApi;

        public FrmMenuPricipal(IInstaApi instaApi)
        {
            InitializeComponent();
            _instaApi = instaApi;
            CargarControles();
        }

        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();
        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private extern static void SendMessage(System.IntPtr hwnd, int wmsg, int wparam, int lparam);

        /// <summary>
        /// Cargar controles
        /// </summary>
        private async void CargarControles()
        {
            var currentUser = await _instaApi.GetCurrentUserAsync();
            lblUsuario.Text = currentUser.Value.UserName.ToString();
            var request = WebRequest.Create(currentUser.Value.ProfilePicture.ToString());

            using (var response = request.GetResponse())
            using (var stream = response.GetResponseStream())
            {
                ImagenPerfil.Image = Bitmap.FromStream(stream);
            }
        }

       


        private async void button1_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;

            try
            {
                string startupPath = Directory.GetCurrentDirectory();

                Bitmap imgBitmap = Utilidades.ConvertTo24bpp(ImagenAsubir.Image);

                string sRutaImagen = string.Format("{0}\\Uploads\\{1}.jpg", startupPath, Guid.NewGuid().ToString());
                imgBitmap.Save(sRutaImagen, ImageFormat.Jpeg);

                var mediaImage = new InstaImageUpload
                {
                    Height = 1080,
                    Width = 1080,
                    Uri = sRutaImagen
                };

                var result = await _instaApi.MediaProcessor.UploadPhotoAsync(mediaImage, txtComentarios.Text);
                if (result.Succeeded)
                {
                    this.Cursor = Cursors.Default;
                    MessageBox.Show("Proceso completado satisfactoriamente", "Mensaje de Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    this.Cursor = Cursors.Default;
                    MessageBox.Show(result.Info.Message, "Mensaje de Exception Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                imgBitmap.Dispose();
                File.Delete(sRutaImagen);
            }
            catch (Exception ex)
            {
                this.Cursor = Cursors.Default;
                MessageBox.Show(ex.Message, "Mensaje de Exception Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnMinimizar_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void FrmMenuPricipal_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        private void pictureBox2_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        private void btnObtenerImagen_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;

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

                //Conectar
                objSocket.Connect(objEndPoint);

                //Enviar información
                int iEnviado = objSocket.Send(Encoding.ASCII.GetBytes(string.Empty));

                //Recibir información
                byte[] bytes = new byte[150 * 1024];
                int iRecibido = objSocket.Receive(bytes);
                RespuestaDTO objRespuesta = JsonConvert.DeserializeObject<RespuestaDTO>(Encoding.UTF8.GetString(bytes, 0, iRecibido).Replace("\0", string.Empty));

                //Validar que el proceso se haya realizado correctamente
                if (!objRespuesta.Error)
                {
                    //Asignar imagen
                    ImagenAsubir.Image = Utilidades.ConvertBase64ToImagen(objRespuesta.Imagen);

                    //Habilitar opciones para subir imagen
                    btnSubirImagen.Enabled = true;
                    txtComentarios.Enabled = true;

                    this.Cursor = Cursors.Default;
                    MessageBox.Show("Proceso completado satisfactoriamente", "Mensaje de Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    this.Cursor = Cursors.Default;
                    MessageBox.Show(objRespuesta.Mensaje, "Mensaje de Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                //Cerrar conexion
                objSocket.Shutdown(SocketShutdown.Both);
                objSocket.Close();
            }
            catch (ArgumentNullException ex)
            {
                this.Cursor = Cursors.Default;
                MessageBox.Show(ex.Message, "Mensaje de Argument Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (SocketException ex)
            {
                this.Cursor = Cursors.Default;
                MessageBox.Show(ex.Message, "Mensaje de Socket Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                this.Cursor = Cursors.Default;
                MessageBox.Show(ex.Message, "Mensaje de Exception Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
