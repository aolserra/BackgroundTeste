using BackgroundTesteApp.Models;
using BackgroundTesteApp.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BackgroundTesteApp
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ListagemUsuarios : ContentPage
    {
        INotificationManager notificationManager;
        int notificationNumber = 0;

        public ListagemUsuarios()
        {
            InitializeComponent();

            notificationManager = DependencyService.Get<INotificationManager>();
            notificationManager.NotificationReceived += (sender, eventArgs) =>
            {
                var evtData = (NotificationEventArgs)eventArgs;
                ShowNotification(evtData.Title, evtData.Message);
            };

            Sair.Clicked += async (sender, args) =>
            {
                //SignalR
                await BackgroundTestService.GetInstance().Sair(UsuarioManager.GetUsuarioLogado());
                //App
                UsuarioManager.DelUsuarioLogado();

                App.Current.MainPage = new Inicio();
            };

            Listagem.ItemTapped += (sender, args) =>
            {
                Usuario usuario = (Usuario)args.Item;

                var listagemMensagens = new ListagemMensagens();
                listagemMensagens.SetUsuario(usuario);

                Navigation.PushAsync(listagemMensagens);
            };

            Task.Run(async () => { await BackgroundTestService.GetInstance().ObterListaUsuarios(); });
        }

        void ShowNotification(string title, string message)
        {
            //Device.BeginInvokeOnMainThread(() =>
            //{
            //    var msg = new Label()
            //    {
            //        Text = $"Notification Received:\nTitle: {title}\nMessage: {message}"
            //    };
            //    stackLayout.Children.Add(msg);
            //});
        }
    }

    public class ListagemUsuariosViewModel : INotifyPropertyChanged
    {
        private List<Usuario> _usuarios;
        public List<Usuario> Usuarios
        {
            get
            {
                return _usuarios;
            }
            set
            {
                _usuarios = value;
                NotifyPropertyChanged(nameof(Usuarios));
            }
        }

        public ListagemUsuariosViewModel()
        {

        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}