using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

//using RDFEngine;

namespace ConsoleTester
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Start ConsoleTester");
            string path_to_resources = "../../../../Resources";

            //CronicleToLines(path_to_resources); // Это предварительные вычисления
            //Dictionary<string, string> zaliznyak = ProcessZaliznyakDic(path_to_resources); // Это если нужен словарь Зализняка

            // Читаем тестовый набор документов в список
            List<ChronNews> records = new List<ChronNews>();
            using (TextReader tr = new StreamReader(new FileStream(path_to_resources + "/chronicle.txt", FileMode.Open, FileAccess.Read)))
            {
                string line = null;
                while ((line=tr.ReadLine()) != null)
                {
                    string[] parts = line.Split('|');
                    records.Add(new ChronNews { Dt = parts[0], Id = parts[1], Nm = parts[2], Tx = parts[3] });
                }
            }
            Console.WriteLine("lines ok. items: " + records.Count);

            // Из поля Tx документов выделим отдельные слова, создадим поток пар {слово, ид документа}
            char[] separators = new char[] { ' ', '.', ',', '!', '?', '\'', '\"', '(', ')', '[', ']', '{', '}', '=', '+', '«', '»' };
            var worddocpair_flow = records.SelectMany<ChronNews, KeyValuePair<string, string>>(news =>
                {
                    IEnumerable<string> words = news.Tx.ToLower().Split(separators, StringSplitOptions.RemoveEmptyEntries);
                    return words.Select(w => new KeyValuePair<string, string>(w, news.Id));
                });
            // Превращаем в словарь id -> doc
            Dictionary<string, ChronNews> docs = records.ToDictionary(r => r.Id);
            // Группируем по словам, превращаем в словарь
            Dictionary<string, string[]> worddocs_dictionary = worddocpair_flow
                .GroupBy(pair => pair.Key)
                .ToDictionary(gr => gr.Key, gr => gr.Select(p => p.Value).Distinct().ToArray());
            Console.WriteLine("dictionary ok. entries: " + worddocs_dictionary.Count);

            // Пусть есть поисковый образ
            string sentense = "дифференциальные уравнения Лаврентьев";
            sentense = "Вычислительный центр Яненко НГУ СО АН СССР Механико-математический факультет Академгородок Дом ученых"; 
            // Разобъем предложение на слова
            var query1 = sentense.ToLower().Split(separators, StringSplitOptions.RemoveEmptyEntries)
                // Каждому слову ставим в соответствие идентификаторы документов, в которых слово встречается, формируем поток пар
                .SelectMany<string, WordDocIdPair>(word =>
                {
                    if (!worddocs_dictionary.ContainsKey(word)) return Enumerable.Empty<WordDocIdPair>();
                    return worddocs_dictionary[word].Select(d => new WordDocIdPair { Wd = word, Id = d });
                })
                // Группируем по документам
                .GroupBy(pa => pa.Id)
                .Select(gr => new { doc = gr.Key, nwords = gr.Count() })
                // Сортируем по числу слов
                .OrderByDescending(dn => dn.nwords)
                ;
            // Распечатаем
            foreach (var q in query1.Take(10))
            {
                Console.Write($"{q.doc} {q.nwords} ");
                Console.WriteLine(docs[q.doc].Tx);
            }

            return;

            /*
    object[] arr = ...
    IEnumerable<object> flow1 = arr;
            
    var flow2 = ((XElement)xdb).Elements();
            
    IEnumerable<object> flow3 = File.ReadLines("file");
            
    System.IO.BinaryReader br = ...;
    IEnumerable<object> flow4() { yield return br.ReadInt32(); };
            */
        }
        // Объект "новость хроники"
        class ChronNews
        {
            public string Id { get; set; }
            public string Dt { get; set; }
            public string Nm { get; set; }
            public string Tx { get; set; }
        }
        // Пара слово - идентификатор документа, в котором это слово имеется
        class WordDocIdPair
        {
            public string Wd { get; set; }
            public string Id { get; set; }
        }

        private static Dictionary<string, string> ProcessZaliznyakDic(string path_to_resources)
        {
            Dictionary<string, string> zaliz = new Dictionary<string, string>();
            // Откроем файл со словарем Зализняка
            TextReader tr = new StreamReader(path_to_resources + "/zaliznyak_shortform.txt");
            string line = null;
            while ((line = tr.ReadLine()) != null)
            {
                string[] arr = line.Split(' ');
                if (arr.Length == 1) continue;
                string norm = arr[0];
                for (int i = 1; i < arr.Length; i++)
                {
                    if (!zaliz.ContainsKey(arr[i])) zaliz.Add(arr[i], norm);
                }
            }
            Console.WriteLine("z-dic ok. nwords=" + zaliz.Count);
            tr.Close();
            return zaliz;
        }

        private static void CronicleToLines(string path_to_resources)
        {
            // Подготовим файл с хрониками Сибирского отделения. Одна строка - одна новость. Вначале идет дата, потом, через пробел - новость.
            var v = System.Text.CodePagesEncodingProvider.Instance;
            System.Text.Encoding.RegisterProvider(v);

            XElement xdoc = XElement.Load(path_to_resources + "/chronicle.xml");
            XElement[] docs = xdoc.Elements("document")
                .Where(d => d.Attribute("{http://www.w3.org/1999/02/22-rdf-syntax-ns#}about").Value != "c_do_chronicles")
                .ToArray();
            // Это путь для определения по обратной ссылки in-doc прямую ссылку reflected
            var dictionary1 = xdoc.Elements("reflection")
                .Where(r => r.Element("in-doc").Attribute("{http://www.w3.org/1999/02/22-rdf-syntax-ns#}resource").Value != "c_do_chronicles")
                .ToDictionary(
                r => r.Element("in-doc").Attribute("{http://www.w3.org/1999/02/22-rdf-syntax-ns#}resource").Value,
                r => r.Element("reflected").Attribute("{http://www.w3.org/1999/02/22-rdf-syntax-ns#}resource").Value);
            // Словарь организаций
            var dictionary2 = xdoc.Elements("org-sys")
                .ToDictionary(o => o.Attribute("{http://www.w3.org/1999/02/22-rdf-syntax-ns#}about").Value);
            // Каждый документ превращается в строку дата|идентификатор|заголовок|Текст
            var query = docs.Select(d =>
            {
                string id = d.Attribute("{http://www.w3.org/1999/02/22-rdf-syntax-ns#}about").Value;
                string oid = dictionary1[id];
                XElement oel = dictionary2[oid];
                return oel.Element("from-date").Value + "|" + id + "|" + d.Element("name").Value + "|" + d.Element("doc-content").Value.Replace('\n', ' ');
            });

            foreach (var s in query.Take(10))
            {
                Console.WriteLine(s);
            }
            Console.WriteLine("docs: " + docs.Length);

            // Запись в файл
            TextWriter tw = new StreamWriter(new FileStream(path_to_resources + "/chronicle.txt", FileMode.OpenOrCreate, FileAccess.Write));
            foreach (var s in query)
            {
                tw.WriteLine(s);
            }
            tw.Close();
        }
    }
}
