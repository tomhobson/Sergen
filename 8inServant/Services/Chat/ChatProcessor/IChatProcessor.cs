using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using _8inServant.Services.Chat.ChatResponseToken;

namespace  _8inServant.Services.Chat.ChatProcessor
{
    public interface IChatProcessor
    {
        Task ProcessMessage (IChatResponseToken _responder, ulong senderID, string input);
    }
}