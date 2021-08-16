using FactographyView.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FactographyView.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }
        public IActionResult Index(string id)
        {
            return View("Index", id);
        }

        public IActionResult Person(string id)
        {
            var model = Infobase.engine.GetRRecord(id);
            return View("Person", model);
        }
        public IActionResult Portrait(string id)
        {
            var model = Infobase.engine.GetRRecord(id);
            return View("Portrait", model);
        }
        //public IActionResult ShowTree(string id)
        //{
        //    var model = Infobase.engine.GetRTree(id, 2, null);
        //    return View("ShowTree", model);
        //}

        private static string BuildPortraitText(string id, int level, string forbidden)
        {
            var rec = Infobase.engine.GetRRecord(id);
            if (rec == null) return null;
            string result = "{Id:" + rec.Id + ", Tp:" + rec.Tp + ", " +
                rec.Props.Select<RDFEngine.RProperty, string>(p =>
                {
                    if (p is RDFEngine.RField) return p.Prop + ":" + ((RDFEngine.RField)p).Value;
                    else if (level > 0 && p is RDFEngine.RLink && p.Prop != forbidden) return p.Prop + ":" + BuildPortraitText(((RDFEngine.RLink)p).Resource, 0, null);
                    else if (level > 1 && p is RDFEngine.RInverseLink) return p.Prop + ":" + BuildPortraitText(((RDFEngine.RInverseLink)p).Source, 1, p.Prop);
                    return null;
                }).Where(s => s != null).Aggregate((a, s) => a + ", " + s) + "}";
            return result;
        }
        public IActionResult Portrait1(string id)
        {
            var model = BuildPortraitText(id, 2, null);
            return View("Portrait1", model);
        }

        private static RDFEngine.RRecord BuildPortrait(string id, int level, string forbidden)
        {
            var rec = Infobase.engine.GetRRecord(id);
            if (rec == null) return null;
            RDFEngine.RRecord result_rec = new RDFEngine.RRecord()
            {
                Id = rec.Id,
                Tp = rec.Tp,
                Props = rec.Props.Select<RDFEngine.RProperty, RDFEngine.RProperty>(p => 
                {
                    if (p is RDFEngine.RField)
                        return new RDFEngine.RField() { Prop = p.Prop, Value = ((RDFEngine.RField)p).Value };
                    else if (level > 0 && p is RDFEngine.RLink && p.Prop != forbidden)
                        return new RDFEngine.RDirect() { Prop = p.Prop, DRec = BuildPortrait(((RDFEngine.RLink)p).Resource, 0, null) };
                    else if (level > 1 && p is RDFEngine.RInverseLink)
                        return new RDFEngine.RInverse() { Prop = p.Prop, IRec = BuildPortrait(((RDFEngine.RInverseLink)p).Source, 1, p.Prop) };
                    return null;
                }).Where(p => p != null).ToArray()
            };
            return result_rec;
        }
        public IActionResult Portrait2(string id)
        {
            var model = BuildPortrait(id, 2, null);
            return View("Portrait2", model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

