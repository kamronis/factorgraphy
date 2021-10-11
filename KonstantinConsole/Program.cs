using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.IO;

namespace KonstantinConsole
{
    class Program
    {
        static void Main()
        {
            //Main2();
            //Main3();
            //Main4();
            Main5();
        }
        static void Main1()
        {
            Console.WriteLine("Start KonstantinConsole");
            string path = @"C:\Home\Data\SypCassete\meta\SypCassete_current.rdf";
            XElement xml = XElement.Load(path);
            XElement result = XElement.Parse("<rdf:RDF xmlns:rdf='http://www.w3.org/1999/02/22-rdf-syntax-ns#'></rdf:RDF>");
            result.Add(xml.Elements().Select(el=> 
            {
                if (el.Name == "photo-doc")
                {
                    return new XElement("photo-doc", new XAttribute(el.Attribute("{http://www.w3.org/1999/02/22-rdf-syntax-ns#}about"))
                        , new XElement("name", el.Element("published").Value), new XElement("uri",el.Element("iisstore").Attribute("uri").Value)
                        );
                }
                else
                {
                    return el;
                }
            }));
            result.Save(@"C:\Home\Data\SypCassete__Test\meta\SypCassete_current_new.rdf");
            foreach(var el in result.Elements("photo-doc").Take(10))
            {
                Console.WriteLine(el.ToString());
            }
        }

        static void Main2()
        {
            Console.WriteLine("Start KonstantinConsole");
            string path = @"C:\Home\Konstantin2\Family.rdf";
            XElement xml = XElement.Load(path);
            XElement result = XElement.Parse("<rdf:RDF xmlns:rdf='http://www.w3.org/1999/02/22-rdf-syntax-ns#'></rdf:RDF>");
            result.Add(xml.Elements().Select(el=>
            {
                if (el.Name == "photo")
                {
                    return new XElement("photo-doc", el.Attribute("{http://www.w3.org/1999/02/22-rdf-syntax-ns#}about"), new XElement("name", el.Element("name").Value), new XElement("uri", el.Element("url").Value));
                }
                else
                {
                    return el;
                }
            }
            ));
            result.Save(@"C:\Home\Data\SypCasseteFamily\meta\SypCassete_current_Family.rdf");
        }
        static void Main3()
        {
            string path = @"C:\Home\Konstantin2\wwwroot\img\";
            int count = Directory.GetFiles(path).Length;
            Console.WriteLine(count);
            string[] filenames = new string[count];
            filenames = Directory.GetFiles(path);
            int number = 0;
            foreach(string filename in filenames)
            {
                string path_new = path + "000" + number.ToString()+".jpg";
                File.Move(filename, path_new);
                number++;
            }
        }

        static void Main4()
        {
            string path = @"C:\Home\Data\SypCasseteFamily\meta\SypCassete_current_Family.rdf";
            XElement xml = XElement.Load(path);
            XElement result = XElement.Parse("<rdf:RDF xmlns:rdf='http://www.w3.org/1999/02/22-rdf-syntax-ns#'></rdf:RDF>");
            
            Guid guid = Guid.NewGuid();
            string uid = guid.ToString().Substring(0,7);
            Console.WriteLine(uid);
        }

        static void Main5()
        {
            XElement xml = XElement.Load(@"C:\Home\Konstantin2\wwwroot\Config.xml");
            IEnumerable<XElement> result = xml.Elements("LoadCassette");
            string[] namecassette = new string[2];
            namecassette = result.Select(a => a.Value).ToArray();
            foreach (XElement el in result)
            {
                Console.WriteLine(el.ToString());
            }
            foreach (string el in namecassette)
            {
                Console.WriteLine(el);
            }

        }
        
    }
}
