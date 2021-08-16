using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace RDFEngine
{
    // Движок, основанный на объектном R-представлении
    public class REngine : IEngine
    {
        // База данных будет:
        private IDictionary<string, RRecord> rdatabase;
        public void Load(IEnumerable<XElement> records)
        {
            // ДОБАВЛЕНИЕ обратных ссылок
            Dictionary<string, List<RInverseLink>> inverseDic = new Dictionary<string, List<RInverseLink>>();
            Action<string, RInverseLink> AddInverse = (id, ilink) =>
            {
                if (!inverseDic.ContainsKey(id)) inverseDic.Add(id, new List<RInverseLink>());
                inverseDic[id].Add(ilink);
            };

            rdatabase = records.Select(x =>
            {
                string nodeId = x.Attribute(IEngine.rdfabout).Value;
                return new RRecord()
                {
                    Id = nodeId,
                    Tp = x.Name.NamespaceName + x.Name.LocalName,
                    Props = x.Elements().Select<XElement, RProperty>(el =>
                    {
                        string prop = el.Name.NamespaceName + el.Name.LocalName;
                        XAttribute resource = el.Attribute(IEngine.rdfresource);
                        if (resource == null)
                        {  // Поле   TODO: надо учесть языковый спецификатор, а может, и тип
                            return new RField() { Prop = prop, Value = el.Value };
                        }
                        else
                        {  // ссылка
                            // Создадим RInverse и добавим в inverseDic 
                            AddInverse(resource.Value, new RInverseLink() { Prop = prop, Source = nodeId }); // ДОБАВЛЕНИЕ
                            return new RLink() { Prop = prop, Resource = resource.Value };
                        }
                    }).ToArray()
                };
            }).ToDictionary(rr => rr.Id);

            // ДОБАВЛЕНИЕ Вставим наработанные обратные ссылки
            foreach (var pair in inverseDic)
            {
                string id = pair.Key;
                var list = pair.Value;
                if (!rdatabase.ContainsKey(id)) continue;
                var node = rdatabase[id];
                node.Props = node.Props.Concat(list).ToArray();
            }
        }

        public void Build()
        {
            // Ничего делать не надо, все сделано при загрузке
        }

        public void Clear()
        {
            rdatabase.Clear();
        }

        public RRecord GetRRecord(string id)
        {
            RRecord rr;
            if (rdatabase.TryGetValue(id, out rr))
            {
                return rr;
            }
            return null;
        }

        public IEnumerable<RRecord> RSearch(string searchstring)
        {
            searchstring = searchstring.ToLower();
            return rdatabase
                .Select(pair => pair.Value)
                .Where(rr => 
                {
                    return rr.Props.Any(p => p is RField && ((RField)p).Prop == "name" && ((RField)p).Value.ToLower().StartsWith(searchstring)); 
                });
        }

        /// <summary>
        /// Создает запись в виде дерева, состоящее из полей (level=0+), RDirect (level=1+), RInverse (level=2+).
        /// </summary>
        /// <param name="id"></param>
        /// <param name="level"></param>
        /// <param name="forbidden"></param>
        /// <returns></returns>
        public RRecord GetRTree(string id, int level, string forbidden)
        {
            RRecord node = GetRRecord(id);
            if (node == null) return null;
            RRecord result = new RRecord() { Id = node.Id, Tp = node.Tp, Props = node.Props.Select(p => 
            {
                if (p is RField) { return p; }
                else if (p is RLink) 
                {
                    if (level < 1 || p.Prop == forbidden) return null;
                    RRecord nd = GetRTree(((RLink)p).Resource, level - 1, null);
                    if (nd == null) return null;
                    return new RDirect() { Prop = p.Prop, DRec = nd };
                }
                else if (p is RInverseLink) 
                {
                    if (level < 2) return null;
                    RRecord nd = GetRTree(((RInverseLink)p).Source, level - 1, p.Prop);
                    return new RInverse() { Prop = p.Prop, IRec = nd };
                }
                return null;
            }).Where(r => r != null).ToArray() };
            return result;
        }
    }
}
