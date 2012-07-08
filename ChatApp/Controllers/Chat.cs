using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using ChatApp.Util;
using CiberChat.Models;
using SignalR.Hubs;

namespace CiberChat.Controllers
{
    public class Chat : Hub, IConnected, IDisconnect
    {
        private readonly DataContext _db = new DataContext();
        private const string DateTimeFormat = "d.MMMM HH:mm:ss";
        private static readonly Dictionary<string, string> _clients = new Dictionary<string, string>();

        public void GetAllMessages()
        {
            var messages = _db.ChatMessages.OrderByDescending(x => x.Id).Take(42).ToList();
            messages.Reverse();

            var jsonMessages = (from chatMessage in messages
                     let time = chatMessage.Time.HasValue
                                    ? ((DateTime) chatMessage.Time).ToString(DateTimeFormat, new CultureInfo("nb-NO"))
                                    : string.Empty
                     select new
                         {
                             chatMessage.User,
                             chatMessage.Message,
                             Time = time
                         }).ToList();
            Caller.addAllMessages(jsonMessages);
        }

        public void Send(ChatMessage chatMessage)
        {
            var tagRegex = new Regex(@"<[^>]+>");
            if (chatMessage.Message.Length > 400 || tagRegex.Match(chatMessage.Message).Success || Context.User == null)
            {
                return;
            }
            chatMessage.Time = DateTime.UtcNow.AddHours(2);
            chatMessage.User = Context.User.Identity.Name;
            _db.ChatMessages.Add(chatMessage);
            _db.SaveChanges();
            Clients.addMessage(new{Time = DateTime.UtcNow.AddHours(2).ToString(DateTimeFormat, new CultureInfo("nb-NO")), chatMessage.User, chatMessage.Message});
        }

        public Task Connect()
        {
            var user = GetCurrentUser();
            if (user != null)
            {
                string newUser;

                if (!_clients.TryGetValue(Context.ConnectionId, out newUser))
                {
                    _clients.Add(Context.ConnectionId, user);
                }
            }
            return Clients.updateOnlineUsers(GetConnectedUsersList());
        }

        private string GetCurrentUser()
        {
            try
            {
                if (!string.IsNullOrEmpty(Context.User.Identity.Name))
                {
                    return Context.User.Identity.Name;
                }
            }
            catch (NullReferenceException)
            {
                Console.WriteLine("User not logged in.");
            }
            return null;
        }

        public Task Reconnect(IEnumerable<string> groups)
        {
            var user = GetCurrentUser();
            if (user != null)
            {
                lock (_clients)
                {
                    string newUser;
                    if (!_clients.TryGetValue(Context.ConnectionId, out newUser))
                    {
                        _clients.Add(Context.ConnectionId, user);
                        return Clients.updateOnlineUsers(GetConnectedUsersList());
                    } 
                }
            }
            return new Task(() => {});
        }

        public Task Disconnect()
        {
            lock (_clients)
            {
                string user;
                if (_clients.TryGetValue(Context.ConnectionId, out user))
                {
                    _clients.Remove(Context.ConnectionId);
                    return Clients.updateOnlineUsers(GetConnectedUsersList());
                }
            }
            return new Task(() => {});
        }

        private static IEnumerable<OnlineUser> GetConnectedUsersList()
        {
            var connectedUsersList = _clients.Values.Select(x => new OnlineUser {username = x}).DistinctBy(x => x.username).ToList();
            return connectedUsersList;
        }

        public class OnlineUser
        {
            public string username { get; set; }
        }
    }
}