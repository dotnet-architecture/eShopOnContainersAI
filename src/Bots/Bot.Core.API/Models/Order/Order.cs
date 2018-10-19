using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;

namespace Microsoft.eShopOnContainers.Bot.API.Models.Order
{
    [Serializable]
    public class Order
    {
        public string OrderNumber { get; set; }

        public DateTime Date { get; set; }

        public string Status { get; set; }

        public decimal Total { get; set; }

        public string Description { get; set; }

        public string City { get; set; }
        public string Street { get; set; }
        public string State { get; set; }
        public string Country { get; set; }

        public string ZipCode { get; set; }
        public string CardNumber { get; set; }

        public string CardHolderName { get; set; }

        public DateTime CardExpiration { get; set; }

        public string CardExpirationShort { get; set; }
        public string CardSecurityNumber { get; set; }

        public int CardTypeId { get; set; }

        public string Buyer { get; set; }

        public List<SelectListItem> ActionCodeSelectList =>
           GetActionCodesByCurrentState();

        // See the property initializer syntax below. This
        // initializes the compiler generated field for this
        // auto-implemented property.
        public List<OrderItem> OrderItems { get; } = new List<OrderItem>();

        public Guid RequestId { get; set; }


        public void CardExpirationShortFormat()
        {
            CardExpirationShort = CardExpiration.ToString("MM/yy");
        }

        public void CardExpirationApiFormat()
        {
            var month = CardExpirationShort.Split('/')[0];
            var year = $"20{CardExpirationShort.Split('/')[1]}";

            CardExpiration = new DateTime(int.Parse(year), int.Parse(month), 1);
        }

        private List<SelectListItem> GetActionCodesByCurrentState()
        {
            var actions = new List<OrderProcessAction>();
            switch (Status?.ToLower())
            {
                case "paid":
                    actions.Add(OrderProcessAction.Ship);
                    break;
            }

            var result = new List<SelectListItem>();
            actions.ForEach(action =>
            {
                result.Add(new SelectListItem { Text = action.Name, Value = action.Code });
            });

            return result;
        }
    }

    public enum CardType
    {
        AMEX = 1
    }
}