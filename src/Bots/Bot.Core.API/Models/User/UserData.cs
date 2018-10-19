using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Microsoft.eShopOnContainers.Bot.API.Models.User
{
    public static class ClaimsExtensions
    {
        public static string FirstOrDefaultClaimType(this IEnumerable<Claim> self, string claimType)
        {
            return self.Where(claim => claim.Type.Equals(claimType))
                    .Select(claimData => claimData.Value)
                    .FirstOrDefault();
        }
    }

    public class UserData
    {
        private const string userNameKey = "preferred_username";
        private const string nameKey = "name";
        private const string lastNameKey = "last_name";
        private const string cardHolderKey = "card_holder";
        private const string cardNumberKey = "card_number";
        private const string cardSecurityNumberKey = "card_security_number";
        private const string cardExpirationKey = "card_expiration";
        private const string cityKey = "address_city";
        private const string countryKey = "address_country";
        private const string stateKey = "address_state";
        private const string streetKey = "address_street";
        private const string zipCodeKey = "address_zip_code";
        private const string emailKey = "email";
        private const string phoneNumberKey = "phone_number";
        private const string userApplicationId = "sub";

        public UserData()
        {

        }

        public UserData(IEnumerable<Claim> claims)
        {
            UserName = claims.FirstOrDefaultClaimType(userNameKey);
            Name = claims.FirstOrDefaultClaimType(nameKey);
            LastName = claims.FirstOrDefaultClaimType(lastNameKey);
            CardHolderName = claims.FirstOrDefaultClaimType(cardHolderKey);
            CardNumber = claims.FirstOrDefaultClaimType(cardNumberKey);
            SecurityNumber = claims.FirstOrDefaultClaimType(cardSecurityNumberKey);
            Expiration = claims.FirstOrDefaultClaimType(cardExpirationKey);
            City = claims.FirstOrDefaultClaimType(cityKey);
            Country = claims.FirstOrDefaultClaimType(countryKey);
            State = claims.FirstOrDefaultClaimType(stateKey);
            Street = claims.FirstOrDefaultClaimType(streetKey);
            ZipCode = claims.FirstOrDefaultClaimType(zipCodeKey);
            Email = claims.FirstOrDefaultClaimType(emailKey);
            PhoneNumber = claims.FirstOrDefaultClaimType(phoneNumberKey);
            UserApplicationId = claims.FirstOrDefaultClaimType(userApplicationId);
        }

        public string UserName { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public string CardNumber { get; set; }
        public string CardHolderName { get; set; }
        public string SecurityNumber { get; set; }
        public string Expiration { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string State { get; set; }
        public string Street { get; set; }
        public string ZipCode { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string UserApplicationId { get; set; }
    }
}