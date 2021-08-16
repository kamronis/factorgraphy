using KonstantinSimpleWebApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using RDFEngine;

namespace KonstantinSimpleWebApp.Controllers
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

        private static string Portrait0(RRecord rec)
        {
            string recstr = $"Id:{rec.Id} Tp:{rec.Tp} ";
            foreach (var prop in rec.Props)
            {
                if (prop is RField)
                {
                    RField field = (RField)prop;
                    recstr += $"{field.Prop}:{field.Value} ";
                }
            }
            return recstr;
        }

        private static string Portrait1(RRecord rec, string forbidden)
        {
            string result = Portrait0(rec);
            foreach (var prop in rec.Props)
            {
                if (prop.Prop == forbidden)
                {
                    continue;
                }
                if (prop is RLink)
                {
                    RLink link = (RLink)prop;
                    RRecord record = Infobase.engine.GetRRecord(link.Resource);
                    if (record != null)
                    {
                        result += $"{link.Prop}:({Portrait0(record)}) ";
                    }
                }
            }
            return result;
        }

        private static string Portrait2(RRecord rec)
        {
            string result = Portrait1(rec, null);
            foreach (var prop in rec.Props)
            {
                if (prop is RInverseLink)
                {
                    RInverseLink rlink = (RInverseLink)prop;
                    RRecord record = Infobase.engine.GetRRecord(rlink.Source);
                    if (record != null)
                    {
                        result += $"{rlink.Prop}:({Portrait1(record, rlink.Prop)})";
                    }
                }
            }
            return result;
        }
        public IActionResult Portrait(string id)
        {
            RDFEngine.RRecord rec = Infobase.engine.GetRRecord(id);
            if (rec == null)
            {
                return BadRequest("rec==null");
            }
            string sportrait = Portrait2(rec);
            sportrait =
                "<div>" +
                    "<table>" +
                        sportrait+
                    "</table>"+
                "</div>";
            return View("Portrait", sportrait);
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
