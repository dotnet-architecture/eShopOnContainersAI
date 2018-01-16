using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Bots.Bot.API.Services
{
    public interface IProductSearchImageService
    {
        Task<IEnumerable<string>> ClassifyImageAsync(byte[] imageFile);
    }
}
