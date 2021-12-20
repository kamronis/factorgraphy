using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorServer.Controllers
{
    public class ImageController : Controller
    {
        [HttpGet("docs/GetImage")]
        public IActionResult GetImage(string u, string s)
        {
            if (u == null) return NotFound();
            u = System.Web.HttpUtility.UrlDecode(u);
            var cass_dir = CassDirPath(u);
            if (cass_dir == null) return NotFound();
            string last10 = u.Substring(u.Length - 10);
            string subpath;
            string method = s;
            //if (method == null) subpath = "/originals";
            if (method == "small") subpath = "/documents/small";
            else if (method == "medium") subpath = "/documents/medium";
            else subpath = "/documents/normal"; // (method == "n")
            string path = cass_dir + subpath + last10 + ".jpg";
            return PhysicalFile(path, "image/jpg");
        }

        public static string CassDirPath(string uri)
        {
            if (!uri.StartsWith("iiss://")) throw new Exception("Err: 22233");
            int pos = uri.IndexOf('@', 7);
            if (pos < 8) throw new Exception("Err: 22234");
            return Infobase.cassPath;
        }
    }
}
