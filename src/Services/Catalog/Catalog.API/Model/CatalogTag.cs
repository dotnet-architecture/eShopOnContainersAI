using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace Microsoft.eShopOnContainers.Services.Catalog.API.Model
{
    public class CatalogTag
    {
        [BsonIgnoreIfDefault]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public int ProductId { get; set; }
        public string Description { get; set; }

        [JsonProperty("color")]
        public string[] Color { get; set; }
        [JsonProperty("size")]
        public string[] Size { get; set; }
        [JsonProperty("quantity")]
        public string[] Quantity { get; set; }
        [JsonProperty("shape")]
        public string[] Shape { get; set; }

        [JsonProperty("tagrams")]
        public string[] Tagrams { get; set; }
    }
}
