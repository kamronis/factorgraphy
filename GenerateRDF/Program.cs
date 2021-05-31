﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace GenerateRDF
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Start GenerateRDF");

            // Сюда будем писать
            // var filestream = File.Open("", FileMode.OpenOrCreate, FileAccess.ReadWrite);
            // Загружаем основу
            XElement dataset = XElement.Parse(
@"<?xml version='1.0' encoding='utf-8'?>
<rdf:RDF xmlns:rdf='http://www.w3.org/1999/02/22-rdf-syntax-ns#'>
</rdf:RDF>
");
            Random rnd = new Random();
            int npersons = 10;
            int np = npersons;
            int nf = npersons * 2;
            int nr = npersons * 6;
            dataset.Add(
                Enumerable.Range(0, np)
                    .Select(i => new XElement("person", new XAttribute("{http://www.w3.org/1999/02/22-rdf-syntax-ns#}about", "p"+i),
                        new XElement("name", new XAttribute("{http://www.w3.org/XML/1998/namespace}lang", "ru"), "и"+i),
                        new XElement("age", "23"))));
            dataset.Add(
                Enumerable.Range(0, nf)
                    .Select(i => new XElement("photo", new XAttribute("{http://www.w3.org/1999/02/22-rdf-syntax-ns#}about", "f" + i),
                        new XElement("name", "dsp" + i))));
            dataset.Add(
                Enumerable.Range(0, nr)
                    .Select(i => new XElement("reflection", new XAttribute("{http://www.w3.org/1999/02/22-rdf-syntax-ns#}about", "r" + i),
                        new XElement("reflected", new XAttribute("{http://www.w3.org/1999/02/22-rdf-syntax-ns#}resource", "p" + rnd.Next(np))),
                        new XElement("indoc", new XAttribute("{http://www.w3.org/1999/02/22-rdf-syntax-ns#}resource", "f" + rnd.Next(nf))))));

            // Проверяю что получилось
            dataset.Save("../../../output.rdf");
        }
    }
}
