﻿using GoldenLeafMobile.Models;
using GoldenLeafMobile.Models.ClerkModels;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace GoldenLeafMobile.ViewModels.ClerkViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly string URL_POST_CLERK = "https://goldenleafapi.herokuapp.com/api/v1.0/Account/Login";
        private string _email;
        private string _password;

        public readonly string SUCCESS = "OnSuccessLogin";
        public readonly string FAILPOST = "OnFailedPost";
        public readonly string FAILCONNECTION = "OnFailedConnection";


        public string Password
        {
            get { return _password; }
            set { _password = value; ((Command)LoginCommand).ChangeCanExecute(); }
        }

        public string Email
        {
            get { return _email; }
            set { _email = value; ((Command)LoginCommand).ChangeCanExecute(); }
        }


        public Command LoginCommand { get; private set; }


        public LoginViewModel()
        {
            LoginCommand = new Command
                (
                    async () =>
                    {
                        await Login();
                    },
                    () =>
                        {
                            return !string.IsNullOrEmpty(_email) 
                            && !string.IsNullOrEmpty(_password)
                            && !Wait;
                        }
                );
        }

        private async Task Login()
        {
            Wait = true;
            HttpClient httpClient = new HttpClient();
            
            var stringContent = new StringContent(LoginToJson(), Encoding.UTF8, "application/json");
            HttpResponseMessage response;
            try
            {
                response = await httpClient.PostAsync(URL_POST_CLERK, stringContent);
            }
            catch (Exception)
            {
                Wait = false;
                var reasonPhrase = "Erro na comunicação";
                var message = @"Ocorreu um erro de comunicação com o servidor. Por favor, verifique a sua conexão e tente novamente mais tarde.";

                MessagingCenter.Send(new LoginException(reasonPhrase, message), FAILCONNECTION);
                return;
            }
           
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var clerk = JsonConvert.DeserializeObject<Clerk>(content);
                if (clerk.Photo != null)
                {
                    clerk.ProfileImage = Base64ToImage(clerk.Photo);
                }

                MessagingCenter.Send(clerk, SUCCESS);
            }
            else
            {
                var content = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                if (response.Content != null)
                    response.Content.Dispose();

                MessagingCenter.Send(new SimpleHttpResponseException(response.StatusCode, response.ReasonPhrase, content),
                    FAILPOST);
            }
            Wait = false;
        }

        private string LoginToJson()
        {
            return JsonConvert.SerializeObject(new
            {
                Email = this._email,
                Password = this._password
            });
        }
        private ImageSource Base64ToImage(string base64String)
        {
            byte[] bytes = Convert.FromBase64String(base64String);
            var profile_pic = ImageSource.FromStream(() => new MemoryStream(bytes));
            return profile_pic;
        }
    }
}
