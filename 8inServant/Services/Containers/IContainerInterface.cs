using System.Collections.Generic;

namespace _8inServant.Services.Containers
{
    public interface IContainerInterface
    {
        System.Threading.Tasks.Task<IList<string>> GetRunningContainers();    
    }
}