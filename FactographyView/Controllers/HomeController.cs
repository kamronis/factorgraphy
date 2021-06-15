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
            if (id == null) return View("Stend");
            XElement model = Infobase.engine.GetRecordBasic(id, true, null);
            XElement model2 = new XElement("record", new XAttribute(model.Attribute("id")), new XAttribute(model.Attribute("type")),
                model.Elements("field").Select(f => new XElement(f)),
                model.Elements("inverse").Select(i =>
                {
                    string prop = i.Attribute("prop").Value;
                    string idd = i.Element("record").Attribute("id").Value;
                    XElement mod = Infobase.engine.GetRecordBasic(idd, false, prop);
                    return new XElement("inverse", new XAttribute("prop", prop), mod);
                }),
                null);
            return View("Index", model2);
        }
        public IActionResult Person(string id)
        {
            XElement model = Infobase.engine.GetRecordBasic(id, true, null);
            XElement model2 = new XElement("record", new XAttribute(model.Attribute("id")), new XAttribute(model.Attribute("type")),
                model.Elements("field").Select(f => new XElement(f)),
                model.Elements("inverse").Select(i =>
                {
                string prop = i.Attribute("prop").Value;
                string idd = i.Element("record").Attribute("id").Value;
                XElement mod = Infobase.engine.GetRecordBasic(id, false, prop);
                return new XElement("inverse", new XAttribute("prop", prop), mod);
                }), 
                null);
            return View("Person", model);
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
