using System;
using System.Collections.Generic;
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
            Infobase.engine = new RDFEngine.REngine();



            bool useconfig = false; // Два варианта: использовать config и OAData или через прямую загрузку

            if (useconfig) 
            {
                OAData.OADB.Init(path);
                var xel = OAData.OADB.GetItemByIdBasic("newspaper_28156_1997_1", false);
                var xels = OAData.OADB.SearchByName("марчук").ToArray();
                if (xel != null) Console.WriteLine(xel.ToString());
                //OAData.OADB.Load();

                Infobase.engine = new RDFEngine.RXEngine(); // Это новый движок!!!

                //Infobase.engine.NewRecord("http://fogid.net/o/person", "Пупкин"); // Опробовал 1
                string idd = OAData.OADB.SearchByName("пупкин").FirstOrDefault()?.Attribute("id")?.Value;
                //Infobase.engine.Update(new RRecord
                //{
                //    Id = idd,
                //    Tp = "http://fogid.net/o/person",
                //    Props = new RProperty[]
                //    {
                //        new RField { Prop = "http://fogid.net/o/name", Value = "Пупкин" },
                //        new RField { Prop = "http://fogid.net/o/from-date", Value = "2021-12-29" },
                //        new RField { Prop = "http://fogid.net/o/description", Value = "Описание!" },
                //    }
                //});
                //var xe = OAData.OADB.GetItemByIdBasic("Cassette_20211014_tester_637763849054494762_1003", false);
                //Infobase.engine.DeleteRecord(idd);
            }
            else
            {
                Infobase.cassPath = "C:\\Users\\Kamroni\\Desktop\\SypCassete";
                //Infobase.cassPath = @"D:\Home\FactographProjects\syp_cassettes\SypCassete"; // Это на машине mag
                XElement graphModelXml = XElement.Load(Infobase.cassPath + "\\meta\\SypCassete_current_20110112.fog");
                var flow = graphModelXml.Elements()
                    .Select(el =>
                    {
                        el = new XElement("{http://fogid.net/o/}" + el.Name.LocalName,
                            el.Attributes(),
                            el.Elements().Select(subel => new XElement("{http://fogid.net/o/}" + subel.Name.LocalName, subel.Attributes(), subel.Value)));
                        if (el.Name.LocalName == "photo-doc")
                        {
                            XElement iisStore = el.Element("{http://fogid.net/o/}" + "iisstore");
                            if (iisStore != null)
                            {
                                var kk = new XElement(el.Name,
                                    new XAttribute("{http://www.w3.org/1999/02/22-rdf-syntax-ns#}about", el.Attribute("{http://www.w3.org/1999/02/22-rdf-syntax-ns#}about").Value),
                                    el.Elements().Where(e => e.Name.LocalName != "iisstore")
                                    .Concat(iisStore.Attributes().Where(a => a.Name.LocalName == "uri" || a.Name.LocalName == "documenttype")
                                    .Select(at => new XElement(at.Name, at.Value))));
                                return kk;
                            }
                            else
                            {
                                return el;
                            }
                        }
                        else
                        {
                            return el;
                        }
                    });
                Infobase.engine.Load(flow);
            }
            

            //Infobase.engine.Build();
            Infobase.ront = new RDFEngine.ROntology(path + "ontology_iis-v12-doc_ruen.xml"); // тестовая онтология
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
