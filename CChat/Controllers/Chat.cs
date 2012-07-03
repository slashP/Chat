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
            var messages = _db.ChatMessages.OrderByDescending(x => x.Id).Take(42).Select(x => new { x.User, x.Message }).ToList();
            messages.Reverse();
            messages.ForEach(x => Caller.addMessage(x));
        }

        public void Send(ChatMessage chatMessage)
        {
            var tagRegex = new Regex(@"<[^>]+>");
            if (chatMessage.Message.Length > 400 || tagRegex.Match(chatMessage.Message).Success)
            {
                return;
            }
            _db.ChatMessages.Add(chatMessage);
            _db.SaveChanges();
            Clients.addMessage(chatMessage);
        }
    }
}