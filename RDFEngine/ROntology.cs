using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDFEngine
{
    // public enum RVid { RClass, DatatypeProperety, ObjectProperty }
    public class ROntology
    {
        // Массив определений
        internal RRecord[] rontology = null;
        // Словарь онтологических объектов имя -> номер в массивах
        public Dictionary<string, int> dicOnto = null;

        /// <summary>
        /// Массив словарей свойств для записей. Элементы массива позиционно соответствуют массиву утверждений.
        /// Элемент массива - словарь, отображений имен свойств в номера позиции в массиве онтологии.
        /// </summary>
        public Dictionary<string, int>[] dicsProps = null;

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
        }
        // Использование константно заданной онтологии sampleontology
        public ROntology() : this(samplerontology) { }

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
                    new RLink { Prop = "DatatypeProperty", Resource = "from-date"},
                    new RLink { Prop = "ObjectProperty", Resource = "father"}
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
                }
            }

        };

    }
}
