using Microsoft.AspNetCore.Mvc;

namespace LT.DigitalOffice.AuthService.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class InfoController
    {
        private readonly string _apiVersion = "1.2.2";

        [HttpGet]
        public string GetApiInformation()
        {
            return _apiVersion;
        }
    }
}
