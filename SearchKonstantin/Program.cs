using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SearchKonstantin
{
    class Program
    {
        static List<string> list = new List<string>() { "ivanov flkf", "sfksk sdjf sd sdfjkj", "f sf s aefkahk" };
        public static IEnumerable<int> Search(string w)
        {
            //for(int i=0;i<list.Count;i++)
            //  {
            //      if (list[i].Contains(w))
            //      {
            //          yield return i;
            //      }
            //  }

            return list.Select((s, i) => new { s = s, i = i })
                .Where(x => x.s.Contains(w))
                .Select(x => x.i);
        }
        static void Main(string[] args)
        {
            Console.WriteLine("SearchKonstantin");
            var result = Search("flkf");
            foreach(var el in result)
            {
                Console.WriteLine(el);
            }

            List<KeyValuePair<string, int>> pairs = new List<KeyValuePair<string, int>>();


            for(int i = 0; i < list.Count; i++)
            {
                string line = list[i];
                string[] words = line.Split(" ");
                foreach(var w in words)
                {
                    pairs.Add(new KeyValuePair<string, int>(w, i));
                }

                var pairss = pairs.OrderBy(p => p.Key);
                foreach(var el in pairss)
                {
                    Console.WriteLine($"Key is {el.Key} ; Value is {el.Value}");
                }
            }
        }
    }
}
