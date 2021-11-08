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
            // TODO: Можно не вставлять, но об этом надо подумать...
            foreach (var pair in inverseDic)
            {
                string id = pair.Key;
                var list = pair.Value;
                if (!rdatabase.ContainsKey(id)) continue;
                var node = rdatabase[id];
                node.Props = node.Props.Concat(list).ToArray();
            }
        }

        /// <summary>
        /// Тестовая загрузка данных
        /// </summary>
        public void Load()
        {
            Load(XElement.Parse(testRDFText).Elements()); ;
        }
        /// <summary>
        /// Тестовая база данных
        /// </summary>
        private const string testRDFText = @"<?xml version='1.0' encoding='utf-8'?>
<rdf:RDF xmlns:rdf='http://www.w3.org/1999/02/22-rdf-syntax-ns#'>
  <person rdf:about='p3817'>
    <name xml:lang='ru'>Иванов</name>
    <from-date>1988</from-date>
  </person>
  <person rdf:about='p3818'>
    <from-date>1999</from-date>
    <name xml:lang='ru'>Петров</name>
  </person>
  <org-sys rdf:about='o19302'>
    <from-date>1959</from-date>
    <name>НГУ</name>
  </org-sys>
  <participation rdf:about='r1111'>
    <participant rdf:resource='p3817' />
    <in-org rdf:resource='o19302' />
    <role>профессор</role>
  </participation>
  <participation rdf:about='r1112'>
    <participant rdf:resource='p3818' />
    <in-org rdf:resource='o19302' />
    <from-date>2008</from-date>
    <role>ассистент</role>
  </participation>
</rdf:RDF>";

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

        public void Update(RRecord rec)
        {
            // Найдем текущее значение записи
            RRecord dbrec = rdatabase[rec.Id];
            // На всякий случай, проверим тип 
            if (rec.Tp != dbrec.Tp) throw new Exception("Err: 223902");
            // Нужно "перебрать" прямые свойства, из полей что-то убрать, что-то добавить, ссылки обработать специально.
            //RProperty[] props = null;
            //var query = dbrec.Props
            //    .Select(p =>
            //    {
            //        if (p is RField)
            //        {
            //            RField f = (RField)p;
            //            return new RField { Prop = f.Prop, Value = f.Value };
            //        }
            //        else // if (p is RLink)
            //        {
            //            RLink l = (RLink)p;
            //            return new RLink { Prop = l.Prop, Resource = l.Resource };
            //        }
            //        //else return null;
            //    })
            //    .ToArray();
            
        }

        // ==== Определения, созданные для Portrait2, Portrait3


        public RRecord BuildPortrait(string id)
        {
            return BuPo(id, 2, null);
        }
        private RRecord BuPo(string id, int level, string forbidden)
        {
            var rec = GetRRecord(id);
            if (rec == null) return null;
            RRecord result_rec = new RRecord()
            {
                Id = rec.Id,
                Tp = rec.Tp,
                Props = rec.Props.Select<RProperty, RProperty>(p =>
                {
                    if (p is RField)
                        return new RField() { Prop = p.Prop, Value = ((RField)p).Value };
                    else if (level > 0 && p is RLink && p.Prop != forbidden)
                        return new RDirect() { Prop = p.Prop, DRec = BuPo(((RLink)p).Resource, 0, null) };
                    else if (level > 1 && p is RInverseLink)
                        return new RInverse() { Prop = p.Prop, IRec = BuPo(((RInverseLink)p).Source, 1, p.Prop) };
                    return null;
                }).Where(p => p != null).ToArray()
            };
            return result_rec;
        }

    }
}
