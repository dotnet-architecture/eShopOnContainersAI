using System.Collections.Generic;
using System.Threading.Tasks;

namespace Catalog.API.AI
{
    public interface IComputerVisionService
    {
        Task<IEnumerable<string>> AnalyzeImageAsync(byte[] image);
    }
}