using Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace APIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DevSeedController : ControllerBase
    {
        private readonly MashwarDbContext _db;
        private readonly IWebHostEnvironment _env;

        public DevSeedController(MashwarDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }




    }
}
