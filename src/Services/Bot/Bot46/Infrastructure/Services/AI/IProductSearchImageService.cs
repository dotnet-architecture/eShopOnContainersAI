using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bot46.API.Infrastructure.Services
{
    public interface IProductSearchImageService
    {
        Task<IEnumerable<string>> ClassifyImageAsync(byte[] imageFile);
    }
}
