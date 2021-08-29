using Konstantin3.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using RDFEngine;

namespace Konstantin3.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        private static RRecord BuildRRecord(string id, int level, string forbidden)
        {
            RRecord rec = Infobase.engine.GetRRecord(id);
            if (rec == null)
            {
                return null;
            }
            return new RRecord()
            {
                Id = rec.Id,
                Tp = rec.Tp,
                Props = rec.Props.Select<RProperty, RProperty>(p =>
                {
                    if (p is RField)
                    {
                        return new RField() { Prop = p.Prop, Value = ((RField)p).Value };
                    }
                    else if (p is RLink && level>0 && p.Prop!=forbidden)
                    {
                        return new RDirect() { Prop = p.Prop, DRec = BuildRRecord(((RLink)p).Resource,0, null) };
                    }
                    else if (p is RInverseLink && level>1)
                    {
                        return new RInverse() { Prop = p.Prop, IRec = BuildRRecord(((RInverseLink)p).Source,1, p.Prop) };
                    }
                    return null;
                }
                ).Where(p=>p!=null).ToArray()                                                                                                                                  
            };
        }

        public IActionResult Portrait(string id)
        {
            RRecord rec = BuildRRecord(id,2, null);
            if (rec != null)
            {
                return View("Portrait", rec);
            }
            return View("Index");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
