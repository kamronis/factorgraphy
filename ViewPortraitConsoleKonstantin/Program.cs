using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using RDFEngine;

namespace ViewPortraitConsoleKonstantin
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Start ViewPortraitConsoleKonstantin");
            XElement rdf = XElement.Load(@"C:\Home\Konstantin2\family1234.xml");
            engine = new REngine();
            engine.Load(rdf.Elements());
            engine.Build();
            RRecord rec = engine.GetRRecord("famwf1234_1055");
            string recstr = Portrait2(rec);
            Console.WriteLine(recstr);
        }

        private static RDFEngine.REngine engine;
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
                    RRecord record = engine.GetRRecord(link.Resource);
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
            foreach(var prop in rec.Props)
            {
                if(prop is RInverseLink)
                {
                    RInverseLink rlink = (RInverseLink)prop;
                    RRecord record = engine.GetRRecord(rlink.Source);
                    if (record != null)
                    {
                        result += $"{rlink.Prop}:({Portrait1(record, rlink.Prop)})";
                    }
                }
            }
            return result;
        }
    }
}
