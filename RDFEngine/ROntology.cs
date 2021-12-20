using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDFEngine
{
    // Перечисление может быть более эффективным, чем строки
    // public enum RVid { RClass, DatatypeProperety, ObjectProperty }

    public class ROntology
    {
        // Массив определений
        public RRecord[] rontology = null;
        // Словарь онтологических объектов имя -> номер в массивах
        public Dictionary<string, int> dicOnto = null;

        /// <summary>
        /// Массив словарей свойств для записей. Элементы массива позиционно соответствуют массиву утверждений.
        /// Элемент массива - словарь, отображений имен свойств в номера позиции в массиве онтологии.
        /// </summary>
        public Dictionary<string, int>[] dicsProps = null;

        // Словарь родителей с именами родителей.
        public static Dictionary<string, string[]> parentsDictionary = null;

        public static RRecord[] LoadROntology(string path)
        {
            System.Xml.Linq.XElement ontology = System.Xml.Linq.XElement.Load(path);
            List<RRecord> resultList = new List<RRecord>();
            string rdf = "{http://www.w3.org/1999/02/22-rdf-syntax-ns#}";
            parentsDictionary = new Dictionary<string, string[]>();


            foreach (var el in ontology.Elements())
            {
                RRecord rec = new RRecord();

                rec.Tp = el.Name.LocalName;
                rec.Id = el.Attribute("{http://www.w3.org/1999/02/22-rdf-syntax-ns#}about").Value.Remove(0, 19);

                var subcl = el.Element("SubClassOf")?.Attribute(rdf + "resource")?.Value;
                var myClasses = getSubClasses(el, ontology);
                parentsDictionary.Add(rec.Id, myClasses);

                var propsList = new List<RProperty>(el.Elements("label").Select(l => new RField() { Prop = "Label", Value = l.Value }));
                propsList.Add(new RField() { Prop = "priority", Value = el.Attribute("priority")?.Value });
                //var objectProps = ontology.Elements("ObjectProperty").Where(x => myClasses.Contains(x.Element("domain").Attribute(rdf + "resource").Value.Remove(0, 19)));
                //var dataTypeProps = ontology.Elements("DatatypeProperty").Where(x => myClasses.Contains(x.Element("domain").Attribute(rdf + "resource").Value.Remove(0, 19)));
                var sortedProps = ontology.Elements()
                    .Where(x => (x.Name.LocalName == "ObjectProperty" || x.Name.LocalName == "DatatypeProperty")
                        && myClasses.Contains(x.Element("domain").Attribute(rdf + "resource").Value.Remove(0, 19)))
                    .OrderBy(prop => prop.Attribute("priority")?.Value);

                //propsList.AddRange(dataTypeProps.Select(p => new RLink { Prop = "DatatypeProperty", Resource = p.Attribute(rdf + "about").Value.Remove(0, 19) }));
                //propsList.AddRange(objectProps.Select(p => new RLink { Prop = "ObjectProperty", Resource = p.Attribute(rdf + "about").Value.Remove(0, 19) }));
                propsList.AddRange(sortedProps.Select(p => new RLink { Prop = p.Name.LocalName, Resource = p.Attribute(rdf + "about").Value.Remove(0, 19) }));

                if (el.Name.LocalName == "DatatypeProperty" || el.Name.LocalName == "ObjectProperty")
                {
                    propsList.AddRange(el.Elements("domain").Select(x => new RLink { Prop = "domain", Resource = x.Attribute(rdf + "resource").Value.Remove(0, 19) }));
                }
                if (el.Name.LocalName == "ObjectProperty")
                {
                    propsList.AddRange(el.Elements("range").Select(x => new RLink { Prop = "range", Resource = x.Attribute(rdf + "resource").Value.Remove(0, 19) }));
                }


                rec.Props = propsList.ToArray();

                resultList.Add(rec);

            }
            return resultList.ToArray();
            //    if (el.HasElements)
            //    {

            //        foreach (var subel in el.Elements())
            //        {

            //            if (subel.Attribute("{http://www.w3.org/XML/1998/namespace}lang") == null || subel.Attribute("{http://www.w3.org/XML/1998/namespace}lang").Value == "ru")
            //            {
            //                switch (subel.Name.LocalName)
            //                {
            //                    case "range":
            //                    case "domain":
            //                        RLink link = new RLink();
            //                        link.Prop = subel.Name.LocalName;
            //                        link.Resource = subel.Attribute("{http://www.w3.org/1999/02/22-rdf-syntax-ns#}resource").Value;
            //                        propsList.Add(link);
            //                        break;
            //                    case "label":
            //                    case "inverse-label":
            //                    default:
            //                        RField field = new RField();
            //                        field.Prop = subel.Name.LocalName;
            //                        field.Value = subel.Value;
            //                        propsList.Add(field);
            //                        break;
            //                }
            //            }

            //        }
            //        var objectProps = ontology.Elements("ObjectProperty").Where(x => x.Element("domain").Attribute("{http://www.w3.org/1999/02/22-rdf-syntax-ns#}resource").Value == rec.Id);
            //        var dataTypeProps = ontology.Elements("DatatypeProperty").Where(x => x.Element("domain").Attribute("{http://www.w3.org/1999/02/22-rdf-syntax-ns#}resource").Value == rec.Id);
            //        foreach(var prop in objectProps)
            //        {
            //            if (prop.Attribute("{http://www.w3.org/XML/1998/namespace}lang") == null || prop.Attribute("{http://www.w3.org/XML/1998/namespace}lang").Value == "ru")
            //            {
            //                RLink link = new RLink();
            //                link.Prop = prop.Name.LocalName;
            //                link.Resource = prop.Element("range").Attribute("{http://www.w3.org/1999/02/22-rdf-syntax-ns#}resource").Value;
            //                propsList.Add(link);
            //            }
            //        }
            //        foreach (var prop in dataTypeProps)
            //        {
            //            if (prop.Attribute("{xml}lang") == null || prop.Attribute("{xml}lang").Value == "ru")
            //            {
            //                if (prop.Element("range").Attribute("{http://www.w3.org/1999/02/22-rdf-syntax-ns#}resource").Value == "http://fogid.net/o/text")
            //                {
            //                    RField field = new RField();
            //                    field.Prop = prop.Name.LocalName;
            //                    field.Value = prop.Value;
            //                    propsList.Add(field);
            //                }
            //                else
            //                {
            //                    RLink link = new RLink();
            //                    link.Prop = prop.Name.LocalName;
            //                    link.Resource = prop.Element("range").Attribute("{http://www.w3.org/1999/02/22-rdf-syntax-ns#}resource").Value;
            //                    propsList.Add(link);
            //                }
            //            }
            //        }
            //        rec.Props = propsList.Distinct().ToArray();
            //    }

            //    resultList.Add(rec);
            //}
            //loadedsampleront = resultList.ToArray();
        }

        private static string[] getSubClasses(System.Xml.Linq.XElement el, System.Xml.Linq.XElement ontology, string[] tempArr)
        {
            var recId = el.Attribute("{http://www.w3.org/1999/02/22-rdf-syntax-ns#}about").Value;
            string rdf = "{http://www.w3.org/1999/02/22-rdf-syntax-ns#}";
            tempArr = tempArr.Append(recId.Remove(0, 19)).ToArray();
            if (el.Element("SubClassOf") == null)
            {
                return tempArr;
            }
            else
            {
                return getSubClasses(
                    ontology.Elements().FirstOrDefault(x =>
                    x.Attribute("{http://www.w3.org/1999/02/22-rdf-syntax-ns#}about").Value == el.Element("SubClassOf").Attribute(rdf + "resource").Value),
                    ontology, tempArr);
            }
        }
        private static string[] getSubClasses(System.Xml.Linq.XElement el, System.Xml.Linq.XElement ontology)
        {
            return getSubClasses(el, ontology, new string[] { });
        }
        public ROntology(IEnumerable<RRecord> statements)
        {
            rontology = statements.ToArray();
            dicOnto = rontology
               .Select((rr, nom) => new { V = rr.Id.Replace("http://fogid.net/o/", ""), nom })
               .ToDictionary(pair => pair.V, pair => pair.nom);
            dicsProps = new Dictionary<string, int>[rontology.Length];
            for (int i = 0; i < rontology.Length; i++)
            {
                if (rontology[i].Props != null)
                {
                    RLink[] links = rontology[i].Props
                        .Where(p => (p.Prop == "DatatypeProperty" || p.Prop == "ObjectProperty"))
                        .Cast<RLink>().ToArray();
                    dicsProps[i] = links
                        .Select((p, n) => new { V = p.Resource.Replace("http://fogid.net/o/", ""), n })
                        .ToDictionary(pair => pair.V, pair => pair.n);
                }
            }
        }
        public ROntology(string path):this(LoadROntology(path)){        }
        // Использование константно заданной онтологии sampleontology
        public ROntology() : this(samplerontology) { }

        /// <summary>
        /// Формирует из записи набор "столбцов" в виде вариантов RProperty, опираясь на данную онтологию.
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public RProperty[] ReorderFieldsDirects(RRecord record)
        {
            // Определяем тип, по нему номер спецификации, по нему спецификацию из rontology. Назовем ее columns
            string tp = record.Tp;
            int nom = dicOnto[tp];
            var columns = rontology[nom];
            Dictionary<string, int> dicProps = dicsProps[nom];

            // Определяем количество полей, строим результирующий массив
            RProperty[] res_arr = new RProperty[dicProps.Count()];

            // Проходимся по колонкам, заполняем элементы res_arr пустыми значениями 
            // TODO: можно эти массивы вычислить заранее, но стоит ли? Все равно для работы потебутеся копия
            foreach (var col in columns.Props) 
            {
                if (col is RLink)
                {
                    RLink rl = (RLink)col;
                    int n = dicProps[rl.Resource];
                    if (rl.Prop == "DatatypeProperty") res_arr[n] = new RField { Prop = rl.Resource };
                    else if (rl.Prop == "ObjectProperty") res_arr[n] = new RDirect { Prop = rl.Resource };
                    else throw new Exception("Err: 931891");
                }
            }

            // Пройдемся по свойствам обрабатываемой записи rrecord, значения скопируем в выходной массив на соответствующей позиции
            foreach (var p in record.Props)
            {
                if (p is RInverse) continue;
                if (dicProps.ContainsKey(p.Prop))
                {
                    int n = dicProps[p.Prop];
                    if (p is RField)
                    {
                        RField f = (RField)p;
                        ((RField)res_arr[n]).Value = f.Value;
                    }
                    else if (p is RDirect)
                    {
                        RDirect d = (RDirect)p;
                        ((RDirect)res_arr[n]).DRec = d.DRec;
                    }
                }
                else
                {

                }
                
            }

            return res_arr;
        }

        public IEnumerable<string> RangesOfProp(string prop)
        {
            int nom = dicOnto[prop];
            return rontology[nom].Props
                .Where(p => p is RLink)
                .Cast<RLink>()
                .Where(rl => rl.Prop == "range")
                .Select(rl => rl.Resource);
        }

        /// <summary>
        /// Онтология состоит из (пронумерованных) утверждений формата RRecord в которых Id - имя понятия,
        /// Tp - вид понятия (RClass, DatatypeProperty, ObjectProperty) и есть набор свойств. Свойства RField
        /// используются со свойствами Label и InverseLabel. Свойства RLink определяют исходящие "стрелки" - их вид 
        /// и имя.
        /// </summary>
        public static RRecord[] samplerontology = new RRecord[]
        {
            new RRecord
            {
                Id = "person",
                Tp = "Class",
                Props = new RProperty[]
                {
                    new RField { Prop = "Label", Value = "Персона" },
                    new RLink { Prop = "DatatypeProperty", Resource = "name"},
                    new RLink { Prop = "DatatypeProperty", Resource = "age"}, // test
                    new RLink { Prop = "DatatypeProperty", Resource = "from-date"},
                    new RLink { Prop = "ObjectProperty", Resource = "father"}

                    //, new RInverseLink { Prop = "ObjectProperty", Source = "reflected" }
                }
            }
            , new RRecord
            {
                Id = "name",
                Tp = "DatatypeProperty",
                Props = new RProperty[] { new RField { Prop = "Label", Value = "имя" }, }
            }
            , new RRecord
            {
                Id = "age",
                Tp = "DatatypeProperty",
                Props = new RProperty[] { new RField { Prop = "Label", Value = "возраст" }, }
            }
            , new RRecord
            {
                Id = "role",
                Tp = "DatatypeProperty",
                Props = new RProperty[] { new RField { Prop = "Label", Value = "роль" }, }
            }
            , new RRecord
            {
                Id = "org-sys",
                Tp = "Class",
                Props = new RProperty[] {
                    new RField { Prop = "Label", Value = "Орг.система" },
                    new RLink { Prop = "DatatypeProperty", Resource = "name"},
                    new RLink { Prop = "DatatypeProperty", Resource = "from-date"}
                }
            }
            , new RRecord
            {
                Id = "from-date",
                Tp = "DatatypeProperty",
                Props = new RProperty[] { new RField { Prop = "Label", Value = "нач.дата" }, }
            }
            , new RRecord
            {
                Id = "participation",
                Tp = "Class",
                Props = new RProperty[]
                {
                    new RField { Prop = "Label", Value = "Участие" },
                    new RLink { Prop = "DatatypeProperty", Resource = "role"},
                    new RLink { Prop = "DatatypeProperty", Resource = "from-date"},
                    new RLink { Prop = "ObjectProperty", Resource = "in-org"},
                    new RLink { Prop = "ObjectProperty", Resource = "participant"},
                }
            }
            , new RRecord
            {
                Id = "in-org",
                Tp = "ObjectProperty",
                Props = new RProperty[]
                {
                    new RField { Prop = "Label", Value = "орг. сист." },
                    new RField { Prop = "InvLabel", Value = "в орг. сист." },
                    new RLink { Prop = "domain", Resource = "participation"},
                    new RLink { Prop = "range", Resource = "org-sys" }
                }
            }
            , new RRecord
            {
                Id = "participant",
                Tp = "ObjectProperty",
                Props = new RProperty[]
                {
                    new RField { Prop = "Label", Value = "участник" },
                    new RField { Prop = "InvLabel", Value = "участник в орг." },
                    new RLink { Prop = "domain", Resource = "participation"},
                    new RLink { Prop = "range", Resource = "person" }
                }
            }
            , new RRecord
            {
                Id = "father",
                Tp = "ObjectProperty",
                Props = new RProperty[]
                {
                    new RField { Prop = "Label", Value = "отец" },
                    new RField { Prop = "InvLabel", Value = "ребенок" },
                    new RLink { Prop = "domain", Resource = "person"},
                    new RLink { Prop = "range", Resource = "person" }
                }
            }

            , new RRecord
            {
                Id = "reflection",
                Tp = "Class",
                Props = new RProperty[]
                {
                    new RField { Prop = "Label", Value = "Отражение" },
                    //new RLink { Prop = "DatatypeProperty", Resource = "from-date"},
                    new RLink { Prop = "ObjectProperty", Resource = "indoc"},
                    new RLink { Prop = "ObjectProperty", Resource = "reflected"},
                }
            }
            , new RRecord
            {
                Id = "indoc",
                Tp = "ObjectProperty",
                Props = new RProperty[]
                {
                    new RField { Prop = "Label", Value = "в док." },
                    new RField { Prop = "InvLabel", Value = "док. для" },
                }
            }
            , new RRecord
            {
                Id = "reflected",
                Tp = "ObjectProperty",
                Props = new RProperty[]
                {
                    new RField { Prop = "Label", Value = "отражен" },
                    new RField { Prop = "InvLabel", Value = "отражамое" },
                }
            }

        };


    }
}
