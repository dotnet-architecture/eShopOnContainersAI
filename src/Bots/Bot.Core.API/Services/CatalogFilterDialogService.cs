using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.eShopOnContainers.Bot.API.Services
{
    public interface ICatalogFilterDialogService
    {
        Task UpdateCatalogFilterUserStateWithTagsAsync(ITurnContext context);
    }

    public class CatalogFilterDialogService : ICatalogFilterDialogService
    {
        private readonly IProductSearchImageService productSearchImageService;
        private readonly IAttachmentService attachmentService;
        private readonly AppSettings appSettings;

        public CatalogFilterDialogService(IOptions<AppSettings> appSettings, IProductSearchImageService productSearchImageService, IAttachmentService attachmentService)
        {
            this.productSearchImageService = productSearchImageService;
            this.attachmentService = attachmentService;
            this.appSettings = appSettings.Value;
        }

        public async Task UpdateCatalogFilterUserStateWithTagsAsync(ITurnContext context)
        {
            var userState = context.GetUserState<UserInfo>();
            var imageFile = await attachmentService.DownloadAttachmentFromActivityAsync(context.Activity);
            var tags = Enumerable.Empty<string>();
            if (imageFile != null)
            {
                tags = await productSearchImageService.ClassifyImageAsync(imageFile);
            }
            if (userState.CatalogFilter == null)
                userState.CatalogFilter = new CatalogFilterData();
            userState.CatalogFilter.Tags = tags.ToArray();
        }
    }
}
