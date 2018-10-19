using Microsoft.Bot.Schema;
using System.Threading.Tasks;

namespace Microsoft.eShopOnContainers.Bot.API.Services.Attachment
{
    public interface IAttachmentService
    {
        Task<byte[]> DownloadAttachmentFromActivityAsync(Activity self);
    }
}
