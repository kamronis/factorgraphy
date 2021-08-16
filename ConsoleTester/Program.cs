using System;
using RDFEngine;

namespace ConsoleTester
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Start ConsoleTester");
            var engine = new RDFEngine.REngine();
            engine.Load(RDFEngine.PhototekaGenerator.Generate(100));
            engine.Build();
            var result = engine.GetRTree("p66", 0, null);
            Console.WriteLine(result.ToString());
            result = engine.GetRTree("p66", 1, null);
            Console.WriteLine(result.ToString());
            result = engine.GetRTree("p66", 2, null);
            Console.WriteLine(result.ToString());

        }
    }
}
