using System.Collections.Generic;
using System.Threading.Tasks;
using Sergen.Master.Data;

namespace Sergen.Master.Services.Chat
{
    public interface IChatAllowList
    {
        Task SetAllowListStatus(string serverId, bool enabled);
        
        Task<bool> IsUserAllowed(string serverId, string UserId);
        
        Task<bool> IsUserAllowedToManage(string serverId, string UserId);

        Task<bool> AddUser(string serverId, string UserId);
        
        Task<bool> RemoveUser(string serverId, string UserId);

        Task<IList<string>> GetAllowList(string serverId);
    }
}