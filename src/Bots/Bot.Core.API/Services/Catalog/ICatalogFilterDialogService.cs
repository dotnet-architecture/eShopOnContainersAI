using Microsoft.Bot.Builder;
using System.Threading.Tasks;

namespace Microsoft.eShopOnContainers.Bot.API.Services.Catalog
{
    public interface ICatalogFilterDialogService
    {
        Task UpdateCatalogFilterUserStateWithTagsAsync(ITurnContext context);
    }
}
