using System;

namespace Bot46.API.Infrastructure.Models
{
    [Serializable]
    public class Brand
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public bool IsSelected {get;set;}
    }
}