using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.eShopOnContainers.WebMVC.Services
{
    public interface IComputerVisionService
    {
        Task<IEnumerable<string>> ClassifyImageAsync(byte[] imageFile);
    }
}
