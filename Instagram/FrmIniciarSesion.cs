using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using InstagramApiSharp.API;
using InstagramApiSharp.Classes;
using InstagramApiSharp.API.Builder;
using InstagramApiSharp.Logger;

namespace Instagram
{
    public partial class FrmIniciarSesion : Form
    {
        private static IInstaApi _instaApi;
        const string stateFile = "state.bin";

        public FrmIniciarSesion()
        {
            InitializeComponent();
        }

        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();
        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private extern static void SendMessage(System.IntPtr hwnd, int wmsg, int wparam, int lparam);

        private void textUsuario_Enter(object sender, EventArgs e)
        {
            if (textUsuario.Text == "USUARIO")
            {
                textUsuario.Text = "";
                textUsuario.ForeColor = Color.LightGray;
            }
        }

        private void textUsuario_Leave(object sender, EventArgs e)
        {
            if (textUsuario.Text == "")
            {
                textUsuario.Text = "USUARIO";
                textUsuario.ForeColor = Color.DimGray;
            }
        }

        private void textPass_Enter(object sender, EventArgs e)
        {
            if (textPass.Text == "CONTRASEÑA")
            {
                textPass.Text = "";
                textPass.ForeColor = Color.LightGray;
                textPass.UseSystemPasswordChar = true;
            }
        }

        private void textPass_Leave(object sender, EventArgs e)
        {
            if (textPass.Text == "")
            {
                textPass.Text = "CONTRASEÑA";
                textPass.ForeColor = Color.DimGray;
                textPass.UseSystemPasswordChar = false;
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

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        private async void butnLogin_Click(object sender, EventArgs e)
        {
            try
            {
                if (textUsuario.Text == "USUARIO")
                    throw new Exception("Porfavor ingrese su usuario.");

                if (textPass.Text == "CONTRASEÑA")
                    throw new Exception("Porfavor ingrese su contraseña");

                // create user session data and provide login details
                var userSession = new UserSessionData
                {
                    UserName = textUsuario.Text,//"stillgonzalez123",
                    Password = textPass.Text//"abc$$$123"
                };

                // create new InstaApi instance using Builder
                _instaApi = InstaApiBuilder.CreateBuilder()
                    .SetUser(userSession)
                    .UseLogger(new DebugLogger(LogLevel.Exceptions)) // use logger for requests and debug messages
                    .Build();

                //if (File.Exists(stateFile))
                //{
                //    using (var fs = File.OpenRead(stateFile))
                //    {
                //        _instaApi.LoadStateDataFromStream(fs);
                //        // in .net core or uwp apps don't use LoadStateDataFromStream
                //        // use this one:
                //        // _instaApi.LoadStateDataFromString(new StreamReader(fs).ReadToEnd());
                //        // you should pass json string as parameter to this function.
                //    }

                //}

                if (!_instaApi.IsUserAuthenticated)
                {
                    // login
                    //Console.WriteLine($"Logging in as {userSession.UserName}");
                    var logInResult = await _instaApi.LoginAsync();
                    if (!logInResult.Succeeded)
                    {
                        //Console.WriteLine($"Unable to login: {logInResult.Info.Message}");
                        throw new Exception(logInResult.Info.Message);
                    }
                }

                //var state = _instaApi.GetStateDataAsStream();
                //using (var fileStream = File.Create(stateFile))
                //{
                //    state.Seek(0, SeekOrigin.Begin);
                //    state.CopyTo(fileStream);
                //}

                // get currently logged in user
                var currentUser = await _instaApi.GetCurrentUserAsync();
                FrmMenuPricipal MP = new FrmMenuPricipal(_instaApi);
                MP.Show();
                this.Hide();
            }
            catch (Exception ex)
            {
                msgError(ex.Message);
            }
        }

        private void msgError(string msg)
        {
            lblErrorMessage.Text = msg;
            lblErrorMessage.Visible = true;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //var logoutResult = Task.Run(() => _instaApi.LogoutAsync()).GetAwaiter().GetResult();

        }
    }
}
