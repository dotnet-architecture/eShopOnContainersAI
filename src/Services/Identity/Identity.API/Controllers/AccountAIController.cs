using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.eShopOnContainers.Services.Identity.API.Data;
using Microsoft.eShopOnContainers.Services.Identity.API.Extensions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.API.Controllers
{
    [Route("api/v1/[controller]")]
    public class AccountAIController : ControllerBase
    {
        private readonly ApplicationDbContext _applicationContext;

        public AccountAIController(ApplicationDbContext context)
        {
            _applicationContext = context;
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> Dump()
        {
            var users = await _applicationContext.Users
                .Select(c => new {
                    c.CardHolderName, c.CardType, c.City,
                    c.Country, c.Email, c.Id,
                    c.LastName, c.Name, c.PhoneNumber,
                    c.State, c.Street, c.UserName,
                    c.ZipCode })
                .ToListAsync();

            var csvFile = File(Encoding.UTF8.GetBytes(users.FormatAsCSV()), "text/csv");
            csvFile.FileDownloadName = "users.csv";
            return csvFile;
        }
    }
}
