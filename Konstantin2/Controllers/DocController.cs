using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Konstantin2.Controllers
{
    public class DocController : Controller
    {
        public IActionResult Index(string u, bool size)
        {
            string cas_name = Infobase.config.Element("LoadCassette").Value;
            string filename = null;
            if (size) // маленькая каритинка
            {
                filename = cas_name + $"/documents/small/0001/{u.Substring(u.Length - 4)}.jpg";
            }
            else  // большая картинка
            {
                filename = cas_name + $"/documents/normal/0001/{u.Substring(u.Length - 4)}.jpg";
            }           
            return new PhysicalFileResult(filename, "image/jpg");
        }

    }
}
