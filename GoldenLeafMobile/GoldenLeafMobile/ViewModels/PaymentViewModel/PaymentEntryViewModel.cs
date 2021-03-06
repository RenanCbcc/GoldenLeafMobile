﻿using GoldenLeafMobile.Models;
using GoldenLeafMobile.Models.ClerkModels;
using GoldenLeafMobile.Models.ClientModels;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace GoldenLeafMobile.ViewModels.PaymentViewModel
{
    public class PaymentEntryViewModel
    {
        private readonly string URL_PAYMENT = "https://goldenleafapi.herokuapp.com/api/v1.0/Payment";
        public readonly string SUCCESS = "OnSuccessSavingPayment";
        public readonly string FAIL = "OnFailedSavingPayment";
        public readonly string ASK = "OnSavingPayment";
        public readonly string ACCESS = "OnRequestUnauthorized";


        public ICommand PayCommand { get; set; }

        public Client Client { get; }
        public Clerk Clerk { get; }

        private float _value;

        public float Value
        {
            get { return _value; }
            set { _value = value; ((Command)PayCommand).ChangeCanExecute(); }
        }


        public PaymentEntryViewModel(Client client)
        {
            PayCommand = new Command
                (
                    () =>
                    {
                        MessagingCenter.Send<string>($"R$ {this.Value}", ASK);
                    },
                    () =>
                    {
                        return Value > 0 && Value <= Client.Debt;
                    }
                );
            Client = client;
            Clerk = Application.Current.Properties["Clerk"] as Clerk;
        }

        public async void SavePayment()
        {
            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {Clerk.GetToken()}");
                var stringContent = new StringContent(PaymentToJson(), Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(URL_PAYMENT, stringContent);                

                if (response.IsSuccessStatusCode)
                {
                    MessagingCenter.Send<String>($"R$ {this.Client.Debt - this.Value}", SUCCESS);
                }
                else
                {
                    var content = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    if (response.Content != null)
                    {
                        response.Content.Dispose();
                    }
                    var simpleHttpResponse = new SimpleHttpResponseException(response.StatusCode, response.ReasonPhrase, content);
                    MessagingCenter.Send(simpleHttpResponse, FAIL);

                }
            }
        }

        private string PaymentToJson()
        {
            var payment = JsonConvert.SerializeObject(new
            {
                ClientId = Client.Id,
                ClerkId = Clerk.Id,
                Value
            });

            return payment;
        }
    }
}
