using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace  Sergen.Services.Chat.ChatContext
{
    public interface IChatContext
    {
        string GetUsername (ulong userID);
    }
}