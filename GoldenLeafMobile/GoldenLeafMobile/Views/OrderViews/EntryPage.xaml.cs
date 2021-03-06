﻿using GoldenLeafMobile.Models;
using GoldenLeafMobile.Models.ClerkModels;
using GoldenLeafMobile.Models.ClientModels;
using GoldenLeafMobile.Models.OrderModels;
using GoldenLeafMobile.ViewModels.OrderViewModel;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ZXing.Net.Mobile.Forms;

namespace GoldenLeafMobile.Views.OrderViews
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EntryPage : TabbedPage
    {
        public OrderEntryViewModel ViewModel { get; private set; }

        public EntryPage(Client client)
        {
            InitializeComponent();
            var clerk = Application.Current.Properties["Clerk"] as Clerk;
            ViewModel = new OrderEntryViewModel(clerk, client);
            BindingContext = ViewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            SignUpMessages();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            MessagingCenter.Unsubscribe<OrderEntryViewModel>(this, ViewModel.ASK);
            MessagingCenter.Unsubscribe<OrderEntryViewModel>(this, ViewModel.SUCCESS);
            MessagingCenter.Unsubscribe<string>(this, ViewModel.ACCESS);
            MessagingCenter.Unsubscribe<SimpleHttpResponseException>(this, ViewModel.FAIL);

        }

        private void SignUpMessages()
        {
            MessagingCenter.Subscribe<OrderEntryViewModel>(this, ViewModel.ASK, async (_msg) =>
            {
                var confirm = await DisplayAlert("Salvar pedido", "Deseja mesmo salvar o pedido?", "Sim", "Não");
                if (confirm)
                {
                    ViewModel.SaveOrder();
                }
            });

            MessagingCenter.Subscribe<OrderEntryViewModel>(this, ViewModel.SUCCESS, async (_msg) =>
            {
                await DisplayAlert("Salvar pedido", "Pedido salvo com sucesso!", "Ok");
                await Navigation.PopToRootAsync();
            });

            MessagingCenter.Subscribe<SimpleHttpResponseException>(this, ViewModel.FAIL, (_msg) =>
            {
                DisplayAlert(_msg.ReasonPhrase, _msg.Message, "Ok");
            });

            MessagingCenter.Subscribe<string>(this, ViewModel.ACCESS, async (_msg) =>
            {
                await DisplayAlert("Salvar pedido", $"{_msg} o seu token expirou! Refaça o login.", "Ok");
                await Navigation.PopToRootAsync();
            });

        }

        private async void ToolbarItem_Clicked(object sender, System.EventArgs e)
        {
            var scanPage = new ZXingScannerPage();
            scanPage.OnScanResult += (result) =>
            {
                scanPage.IsScanning = false;

                Device.BeginInvokeOnMainThread(async () =>
                {
                    await Navigation.PopModalAsync();
                    ViewModel.Code = result.Text;
                });
            };
            await Navigation.PushModalAsync(scanPage);
        }

        private void OnEdit(object sender, System.EventArgs e)
        {
            var mi = ((MenuItem)sender);
            var item = mi.CommandParameter as OrderTableItem;
            ViewModel.Edit(item);
            this.CurrentPage = this.Children[1];
        }

        private void OnDelete(object sender, System.EventArgs e)
        {
            var mi = ((MenuItem)sender);
            var item = mi.CommandParameter as OrderTableItem;
            ViewModel.Remove(item);
        }
    }
}