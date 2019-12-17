using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using _8inServant.Services.Containers;

namespace _8inServant.Services.Processor
{
    public class ChatProcessor : IChatProcessor
    {
        private readonly IChatContext _context;
        private readonly IContainerInterface _containerInterface;

        public ChatProcessor(
            IChatContext context,
            IContainerInterface containerInt)
        {
            _context = context;
            _containerInterface = containerInt;
        }

        public async Task<string> GetChatResponse(ulong senderID, string input)
        {
            switch (input)
            {
                case "-ping":
                    return "pong!";
                case "-whoami":
                    return $"You are: {_context.GetUsername(senderID)}";
                case "-version":
                    System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
                    FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
                    string version = fvi.FileVersion;
                    return $"My version is: {version}";
                case "-dockerps":
                    string allcontainers = "";
                    var allconts = await _containerInterface.GetRunningContainers();
                    foreach (var stri in allconts)
                    {
                        allcontainers = allcontainers + $"\n {stri}";
                    }
                    return $"Running containers are: {allcontainers}";
            }

            return null;
        }
    }
}
