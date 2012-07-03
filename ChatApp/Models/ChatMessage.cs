using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CiberChat.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }
        public string User { get; set; }
        public string Message { get; set; }
    }
}