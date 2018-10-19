using Microsoft.Bot.Builder;
using System.Threading.Tasks;

namespace Microsoft.eShopOnContainers.Bot.API.Services.LUIS
{
    public interface ILuisService
    {
        Task<eShopLuisResult> GetResultAsync(ITurnContext context);
    }
}
