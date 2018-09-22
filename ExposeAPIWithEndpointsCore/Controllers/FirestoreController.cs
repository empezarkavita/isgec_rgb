using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace ExposeAPIWithEndpointsCore.Controllers
{
    [Route("view/[controller]/[action]")]
    public class FirestoreController : Controller
    {
        private readonly DbConnection _connection;
        public FirestoreController(DbConnection connection)
        {
            this._connection = connection;
        }


        [HttpGet]
        public async Task<ViewResult> Index()
        {

           using (var insertVisitCommand = _connection.CreateCommand())
            {
                insertVisitCommand.CommandText =
                    @"INSERT INTO visits (user_ip) values (123)";
               
                await insertVisitCommand.ExecuteNonQueryAsync();
            }

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