using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace _8inServant.Services.Processor
{
    public interface IChatProcessor
    {
        Task<string> GetChatResponse(ulong senderID, string input);
    }
}
