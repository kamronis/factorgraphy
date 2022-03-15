using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using RDFEngine;

namespace BlazorServer
{
    public class Infobase
    {
        // Движок базы данных
        public static RDFEngine.IEngine engine = null;
        // ROntology - объектная онтология
        public static ROntology ront = null;

        private static Dictionary<string, string> labels_ru;
        private static Dictionary<string, string> inverse_labels_ru;

        public static string cassPath;

        public static void Init(string path)
        {

            OAData.OADB.Init(path);
            //var xel = OAData.OADB.GetItemByIdBasic("newspaper_28156_1997_1", false);
            //var xels = OAData.OADB.SearchByName("марчук").ToArray();
           // if (xel != null) Console.WriteLine(xel.ToString());
            //OAData.OADB.Load();

            Infobase.engine = new RXEngine(); // Это новый движок!!!

            //user here
            ((RXEngine)Infobase.engine).User = "mag_1";

            //Infobase.engine.NewRecord("http://fogid.net/o/person", "Пупкин"); // Опробовал 1
            //string idd = OAData.OADB.SearchByName("пупкин").FirstOrDefault()?.Attribute("id")?.Value;

            //Infobase.engine.Build();
            Infobase.ront = new ROntology(path + "ontology_iis-v12-doc_ruen.xml"); // тестовая онтология
            LoadOntology(path + "ontology_iis-v12-doc_ruen.xml");


        }
        public static void LoadOntology(string path)
        {
            XElement ontology = XElement.Load(path);
            labels_ru = ontology.Elements()
                .SelectMany(el => el.Elements("{http://www.w3.org/2000/01/rdf-schema#}label"))
                .Where(lab => lab.Attribute("{http://www.w3.org/XML/1998/namespace}lang")?.Value == "ru")
                .ToDictionary(lab => lab.Parent.Attribute("{http://www.w3.org/1999/02/22-rdf-syntax-ns#}about").Value,
                    lab => lab.Value);
            inverse_labels_ru = ontology.Elements()
                .SelectMany(el => el.Elements("{http://www.w3.org/2000/01/rdf-schema#}inverse-label"))
                .Where(lab => lab.Attribute("{http://www.w3.org/XML/1998/namespace}lang")?.Value == "ru")
                .ToDictionary(lab => lab.Parent.Attribute("{http://www.w3.org/1999/02/22-rdf-syntax-ns#}about").Value,
                    lab => lab.Value);

            // Альтернативно

            //ront = new ROntology(ROntology.samplerontology);
            labels_ru = ront.rontology
                .Select(d => new { d.Id, ((RField)(d.Props.FirstOrDefault(p => p.Prop == "Label")))?.Value })
                .ToDictionary(pa => pa.Id, pa => pa.Value);
            inverse_labels_ru = ront.rontology
                .Select(d => new { d.Id, ((RField)(d.Props.FirstOrDefault(p => p.Prop == "InvLabel")))?.Value })
                .ToDictionary(pa => pa.Id, pa => pa.Value);
        }
        public static string GetTerm(string id)
        {
            if (!labels_ru.ContainsKey(id)) return id;
            return labels_ru[id];
        }
        public static string GetInvTerm(string id)
        {
            if (!inverse_labels_ru.ContainsKey(id)) return id;
            return inverse_labels_ru[id];
        }

    }
}
