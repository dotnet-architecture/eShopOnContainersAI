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

        [JsonProperty("tagrams")]
        public string[] Tagrams { get; set; }
    }

    public class CatalogFullTag : CatalogTag
    {
        [JsonProperty("color")]
        public string[] Color { get; set; }
        [JsonProperty("size")]
        public string[] Size { get; set; }
        [JsonProperty("quantity")]
        public string[] Quantity { get; set; }
        [JsonProperty("shape")]
        public string[] Shape { get; set; }

        public string agram { get; set; }
        public string bgram { get; set; }
        public string abgram { get; set; }
        public string ygram { get; set; }
        public string zgram { get; set; }
        public string yzgram { get; set; }
    }
}
