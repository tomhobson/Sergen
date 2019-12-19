using System.Threading.Tasks;

namespace Sergen.Core.Services.IpGetter
{
    public interface IIPGetter
    {
        Task<string> GetIP();
    }
}