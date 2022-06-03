using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace ConsoleTransformer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Start ConsoleTransformer.");
            // Создаем базу данных через конфигуратор
            OAData.OADB.directreload = false;
            OAData.OADB.Init("../../../");
            // Директория изучаемой кассеты
            string cass_dir = @"E:\FactographProjects\PA_cassettes\gi_pharchive";
            // имя кассеты
            string cass_name = "gi_pharchive";
            // Попробую выделить все документы и найти нужные
            var qu = OAData.OADB.GetAll()
                .Where(xrec => false 
                || xrec.Attribute("type").Value == "http://fogid.net/o/document"
                || xrec.Attribute("type").Value == "http://fogid.net/o/photo-doc"
                || xrec.Attribute("type").Value == "http://fogid.net/o/video-doc"
                || xrec.Attribute("type").Value == "http://fogid.net/o/audio-doc"
                )
                .Where(xrec =>
                {
                    var u = xrec.Elements("field").FirstOrDefault(f => f.Attribute("prop").Value == "http://fogid.net/o/uri");
                    if (u != null && u.Value.StartsWith("iiss://" + cass_name + "@")) return true; //  
                    return false;
                })
                ;
            Console.WriteLine("total=" + qu.Count());

            string[] uris = qu.Select(xrec => xrec.Elements("field")
                .FirstOrDefault(f => f.Attribute("prop").Value == "http://fogid.net/o/uri").Value)
                .ToArray();
            XElement[] xels = qu.ToArray();
            if (uris.Length != xels.Length) throw new Exception("22222");
            Array.Sort(uris, xels);

            List<string> dbles = new List<string>();
            System.Collections.Generic.HashSet<string> hash = new HashSet<string>();
            for (int i=0; i<uris.Length; i++)
            {
                var r = uris[i];
                if (hash.Contains(r))
                {
                    dbles.Add(xels[i].Attribute("id").Value);
                    Console.WriteLine(xels[i]);
                }
                hash.Add(r);
            }

            foreach (var u in dbles)
            {
                Console.WriteLine(u);
            }

            string patonId = "svet_100616111408_21334";
            var rec = OAData.OADB.GetItemByIdBasic(patonId, false);
            Console.WriteLine(rec.ToString());

        }
    }
}
