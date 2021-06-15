using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using RDFEngine;

namespace TesterConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Start TesterConsole.");

            int npersons = 100;

            // Создадим движок
            RDFEngine.XMLEngine engine = new XMLEngine();
            engine.Load(PhototekaGenerator.Generate(npersons));
            engine.Build();

            // Попробуем поиск
            foreach (var rec in engine.Search("и9"))
            {
                Console.WriteLine(rec.ToString());
            }
            Console.WriteLine("====");

            // Попробуем выборку
            int key = npersons * 2 / 3;
            var record = engine.GetRecordBasic("p" + key, false, null);
            Console.WriteLine(record.ToString());
            Console.WriteLine("====");

            // Испытание на производительность
            Random rnd = new Random();
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            npersons = 10_000;
            engine.Clear();
            engine.Load(PhototekaGenerator.Generate(npersons));
            engine.Build();
            Console.WriteLine("new generation ok.");
            Console.WriteLine("====");

            int nprobes = 1000;

            sw.Start();
            for (int i=0; i<nprobes; i++)
            {
                key = rnd.Next(npersons);
                var r = engine.GetRecordBasic("p" + key, false, null);
            }
            sw.Stop();
            Console.WriteLine($"duration of {nprobes} probes for {npersons} persons: {sw.ElapsedMilliseconds} ms.");
            Console.WriteLine("====");

            nprobes = 100;

            sw.Start();
            int n = 0;
            for (int i = 0; i < nprobes; i++)
            {
                key = rnd.Next(npersons);
                var recs = engine.Search("и" + key);
                n += recs.Count();
                //foreach (var r in recs)
                //{
                //    Console.WriteLine(r.ToString());
                //}
                //Console.WriteLine();
            }
            sw.Stop();
            Console.WriteLine($"n={n} duration of {nprobes} seaches for {npersons} persons: {sw.ElapsedMilliseconds} ms.");
            Console.WriteLine("====");


        }
    }
}


