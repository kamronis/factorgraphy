using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RDFEngine
{
    /// <summary>
    /// 
    /// Это пока в разработке (идеи)!!!
    /// 
    /// Онтологическое определение имеет формат RRecord. При этом, идентификатор записи Id задает имя онтологического объекта.
    /// Тип записи может быть Class, DatatypeProperty, ObjectProperty и что-то для определения перечислимого.
    /// Class соответствует определению класса, свойства, соотвествуют определению соответствующих свойств (стрелок).
    /// В списке Props могут быть свойства видов: RField - определяет некоторый атрибут для онтологического элемента, RLink - определяет 
    /// ссылку на другой онтологический объект.
    /// Атрибуты: 
    /// имя атрибута Label, значение атрибута строка "человеческого" названия онтологического объекта
    /// имя атрибута InvLabel, значение атрибута строка "человеческого" названия онтологического объекта - обратной стрелки 
    /// имя атрибута ... - приоритет
    /// Ссылки:
    /// имя свойства DatatypeProperty, значение свойства - имя онтологического объекта (стрелки предиката данных)
    /// имя свойства ObjectProperty, значение свойства - имя онтологичествого объекта (стрелки предиката ссылки)
    /// 
    /// Что можно делать с помощью онтологии в формате ROntology?
    /// 
    /// 1. Локализовывать онтологическое описание по номеру в rontology и по словарю dicOnto
    /// 2. Получать названия онтологических объектов через Label и InvLabel
    /// 3. Для заданного типа, множество (массив) определений исходящих стрелок RRecord[] PossibleDirects(string tp)
    /// 4. Для заданного типа, множество (массив) определений входящих стрелок RRecord[] PossibleInverse(string tp)
    /// 5. Дерево классов, возможно и дерево свойств
    /// 
    /// 
    /// </summary>

    // Перечисление может быть более эффективным, чем строки
    // public enum RVid { RClass, DatatypeProperety, ObjectProperty }

    // Класс для построения дерева онтологических классов
    class RTreeNode
    {
        public string Id { get; set; }
        public RTreeNode Parent { get; set; }
        public List<RTreeNode> Childs { get; set; }
    }

    public class ROntology
    {
        // ============================= "Классная кухня" - Конечное построение - предки и потомки ==============
        internal static XElement xontology; // Сюда загрузчик поместит онтологию 
        // словарь узлов классов
        Dictionary<string, RTreeNode> RTNdic; 
        // Создание словаря из XML-онтологии. Загружается когда есть xontology
        public void BuldRTree()
        {
            string rdf = "{http://www.w3.org/1999/02/22-rdf-syntax-ns#}";
            // Создадим узлы, поместим их в дерево
            RTNdic = xontology.Elements("Class").Select(x => new RTreeNode
            {
                Id = x.Attribute("{http://www.w3.org/1999/02/22-rdf-syntax-ns#}about").Value,
                Childs = new List<RTreeNode>()
            }).ToDictionary(t => t.Id);
            // Снова сканируем элементы, заполняем родителя и детей
            foreach (XElement x in xontology.Elements("Class"))
            {
                string parentId = x.Element("SubClassOf")?.Attribute("{http://www.w3.org/1999/02/22-rdf-syntax-ns#}resource")?.Value;
                if (parentId == null) continue;
                RTreeNode node = RTNdic[x.Attribute("{http://www.w3.org/1999/02/22-rdf-syntax-ns#}about").Value];
                RTreeNode parentNode = RTNdic[parentId];
                node.Parent = parentNode;
                parentNode.Childs.Add(node);
            }
        }

        private IEnumerable<RTreeNode> AAS(RTreeNode node)
        {
            if (node.Parent == null) return new RTreeNode[] { node };
            var res = AAS(node.Parent).Append(node);
            return res;
        }
        // 
        public IEnumerable<string> AncestorsAndSelf(string id)
        {
            RTreeNode node = RTNdic[id];
            var res = AAS(node).Select(n => n.Id);
            return res;
        }
        private IEnumerable<RTreeNode> DAS(RTreeNode node)
        {
            return (new RTreeNode[] { node }).Concat(node.Childs.SelectMany(c => DAS(c)));
        }
        public IEnumerable<string> DescendantsAndSeld(string id)
        {
            RTreeNode node = RTNdic[id];
            return DAS(node).Select(n => n.Id);
        }
        // ======================== конец "кухни" =========================


        // Массив определений
        public RRecord[] rontology = null;
        // Словарь онтологических объектов имя -> номер в массивах
        public Dictionary<string, int> dicOnto = null;

        /// <summary>
        /// Массив словарей свойств для записей. Элементы массива позиционно соответствуют массиву утверждений.
        /// Элемент массива - словарь, отображений имен свойств в номера позиции в массиве онтологии.
        /// </summary>
        public Dictionary<string, int>[] dicsProps = null;
        /// <summary>
        /// 
        /// </summary>
        private Dictionary<string, string[]> dicsInversePropsForType = null;
        public IEnumerable<string> GetInversePropsByType(string tp) 
        {
            return dicsInversePropsForType[tp];
                //.Select(ps => )
        }

        // Словарь родителей с именами родителей.
        public static Dictionary<string, string[]> parentsDictionary = null;

        public static RRecord[] LoadROntology(string path)
        {
            string rdf = "{http://www.w3.org/1999/02/22-rdf-syntax-ns#}";
            Func<XElement, string> ename = x => x.Name.NamespaceName + x.Name.LocalName;

            List<RRecord> resultList = new List<RRecord>();
            parentsDictionary = new Dictionary<string, string[]>();


            foreach (var el in xontology.Elements())
            {
                // Входными элементами являются: Class, DatatypeProperty, ObjectProperty, EnumerationType
                
                // Во всех случаях, в выходной поток направляется RRecord, причем тип записи совпадает с именем элемента,
                // идентификатор - берется из rdf:about
                RRecord rec = new RRecord();
                rec.Tp = ename(el);
                rec.Id = el.Attribute("{http://www.w3.org/1999/02/22-rdf-syntax-ns#}about").Value;

                // 

                var subcl = el.Element("SubClassOf")?.Attribute(rdf + "resource")?.Value;
                var myClasses = getSubClasses(el, xontology);
                parentsDictionary.Add(rec.Id, myClasses);

                List<RProperty> propsList = new List<RProperty>();
                // el.Elements("label").Select(l => new RField() { Prop = "Label", Value = l.Value })
                var lls = el.Elements("label").ToArray();
                string label = el.Elements("label").FirstOrDefault(e => e.Attribute("{http://www.w3.org/XML/1998/namespace}lang").Value == "ru")?.Value;
                if (label != null) propsList.Add(new RField() { Prop = "Label", Value = label });
                string invlabel = el.Elements("inverse-label").FirstOrDefault(e => e.Attribute("{http://www.w3.org/XML/1998/namespace}lang").Value == "ru")?.Value;
                if (invlabel != null) propsList.Add(new RField() { Prop = "InvLabel", Value = label });
                propsList.Add(new RField() { Prop = "priority", Value = el.Attribute("priority")?.Value });

                var sortedProps = xontology.Elements()
                    .Where(x => (x.Name == "ObjectProperty" || x.Name == "DatatypeProperty")
                        && myClasses.Contains(x.Element("domain").Attribute(rdf + "resource").Value))
                    .OrderBy(prop => prop.Attribute("priority")?.Value);

                propsList.AddRange(sortedProps.Select(p => new RLink { Prop = ename(p), Resource = p.Attribute(rdf + "about").Value }));

                if (el.Name == "DatatypeProperty" || el.Name == "ObjectProperty")
                {
                    propsList.AddRange(el.Elements("domain").Select(x => new RLink { Prop = "domain", Resource = x.Attribute(rdf + "resource").Value }));
                }
                if (el.Name.LocalName == "ObjectProperty")
                {
                    propsList.AddRange(el.Elements("range").Select(x => new RLink { Prop = "range", Resource = x.Attribute(rdf + "resource").Value }));
                }


                rec.Props = propsList.ToArray();

                resultList.Add(rec);

            }
            var arr = resultList.ToArray();
            return arr;
        }

        private static string[] getSubClasses(XElement el, XElement ontology, string[] tempArr)
        {
            var recId = el.Attribute("{http://www.w3.org/1999/02/22-rdf-syntax-ns#}about").Value;
            string rdf = "{http://www.w3.org/1999/02/22-rdf-syntax-ns#}";
            tempArr = tempArr.Append(recId).ToArray();
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
        private static string[] getSubClasses(XElement el, XElement ontology)
        {
            return getSubClasses(el, ontology, new string[] { });
        }
        //public ROntology(IEnumerable<RRecord> statements)
        //{
        //    // Это массив оннтологических описаний
        //    rontology = statements.ToArray();

        //    // Это словарь онтологических описани: по идентификатору онто объекта дается номер в таблице описаний
        //    dicOnto = rontology
        //       .Select((rr, nom) => new { V = rr.Id, nom })
        //       .ToDictionary(pair => pair.V, pair => pair.nom);


        //    dicsProps = new Dictionary<string, int>[rontology.Length];
        //    for (int i = 0; i < rontology.Length; i++)
        //    {
        //        if (rontology[i].Props != null)
        //        {
        //            RLink[] links = rontology[i].Props
        //                .Where(p => (p.Prop == "DatatypeProperty" || p.Prop == "ObjectProperty"))
        //                .Cast<RLink>().ToArray();
        //            dicsProps[i] = links
        //                .Select((p, n) => new { V = p.Resource, n })
        //                .ToDictionary(pair => pair.V, pair => pair.n);
        //        }
        //    }
        //    // Вычисляем обратные свойства для типов
        //    dicsInversePropsForType = rontology.Where(rr => rr.Tp == "ObjectProperty")
        //        .SelectMany(rr => rr.Props
        //            .Where(p => p is RLink && p.Prop == "range")
        //            .Select(p => new { pr = rr.Id, tp = ((RLink)p).Resource }))
        //        .GroupBy(pair => pair.tp)
        //        .ToDictionary(keypair => keypair.Key, keypair => keypair.Select(x => x.pr).ToArray());
        //    var qu = rontology.Where(rr => rr.Tp == "ObjectProperty")
        //        .SelectMany(rr => rr.Props
        //            .Where(p => p is RLink && p.Prop == "range")
        //            .Select(p => new { pr = rr.Id, tp = ((RLink)p).Resource }))
        //        .ToArray();
        //}
        public ROntology(string path)
        {
            // Действие для "Классной кухни"
            xontology = XElement.Load(path);
            this.BuldRTree();
            rontology = LoadROntology(path);
            // Это словарь онтологических описани: по идентификатору онто объекта дается номер в таблице описаний
            dicOnto = rontology
               .Select((rr, nom) => new { V = rr.Id, nom })
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
                        .Select((p, n) => new { V = p.Resource, n })
                        .ToDictionary(pair => pair.V, pair => pair.n);
                }
            }
            // Вычисляем обратные свойства для типов
            //dicsInversePropsForType = rontology.Where(rr => rr.Tp == "ObjectProperty")
            //    .SelectMany(rr => rr.Props
            //        .Where(p => p is RLink && p.Prop == "range")
            //        .Select(p => new { pr = rr.Id, tp = ((RLink)p).Resource }))
            //    .GroupBy(pair => pair.tp)
            //    .ToDictionary(keypair => keypair.Key, keypair => keypair.Select(x => x.pr).ToArray());
            dicsInversePropsForType = //null;
                rontology.Where(rr => rr.Tp == "ObjectProperty")
                .SelectMany(rr => rr.Props
                    .Where(p => p is RLink && p.Prop == "range")
                    .Select(p => new { pr = rr.Id, tp = ((RLink)p).Resource }))
                .SelectMany(pa => DescendantsAndSeld(pa.tp).Select(t => new { ty = t, pr_id = pa.pr }))
                .GroupBy(typr => typr.ty)
                .ToDictionary(keypair => keypair.Key, keypair => keypair.Select(x => x.pr_id).Distinct().ToArray());

        }
        // Использование константно заданной онтологии sampleontology
        //public ROntology() : this(samplerontology) { }

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
        public IEnumerable<string> DomainsOfProp(string prop)
        {
            int nom = dicOnto[prop];
            return rontology[nom].Props
                .Where(p => p is RLink)
                .Cast<RLink>()
                .Where(rl => rl.Prop == "domain")
                .Select(rl => rl.Resource);
        }
        public string LabelOfOnto(string id)
        {
            int nom = dicOnto[id];
            return rontology[nom].Props
                .Where(p => p is RField)
                .Cast<RField>()
                .FirstOrDefault(rl => rl.Prop == "Label")?.Value;
        }
        public string InvLabelOfOnto(string id)
        {
            int nom = dicOnto[id];
            return rontology[nom].Props
                .Where(p => p is RField)
                .Cast<RField>()
                .FirstOrDefault(rl => rl.Prop == "InvLabel")?.Value;
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
