using System.Collections.Generic;
using System.Threading.Tasks;

namespace Catalog.API.ServicesAI
{
    public interface IComputerVisionService
    {
        Task<IEnumerable<string>> AnalyzeImageAsync(byte[] image);
    }
}