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

namespace Microsoft.eShopOnContainers.Services.AI.ProductSearchImageBased.CNTK.API.Controllers
{
    [Route("api/productSearchImage")]
    public class ProductSearchImageBasedController : ApiController
    {
        public ProductSearchImageBasedController()
        {
        }

        [HttpPost]
        [Route("classifyImage")]
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
                    tags = await modelPrediction.ClassifyImageAsync(image.ToArray());
                }

                return Ok(tags.Select(t => t.Label));
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }
        }
    }
}