using Microsoft.eShopOnContainers.Services.AI.ProductSearchImageBased.CNTK.API.Classifier;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Microsoft.eShopOnContainers.Services.AI.ProductSearchImageBased.CNTK.API.Infrastructure;

namespace Microsoft.eShopOnContainers.Services.AI.ProductSearchImageBased.CNTK.API.Controllers
{
    public class ProductSearchImageBasedController : ApiController
    {
        [HttpPost]
        [Route("api/v1/productSearchImage/classifyImage")]
        public async Task<IHttpActionResult> ClassifyImage()
        {
            var modelPrediction = new CNTKModelPrediction(CNTKModelPredictionResources.Resources);

            // Check if the request contains multipart / form - data.
            if (!Request.Content.IsMimeMultipartContent() ||
                HttpContext.Current.Request.Files.Count != 1 ||
                HttpContext.Current.Request.Files[0].ContentLength == 0)
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            try
            {
                var imageFile = HttpContext.Current.Request.Files[0];

                IEnumerable<LabelConfidence> tags = null;
                using (var image = new MemoryStream())
                {
                    await imageFile.InputStream.CopyToAsync(image);
                    var imageData = image.ToArray();
                    if (!imageData.IsValidImage())
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);

                    tags = await modelPrediction.ClassifyImageAsync(image.ToArray());
                }

                return Ok(tags.Select(t => t.Label));
            }
            catch (HttpResponseException)
            {
                throw;
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }
        }
    }
}