using System.Threading.Tasks;

namespace Sergen.Core.Services.IpGetter
{
    public interface IIpGetter
    {
        Task<string> GetIp();
    }
}