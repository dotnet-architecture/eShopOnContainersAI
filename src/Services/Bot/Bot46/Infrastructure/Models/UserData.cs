using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;

namespace Bot46.API.Infrastructure.Models
{
    public class UserData
    {
        public UserData()
        {

        }

        public UserData(IEnumerable<Claim> claims)
        {
            UserName = claims
                    .Where(claim => claim.Type.Equals("preferred_username"))
                    .Select(claimData => claimData.Value)
                    .FirstOrDefault();
            Name = claims
                    .Where(claim => claim.Type.Equals("name"))
                    .Select(claimData => claimData.Value)
                    .FirstOrDefault();
            LastName = claims
                    .Where(claim => claim.Type.Equals("last_name"))
                    .Select(claimData => claimData.Value)
                    .FirstOrDefault();
            CardHolderName = claims
                    .Where(claim => claim.Type.Equals("card_holder"))
                    .Select(claimData => claimData.Value)
                    .FirstOrDefault();
            CardNumber = claims
                    .Where(claim => claim.Type.Equals("card_number"))
                    .Select(claimData => claimData.Value)
                    .FirstOrDefault();
            SecurityNumber = claims
                    .Where(claim => claim.Type.Equals("card_security_number"))
                    .Select(claimData => claimData.Value)
                    .FirstOrDefault();
            Expiration = claims
                    .Where(claim => claim.Type.Equals("card_expiration"))
                    .Select(claimData => claimData.Value)
                    .FirstOrDefault();
            City = claims
                    .Where(claim => claim.Type.Equals("address_city"))
                    .Select(claimData => claimData.Value)
                    .FirstOrDefault();
            Country = claims
                    .Where(claim => claim.Type.Equals("address_country"))
                    .Select(claimData => claimData.Value)
                    .FirstOrDefault();
            State = claims
                    .Where(claim => claim.Type.Equals("address_state"))
                    .Select(claimData => claimData.Value)
                    .FirstOrDefault();
            Street = claims
                    .Where(claim => claim.Type.Equals("address_street"))
                    .Select(claimData => claimData.Value)
                    .FirstOrDefault();
            ZipCode = claims
                    .Where(claim => claim.Type.Equals("address_zip_code"))
                    .Select(claimData => claimData.Value)
                    .FirstOrDefault();
            Email = claims
                    .Where(claim => claim.Type.Equals("email"))
                    .Select(claimData => claimData.Value)
                    .FirstOrDefault();
            PhoneNumber = claims
                    .Where(claim => claim.Type.Equals("phone_number"))
                    .Select(claimData => claimData.Value)
                    .FirstOrDefault();
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
    }
}