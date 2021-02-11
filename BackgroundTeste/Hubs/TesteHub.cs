using BackgroundTeste.Database;
using BackgroundTeste.Models;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackgroundTeste.Hubs
{
    public class TesteHub : Hub
    {
        private BancoContext _banco;
        public TesteHub(BancoContext banco)
        {
            _banco = banco;
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var usuario = _banco.Usuarios.FirstOrDefault(a => a.ConnectionId.Contains(Context.ConnectionId));
            if (usuario != null)
            {
                await DelUserConnectionId(usuario);
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task Cadastrar(Usuario usuario)
        {
            try
            {
                bool IsExistUser = _banco.Usuarios.Where(a => a.Email == usuario.Email).Count() > 0;

                if (IsExistUser == true)
                {
                    await Clients.Caller.SendAsync("ReceberCadastro", false, null, "E-mail já cadastrado!");
                }
                else
                {
                    _banco.Usuarios.Add(usuario);
                    _banco.SaveChanges();

                    await Clients.Caller.SendAsync("ReceberCadastro", true, usuario, "Usuário cadastrado com sucesso!");
                }
            }
            catch
            {

            }
        }

        public async Task Login(Usuario usuario)
        {
            var usuarioDB = _banco.Usuarios.FirstOrDefault(a => a.Email == usuario.Email && a.Senha == usuario.Senha);

            if (usuarioDB == null)
            {
                await Clients.Caller.SendAsync("ReceberLogin", false, null, "E-mail ou senha errado!");
            }
            else
            {
                await Clients.Caller.SendAsync("ReceberLogin", true, usuarioDB, null);
                usuarioDB.IsOnline = true;
                _banco.Usuarios.Update(usuarioDB);
                _banco.SaveChanges();

                await NotificarMudancaNaListaUsuarios();
            }
        }

        public async Task Logout(Usuario usuario)
        {
            var usuarioDB = _banco.Usuarios.Find(usuario.Id);
            usuarioDB.IsOnline = false;
            _banco.Usuarios.Update(usuarioDB);
            _banco.SaveChanges();

            await DelUserConnectionId(usuarioDB);

            await NotificarMudancaNaListaUsuarios();

        }

        public async Task AddUserConnectionId(Usuario user)
        {
            var connectionIdCurrent = Context.ConnectionId;
            List<string> connectionsId = null;

            Usuario usuarioDB = _banco.Usuarios.Find(user.Id);
            if (usuarioDB.ConnectionId == null | usuarioDB.ConnectionId == "")
            {
                connectionsId = new List<string>();
                connectionsId.Add(connectionIdCurrent);
            }
            else
            {
                connectionsId =  JsonConvert.DeserializeObject<List<string>>(usuarioDB.ConnectionId);

                if (!connectionsId.Contains(connectionIdCurrent))
                {
                    connectionsId.Add(connectionIdCurrent);
                }
            }

            usuarioDB.IsOnline = true;
            usuarioDB.ConnectionId = JsonConvert.SerializeObject(connectionsId);

            _banco.Usuarios.Update(usuarioDB);
            _banco.SaveChanges();

            await NotificarMudancaNaListaUsuarios();

            string AddGrpSiganlR = null;
            //Adicionar ConnectionsId aos grupos do SignalR
            var grupos = _banco.Grupos.Where(a => a.Usuarios.Contains(usuarioDB.Email));
            foreach (var connectionId in connectionsId)
            {
                foreach (var grupo in grupos)
                {
                    await Groups.AddToGroupAsync(connectionId, grupo.Nome);

                    AddGrpSiganlR = "Adicionado ao grupo do SignalR.";
                }
            }

            await Clients.Caller.SendAsync("Teste", connectionIdCurrent, connectionsId, usuarioDB, grupos, AddGrpSiganlR);
        }

        public async Task DelUserConnectionId(Usuario user)
        {
            Usuario usuarioDB = _banco.Usuarios.Find(user.Id);
            List<string> connectionsId = null;
            if (usuarioDB.ConnectionId.Length > 0)
            {
                var connectionIdCurrent = Context.ConnectionId;
                //connectionsId = Jil.JSON.Deserialize<List<string>>(usuarioDB.ConnectionId);
                connectionsId = JsonConvert.DeserializeObject<List<string>>(usuarioDB.ConnectionId);
                if (connectionsId.Contains(connectionIdCurrent))
                {
                    connectionsId.Remove(connectionIdCurrent);
                }

                //usuarioDB.ConnectionId = Jil.JSON.Serialize(connectionsId);
                usuarioDB.ConnectionId = JsonConvert.SerializeObject(connectionsId);

                if (connectionsId.Count <= 0)
                {
                    usuarioDB.IsOnline = false;
                }

                _banco.Usuarios.Update(usuarioDB);
                _banco.SaveChanges();
                await NotificarMudancaNaListaUsuarios();

                //Remoção da ConnectionId dos Grupos de conversa desse usuário no SignalR.
                var grupos = _banco.Grupos.Where(a => a.Usuarios.Contains(usuarioDB.Email));
                foreach (var connectionId in connectionsId)
                {
                    foreach (var grupo in grupos)
                    {
                        await Groups.RemoveFromGroupAsync(connectionId, grupo.Nome);
                    }
                }
            }
        }

        public async Task ObterListaUsuarios()
        {
            var usuarios = _banco.Usuarios.ToList();
            await Clients.Caller.SendAsync("ReceberListaUsuarios", usuarios);
        }

        public async Task NotificarMudancaNaListaUsuarios()
        {
            var usuarios = _banco.Usuarios.ToList();
            await Clients.All.SendAsync("ReceberListaUsuarios", usuarios);
        }

        public async Task CriarOuAbrirGrupo(string emailUserUm, string emailUserDois)
        {
            string nomeGrupo = CriarNomeGrupo(emailUserUm, emailUserDois);

            Grupo grupo = _banco.Grupos.FirstOrDefault(a => a.Nome == nomeGrupo);
            if (grupo == null)
            {
                grupo = new Grupo();
                grupo.Nome = nomeGrupo;
                //grupo.Usuarios = Jil.JSON.Serialize(new List<string>()
                grupo.Usuarios = JsonConvert.SerializeObject(new List<string>()
                {
                    emailUserUm,
                    emailUserDois
                });

                _banco.Grupos.Add(grupo);
                _banco.SaveChanges();
            }

            //List<string> emails = Jil.JSON.Deserialize<List<string>>(grupo.Usuarios);
            List<string> emails = JsonConvert.DeserializeObject<List<string>>(grupo.Usuarios);
            List<Usuario> usuarios = new List<Usuario>() {
                _banco.Usuarios.First(a => a.Email == emails[0]),
                _banco.Usuarios.First(a => a.Email == emails[1])
            };

            foreach (var usuario in usuarios)
            {
                var json = usuario.ConnectionId;
                try
                {
                    var connectionsId = JsonConvert.DeserializeObject<List<string>>(json);
                    foreach (var connectionId in connectionsId)
                    {
                        await Groups.AddToGroupAsync(connectionId, nomeGrupo);
                    }

                    List<Mensagem> mensagens = new List<Mensagem>();
                    bool IsExistMsg = _banco.Mensagens.Where(a => a.NomeGrupo == nomeGrupo).Count() > 0;
                    if (IsExistMsg)
                    {
                        mensagens = _banco.Mensagens.Where(a => a.NomeGrupo == nomeGrupo).OrderBy(a => a.DataCriacao).ToList();
                        for (int i = 0; i < mensagens.Count; i++)
                        {
                            mensagens[i].Usuario = JsonConvert.DeserializeObject<Usuario>(mensagens[i].UsuarioJson);
                        }
                    }
                    await Clients.Caller.SendAsync("AbrirGrupo", nomeGrupo, mensagens);
                }
                catch (Exception ex) { Console.WriteLine(ex.GetBaseException().Message); }
            }
        }

        public async Task EnviarMensagem(Usuario usuario, string msg, string nomeGrupo)
        {
            Grupo grupo = _banco.Grupos.FirstOrDefault(a => a.Nome == nomeGrupo);

            if (!grupo.Usuarios.Contains(usuario.Email))
            {
                throw new Exception("Usuário não pertence ao grupo!");
            }

            Mensagem mensagem = new Mensagem();
            mensagem.NomeGrupo = nomeGrupo;
            mensagem.Texto = msg;
            mensagem.UsuarioId = usuario.Id;
            mensagem.UsuarioJson = JsonConvert.SerializeObject(usuario);
            mensagem.Usuario = usuario;
            mensagem.DataCriacao = DateTime.Now;

            _banco.Mensagens.Add(mensagem);
            _banco.SaveChanges();

            await Clients.Group(nomeGrupo).SendAsync("ReceberMensagem", mensagem, nomeGrupo);
        }

        private string CriarNomeGrupo(string emailUserUm, string emailUserDois)
        {
            List<string> lista = new List<string>() { emailUserUm, emailUserDois };
            var listaOrdernada = lista.OrderBy(a => a).ToList();

            StringBuilder sb = new StringBuilder();
            foreach (var item in listaOrdernada)
            {
                sb.Append(item);
            }

            return sb.ToString();
        }
    }
}
