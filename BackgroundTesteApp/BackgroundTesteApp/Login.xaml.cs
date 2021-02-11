﻿using BackgroundTesteApp.Models;
using BackgroundTesteApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BackgroundTesteApp
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Login : ContentPage
    {
        public Login()
        {
            InitializeComponent();

            Acessar.Clicked += async (sender, args) =>
            {
                string email = Email.Text;
                string senha = Senha.Text;

                Usuario usuario = new Usuario { Email = email, Senha = senha };

                Mensagem.Text = string.Empty;
                Acessar.IsEnabled = false;
                Processando.IsRunning = true;

                await BackgroundTestService.GetInstance().Login(usuario);
            };
        }

        public void SetMensagem(string msg)
        {
            Mensagem.Text = msg;
            Acessar.IsEnabled = true;
            Processando.IsRunning = false;
        }

        private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            ((Inicio)App.Current.MainPage).CurrentPage = ((Inicio)App.Current.MainPage).Children[1];
        }
    }
}