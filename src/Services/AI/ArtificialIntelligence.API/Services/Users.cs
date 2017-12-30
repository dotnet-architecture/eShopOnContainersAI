using Microsoft.eShopOnContainers.BuildingBlocks.Resilience.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArtificialIntelligence.API.Services
{
    public interface IUsers
    {
        Task<IEnumerable<Users.UserSchema>> GetUsersAsync();
    }

    public class Users : IUsers
    {
        private readonly string remoteServiceBaseUrl;
        private readonly IHttpClient httpClient;

        public Users(IOptionsSnapshot<ArtificialIntelligenceSettings> settings, IHttpClient httpClient)
        {
            remoteServiceBaseUrl = $"{settings.Value.IdentityUrl}/api/v1/AccountAI/";
            this.httpClient = httpClient;
        }

        public async Task<IEnumerable<UserSchema>> GetUsersAsync()
        {
            var allUsersUri = Infrastructure.API.Identity.GetAll(remoteServiceBaseUrl);

            // TODO: use request with header: Accept-Encoding: gzip
            var dataString = await httpClient.GetStringAsync(allUsersUri);

            var response = JsonConvert.DeserializeObject<IEnumerable<UserSchema>>(dataString);

            return response;
        }

        public class UserSchema
        {
            public string CardHolderName { get; set; }
            public string CardType { get; set; }
            public string City { get; set; }
            public string Country { get; set; }
            public string Email { get; set; }
            public string Id { get; set; }
            public string LastName { get; set; }
            public string Name { get; set; }
            public string PhoneNumber { get; set; }
            public string State { get; set; }
            public string Street { get; set; }
            public string UserName { get; set; }
            public string ZipCode { get; set; }
        }
    }
}
