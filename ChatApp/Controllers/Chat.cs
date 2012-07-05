using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using CiberChat.Models;
using SignalR.Hubs;

namespace CiberChat.Controllers
{
    public class Chat : Hub
    {
        private readonly DataContext _db = new DataContext();

        public void GetMessages()
        {
            var messages = _db.ChatMessages.OrderByDescending(x => x.Id).Take(42).ToList();
            messages.Reverse();
            foreach (var message in from chatMessage in messages let time = chatMessage.Time.HasValue
                                                                                ? ((DateTime) chatMessage.Time).ToString("d.MMMM HH:mm:ss")
                                                                                : string.Empty select new
                                                                                    {
                                                                                        chatMessage.User,
                                                                                        chatMessage.Message,
                                                                                        Time = time
                                                                                    })
            {
                Caller.addMessage(message);
            }
        }

        public void Send(ChatMessage chatMessage)
        {
            var tagRegex = new Regex(@"<[^>]+>");
            if (chatMessage.Message.Length > 400 || tagRegex.Match(chatMessage.Message).Success)
            {
                return;
            }
            chatMessage.Time = DateTime.UtcNow.AddHours(2);
            _db.ChatMessages.Add(chatMessage);
            _db.SaveChanges();
            Clients.addMessage(new{Time = DateTime.UtcNow.AddHours(2).ToString("d.MMMM HH:mm:ss"), chatMessage.User, chatMessage.Message});
        }
    }
}