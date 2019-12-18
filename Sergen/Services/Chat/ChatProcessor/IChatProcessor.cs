using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sergen.Services.Chat.ChatResponseToken;

namespace  Sergen.Services.Chat.ChatProcessor
{
    public interface IChatProcessor
    {
        Task ProcessMessage (IChatResponseToken _responder, ulong senderID, string input);
    }
}