using System;

namespace Microsoft.Bots.Bot.API.Models.Catalog
{
    [Serializable]
    public class Brand
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public bool IsSelected {get;set;}
    }
}