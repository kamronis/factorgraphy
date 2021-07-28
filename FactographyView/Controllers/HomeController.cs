﻿using FactographyView.Models;
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
        public IActionResult ShowTree(string id)
        {
            var model = Infobase.engine.GetRTree(id, 2, null);
            return View("ShowTree", model);
        }



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
