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
        public ViewResult MSC()
        {
            return View();
        }

        [HttpGet]
        public ViewResult ISGEC()
        {
            return View();
        }

        [HttpGet]
        public ViewResult Enbloc()
        {
            return View();
        }


        [HttpGet]
        public ViewResult EnblocData()
        {
            return View();
        }

    }
}