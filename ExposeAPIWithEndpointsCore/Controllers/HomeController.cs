using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Collections.Generic;
using ExposeAPIWithEndpointsCore.Models;
using Newtonsoft.Json;

namespace ExposeAPIWithEndpointsCore.Controllers
{
    public class HomeController : Controller
    {
        private ContainerContext _context = null;
        public HomeController(ContainerContext context)
        {
            _context = context;
        }
     
         [HttpGet]
        [Route("home/index")]
       
        public IActionResult Index()
        {
            List<Container> data = _context.GetAllContainers();
            return View(data);
        }

          
         [HttpGet]
        [Route("home/test")]
       
        public IActionResult Test()
        {
            List<Container> data = _context.GetAllContainers();
            return View();
        }

        [HttpGet]
        [Route("home/edit")]        
        public IActionResult Edit(int? id)
        {
            Container data = new Container();
            if (id.HasValue)
            {
                data = _context.GetAllContainers().Where(p => p.id == id.Value).FirstOrDefault();
            }
            else
            {
                data = new Container();
            }
            return View("Edit", data);
          
        }
 [HttpPost]
        [Route("home/post")]
       
        public IActionResult Post(Container container)
        {
            if (container.id == 0)
            {
                _context.SaveContainer(container);
            }
            else
            {
                _context.UpdateContainer(container);
            }
            return RedirectToAction("Index");
        }
  [HttpGet]
        [Route("home/delete")]
      
        public IActionResult Delete(int id)
        {
            _context.DeleteContainer(id);

            return RedirectToAction("Index");
        }

    }
}