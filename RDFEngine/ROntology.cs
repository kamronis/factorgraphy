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
        public Dictionary<string, int[]>[] dicsInverseProps = null;

        public ROntology(IEnumerable<RRecord> statements)
        {
            rontology = statements.ToArray();
            dicOnto = rontology
               .Select((rr, nom) => new { rr.Id, nom })
               .ToDictionary(pair => pair.Id, pair => pair.nom);
            dicsProps = new Dictionary<string, int>[rontology.Length];
            for (int i = 0; i < rontology.Length; i++)
            {
                RLink[] links = rontology[i].Props
                    .Where(p => p.Prop == "DatatypeProperty" || p.Prop == "ObjectProperty")
                    .Cast<RLink>().ToArray();
                dicsProps[i] = links
                    .Select((p, n) => new { p.Resource, n })
                    .ToDictionary(pair => pair.Resource, pair => pair.n);
            }
            dicsInverseProps = new Dictionary<string, int[]>[rontology.Length];
            for (int i = 0; i< rontology.Length; i++)
            {

            }
        }
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
                if (!dicProps.ContainsKey(p.Prop)) continue;
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
                Tp = "RClass",
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
