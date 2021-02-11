using BackgroundTesteApp.Models;
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
    public partial class Cadastro : ContentPage
    {
        public Cadastro()
        {
            InitializeComponent();

            Cadastrar.Clicked += async (sender, args) =>
            {
                string nome = Nome.Text;
                string email = Email.Text;
                string senha = Senha.Text;

                Usuario usuario = new Usuario() { Nome = nome, Email = email, Senha = senha };

                Mensagem.Text = string.Empty;
                Cadastrar.IsEnabled = false;
                Processando.IsRunning = true;


                await Services.BackgroundTestService.GetInstance().Cadastrar(usuario);
            };
        }

        public void SetMensagem(string msg, bool isErro)
        {
            Mensagem.TextColor = (isErro) ? Color.Red : Color.White;

            Mensagem.Text = msg;
            Cadastrar.IsEnabled = true;
            Processando.IsRunning = false;

            if (!isErro)
            {
                Nome.Text = string.Empty;
                Email.Text = string.Empty;
                Senha.Text = string.Empty;
            }
        }
    }
}