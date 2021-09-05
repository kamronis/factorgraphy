using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using RDFEngine;

namespace FactographyView
{
    public class Infobase
    {
        public static RDFEngine.IEngine engine = null;
        private static XElement ontology = null;
        private static Dictionary<string, string> labels_ru;
        private static Dictionary<string, string> inverse_labels_ru;
        public static void LoadOntology(string path)
        {
            ontology = XElement.Load(path);
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

        // ROntology
        RRecord[] rontology = new RRecord[] 
        { 
            new RRecord 
            { 
                //Tp = "person",
                //Label= "Персона"
            }
        };
    }
}
