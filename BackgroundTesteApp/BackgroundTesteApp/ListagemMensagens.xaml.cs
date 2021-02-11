using BackgroundTesteApp.Converters;
using BackgroundTesteApp.Models;
using BackgroundTesteApp.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public partial class ListagemMensagens : ContentPage
    {
        INotificationManager notificationManager;
        int notificationNumber = 0;

        private string _nomeGrupo { get; set; }
        private Usuario _usuario { get; set; }
        public ListagemMensagens()
        {
            InitializeComponent();

            notificationManager = DependencyService.Get<INotificationManager>();
            notificationManager.NotificationReceived += (sender, eventArgs) =>
            {
                var evtData = (NotificationEventArgs)eventArgs;
                ShowNotification(evtData.Title, evtData.Message);
            };

            Enviar.Clicked += async (sender, args) =>
            {
                var mensagem = Mensagem.Text.Trim();

                if (mensagem.Length > 0)
                {
                    await BackgroundTestService.GetInstance().EnviarMensagem(UsuarioManager.GetUsuarioLogado(), mensagem, _nomeGrupo);
                    Mensagem.Text = string.Empty;

                    notificationNumber++;
                    string title = $"NOTIFICAÇÃO DE ALARME";
                    //string message = receiveText.Text + $" Notificação nº {notificationNumber}";
                    string message = $" Notificação nº {notificationNumber}";

                    notificationManager.SendNotification(title, message);
                }
                else
                {
                    await DisplayAlert("Erro no preenchimento!", "Preencha o campo mensagem!", "OK");
                }
            };
        }

        public void SetUsuario(Usuario usuario)
        {
            _usuario = usuario;
            Title = usuario.Nome.FirstCharWordsToUpper();

            var emailUm = UsuarioManager.GetUsuarioLogado().Email;
            var emailDois = usuario.Email;
            Task.Run(async () => { await BackgroundTestService.GetInstance().CriarOuAbrirGrupo(emailUm, emailDois); });
        }

        public void SetScrollOnBottom()
        {
            if (Listagem.ItemsSource != null)
            {
                var ultimoItemDaLista = Listagem.ItemsSource.Cast<object>().LastOrDefault();
                Listagem.ScrollTo(ultimoItemDaLista, ScrollToPosition.MakeVisible, true);
            }
        }

        public void SetNomeGrupo(string nomeGrupo)
        {
            _nomeGrupo = nomeGrupo;
        }

        public string GetNomeGrupo()
        {
            return _nomeGrupo;
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

    public class ListagemMensagensViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<Mensagem> _mensagens;
        public ObservableCollection<Mensagem> Mensagens
        {
            get
            {
                return _mensagens;
            }
            set
            {
                _mensagens = value;
                NotifyPropertyChanged(nameof(Mensagens));
            }
        }

        public ListagemMensagensViewModel()
        {

        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class MensagemDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate EsquerdaTemplate { get; set; }
        public DataTemplate DireitaTemplate { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            Usuario usuarioLogado = new Usuario() { Id = 1, Nome = "Anderson" };

            return ((Mensagem)item).Usuario.Id == usuarioLogado.Id ? DireitaTemplate : EsquerdaTemplate;
        }
    }
}