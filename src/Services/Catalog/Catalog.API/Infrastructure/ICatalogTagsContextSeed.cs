using System.Threading.Tasks;

namespace Catalog.API.Infrastructure
{
    public interface ICatalogTagsContextSeed
    {
        Task SeedAsync();
    }
}