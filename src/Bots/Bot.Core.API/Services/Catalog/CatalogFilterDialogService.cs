using Microsoft.Bot.Builder;
using Microsoft.eShopOnContainers.Bot.API.Services.Attachment;
using Microsoft.eShopOnContainers.Bot.API.Services.ProductSearchImage;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.eShopOnContainers.Bot.API.Services.Catalog
{
    public class CatalogFilterDialogService : ICatalogFilterDialogService
    {
        private readonly DomainPropertyAccessors accessors;
        private readonly IProductSearchImageService productSearchImageService;
        private readonly IAttachmentService attachmentService;

        public CatalogFilterDialogService(DomainPropertyAccessors accessors, IProductSearchImageService productSearchImageService, IAttachmentService attachmentService)
        {
            this.accessors = accessors;
            this.productSearchImageService = productSearchImageService;
            this.attachmentService = attachmentService;
        }

        public async Task UpdateCatalogFilterUserStateWithTagsAsync(ITurnContext context)
        {
            var catalogFilter = await accessors.CatalogFilterProperty.GetAsync(context, () => new CatalogFilterData());
            var imageFile = await attachmentService.DownloadAttachmentFromActivityAsync(context.Activity);
            var tags = Enumerable.Empty<string>();
            if (imageFile != null)
            {
                tags = await productSearchImageService.ClassifyImageAsync(imageFile);
            }
            catalogFilter.Tags = tags.ToArray();
            await accessors.CatalogFilterProperty.SetAsync(context, catalogFilter);
        }
    }
}
