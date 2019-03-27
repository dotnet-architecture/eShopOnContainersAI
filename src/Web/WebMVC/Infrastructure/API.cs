using System;
using System.Collections.Generic;
using System.Linq;

namespace WebMVC.Infrastructure
{
    public static class API
    {

        public static class Purchase
        {
            public static string AddItemToBasket(string baseUri) => $"{baseUri}/basket/items";
            public static string UpdateBasketItem(string baseUri) => $"{baseUri}/basket/items";

            public static string GetOrderDraft(string baseUri, string basketId) => $"{baseUri}/order/draft/{basketId}";
        }

        public static class Basket
        {
            public static string GetBasket(string baseUri, string basketId) => $"{baseUri}/{basketId}";
            public static string UpdateBasket(string baseUri) => baseUri;
            public static string CheckoutBasket(string baseUri) => $"{baseUri}/checkout";
            public static string CleanBasket(string baseUri, string basketId) => $"{baseUri}/{basketId}";
        }

        public static class Order
        {
            public static string GetOrder(string baseUri, string orderId)
            {
                return $"{baseUri}/id/{orderId}";
            }

            public static string GetAllMyOrders(string baseUri, string userId)
            {
                return $"{baseUri}/user/{userId}";
            }

            public static string AddNewOrder(string baseUri)
            {
                return $"{baseUri}/new";
            }

            public static string CancelOrder(string baseUri)
            {
                return $"{baseUri}/cancel";
            }

            public static string ShipOrder(string baseUri)
            {
                return $"{baseUri}/ship";
            }
        }

        public static class Catalog
        {
            public static string GetAllCatalogItems(string baseUri, int page, int take, int? brand, int? type)
            {
                var filterQs = "";

                if (type.HasValue)
                {
                    var brandQs = (brand.HasValue) ? brand.Value.ToString() : string.Empty;
                    filterQs = $"/type/{type.Value}/brand/{brandQs}";

                }
                else if (brand.HasValue)
                {
                    var brandQs = (brand.HasValue) ? brand.Value.ToString() : string.Empty;
                    filterQs = $"/type/all/brand/{brandQs}";
                }
                else
                {
                    filterQs = string.Empty;
                }

                return $"{baseUri}items{filterQs}?pageIndex={page}&pageSize={take}";
            }

            public static string GetAllBrands(string baseUri)
            {
                return $"{baseUri}catalogBrands";
            }

            public static string GetAllTypes(string baseUri)
            {
                return $"{baseUri}catalogTypes";
            }
        }

        public static class CatalogAI
        {
            public static string GetProducSetDetailsByIDs(string baseUri, string productId, IEnumerable<string> productIDs)
            {
                return $"{baseUri}productSetDetailsByIDs?productId={productId}&productIDs={String.Join(',',productIDs)}";
            }

            public static string GetAllCatalogItems(string baseUri, int page, int take, int? brand, int? type, IEnumerable<string> tags)
            {
                var brandQs = (brand.HasValue) ? $"&catalogBrandId={brand.Value.ToString()}" : String.Empty;
                var typeQs = (type.HasValue) ? $"&catalogTypeId={type.Value.ToString()}" : String.Empty;
                var tagsQs = (tags != null && tags.Any()) ? $"&tags={String.Join(',', tags)}" : String.Empty; 

                return $"{baseUri}items?pageIndex={page}&pageSize={take}{brandQs}{typeQs}{tagsQs}";
            }
        }

        public static class ProductRecommender
        {
            public static string GetRecommendProducts(string baseUri,string productId, string customerId)
            {
                return $"{baseUri}recommendProducts?productId={productId}&customerId={customerId}";
            }
        }

        public static class ProductImageSearch
        {
            public static string ClassifyImage(string baseUri)
            {                
                return $"{baseUri}classifyImage";
            }
        }

        public static class Marketing
        {
            public static string GetAllCampaigns(string baseUri, int take, int page)
            {
                return $"{baseUri}user?pageSize={take}&pageIndex={page}";
            }

            public static string GetAllCampaignById(string baseUri, int id)
            {
                return $"{baseUri}{id}";
            }
        }

        public static class Locations
        {
            public static string CreateOrUpdateUserLocation(string baseUri)
            {
                return baseUri;
            }
        }
    }
}