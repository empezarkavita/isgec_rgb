using ExposeAPIWithEndpointsCore.eslabs;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using System.Linq;

namespace ExposeAPIWithEndpointsCore.Controllers
{
    [Route("view/[controller]/[action]")]
    public class FirestoreController : Controller
    {

        private readonly eslabsContext _context;

        public FirestoreController(eslabsContext context)
        {
            _context = context;
        }


        [HttpGet]
        public async Task<ViewResult> Index()
        {

            int Id = _context.TblEnbloc.Count();


            TblEnbloc tbl = new TblEnbloc();
            tbl.Id = Id + 1;
            tbl.Name = "Sajid";
            _context.Add(tbl);
            await _context.SaveChangesAsync();


            //   var enbloc =  await _context.Enbloc.ToListAsync();

            //    using (var insertVisitCommand = _connection.CreateCommand())
            //     {
            //         insertVisitCommand.CommandText =
            //             @"INSERT INTO visits (user_ip) values (123)";

            //         await insertVisitCommand.ExecuteNonQueryAsync();
            //     }

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