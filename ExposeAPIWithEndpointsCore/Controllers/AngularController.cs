using Microsoft.AspNetCore.Mvc;

namespace ExposeAPIWithEndpointsCore.Controllers
{
    [Route("view/[controller]/[action]")]
    public class AngularController : Controller
    {

        [HttpGet]
        public ViewResult Index()
        {

            return View();
        }

    }
}