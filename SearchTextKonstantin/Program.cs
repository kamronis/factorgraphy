using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SearchTextKonstantin
{
    class NewRecord
    {
        public string ID { get; set; }
        public string Date { get; set; }
        public string Name { get; set; }
        public string Text { get; set; }

    }
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            Stream stream = new FileStream(@"C:\Users\shish\source\repos\DoingFactography\Resources\chronicle.txt", FileMode.Open, FileAccess.Read);
            StreamReader reader = new StreamReader(stream);
            NewRecord[] lines = reader.ReadToEnd().Split('\n')
                .Select(line =>
                {
                    string[] parts = line.Split("|");
                    return new NewRecord() { Date = parts[0], ID = parts[1], Name = parts[2], Text = parts[3] };

                }
                ).ToArray();
            var query = lines.SelectMany(x =>
            {
                string[] words = x.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                return words.Select(w => new { w, x.ID });
            });

            foreach(var el in query.Take(20))
            {
                Console.WriteLine($"{el.ID} {el.w}");
            }
            Console.WriteLine(query.Count());

            
            var query2 = query.GroupBy(pair => pair.w).Select(x => new { word=x.Key, list = x.Select(y => y.ID).ToArray() });
            foreach (var el in query2.Take(20))
            {
                Console.Write(el.word+" ");
                foreach (var id in el.list)
                {
                    Console.Write($"{id} ");

                }
                Console.WriteLine();
            }
        }

    }
}
