using Microsoft.AspNetCore.Mvc;

namespace ExposeAPIWithEndpointsCore.Controllers
{
    [Route("view/[controller]/[action]")]
    public class FirestoreController : Controller
    {

        [HttpGet]
        public ViewResult Index()
        {
            return View();
        }

         [HttpGet]
        public ViewResult Yard()
        {
            return View();
        }

         [HttpGet]
        public ViewResult Container()
        {
            return View();
        }

    }
}