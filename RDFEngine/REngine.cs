using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace RDFEngine
{
    // Движок, основанный на объектном R-представлении
    /// <summary>
    /// База данных состоит из множества записей RRecord, сформированных как хеш-словарь (Dictionary) В котором ключем является
    /// Id записи. Сами записи состоят из идентификатора Id, имени типа Tp и набора свойств RPreperty[] Props. В базе данных
    /// используется три вида свойств: RField, RLink, RInverseLink. Основные свойства - поля и прямые ссылки. Обратные ссылки
    /// вычисляются по основным, точнее по RLink. Каждое свойство RLink порождает одно и только одно обратное свойство по соотношению
    /// new RRecord { Id=id0 ... Props = new RProperty[] { ... new RLink { Prop = "p1", Resource = id1 } ... } --->
    /// new RREcord { Id=id1 ... Props = new RProperty[] { ... new RInverseLink { Prop = "p1", Source = id0 } ... }
    /// Эти свойства должны воспроизводиться при любой операции добавления, изменения, уничтожения записи. 
    /// </summary>
    public class REngine : IEngine
    {
        // База данных будет:
        private IDictionary<string, RRecord> rdatabase;

        public void Load(IEnumerable<XElement> records)
        {
            // Локальные определения
            Dictionary<string, List<RInverseLink>> inverseDic = new Dictionary<string, List<RInverseLink>>();
            void AddInverse(string id, RInverseLink ilink)
            {
                if (!inverseDic.ContainsKey(id)) inverseDic.Add(id, new List<RInverseLink>());
                inverseDic[id].Add(ilink);
            }

            // Загрузка элементов и добавление в словарь inverseDic (побочный эффект) обратных ссылок
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

            // ДОБАВЛЕНИЕ Вставим в базу данных rdatabase наработанные обратные ссылки
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

        // =============== Вариант Трошкова =================
        public void UpdateRRecord(RRecord record, string forbidden, string modelId, bool delete)
        {
            // Есть два варианта: запись уж существует в базе данных или нет
            if (rdatabase.ContainsKey(record.Id))
            {   // Запись есть
                // теперь таже два варианта действий: уничтожить текущую запись и изменить (это и будет Update)
                if (delete)
                {
                    // Для уничтожения записи сначала в элементах, на которые указывают линки, убираются обратные ссылки
                    foreach (var prop in rdatabase[record.Id].Props)
                    {
                        if (prop is RLink)
                        {
                            rdatabase[((RLink)prop).Resource].Props = rdatabase[((RLink)prop).Resource].Props
                                .Where(x => (x is RField) || (x is RInverseLink && ((RInverseLink)x).Source != record.Id))
                                .ToArray();
                        }
                    }
                    // потом уничтожается сама запись
                    rdatabase.Remove(record.Id);
                }
                else
                {   // Это настоящий Update
                    rdatabase[record.Id] = generateRecordToAdd(record, forbidden, modelId);
                }
            }
            else
            {
                rdatabase.Add(record.Id, generateRecordToAdd(record, forbidden, modelId));
                var modelInverse = new RInverseLink();
                modelInverse.Prop = forbidden;
                modelInverse.Source = record.Id;
                var linkId = ((RDirect)record.Props.FirstOrDefault(x => (x is RDirect) && (x.Prop != forbidden))).DRec.Id;
                var linkInverse = new RInverseLink();
                linkInverse.Prop = record.Props.FirstOrDefault(x => (x is RDirect) && (x.Prop != forbidden)).Prop;
                linkInverse.Source = record.Id;
                rdatabase[modelId].Props = rdatabase[modelId].Props.Append(modelInverse).ToArray();
                rdatabase[linkId].Props = rdatabase[linkId].Props.Append(linkInverse).ToArray();
            }
        }
        public void UpdateRRecord(RRecord record, string forbidden, string modelId)
        {
            UpdateRRecord(record, forbidden, modelId, false);
        }
        private RRecord generateRecordToAdd(RRecord record, string forbidden, string modelId)
        {
            RRecord toAdd = new RRecord();
            toAdd.Id = record.Id;
            toAdd.Tp = record.Tp;
            toAdd.Props = new RProperty[record.Props.Length];
            for (int i = 0; i < record.Props.Length; i++)
            {
                var prop = record.Props[i];
                if (prop is RField)
                {
                    toAdd.Props[i] = prop;
                }
                if (prop is RDirect)
                {
                    if (prop.Prop == forbidden)
                    {
                        RLink link = new RLink();
                        link.Prop = prop.Prop;
                        link.Resource = modelId;
                        toAdd.Props[i] = link;
                    }
                    else
                    {
                        RLink link = new RLink();
                        link.Prop = prop.Prop;
                        link.Resource = ((RDirect)prop).DRec?.Id;
                        toAdd.Props[i] = link;
                    }

                }

            }
            return toAdd;
        }
        // =============== конец ================

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

        public IEnumerable<RRecord> RSearch(string searchstring, string type)
        {
            searchstring = searchstring.ToLower();
            return rdatabase
                .Select(pair => pair.Value)
                .Where(rr => rr.Tp == type)
                .Where(rr =>
                {
                    return rr.Props.Any(p => p is RField && ((RField)p).Prop == "name" && ((RField)p).Value.ToLower().StartsWith(searchstring));
                });
        }

        private RProperty GetProp(RRecord rec, string prop) { return rec.Props.FirstOrDefault(p => p.Prop == prop); }

        /// <summary>
        /// Замена значения записи в базе данных и корректирование списков обратных ссылок
        /// </summary>
        /// <param name="rec"></param>
        public void Update(RRecord rec)
        {
            // Найдем текущее значение записи
            RRecord dbrec = rdatabase[rec.Id];
            // На всякий случай, проверим тип 
            if (rec.Tp != dbrec.Tp) throw new Exception("Err: 223902");

            // Сначала сделаем коррекции обратных ссылок. Для этого, выделяем множество "прямых стрелок",
            // т.е. значений типа RLink, взятых их старой записи dbrec и новой записи rec. Те стрелки, которые входят в пересечение 
            // множеств, корректировать не надо. Корректировать нужно разность. Стрелки, имеющиеся в СТАРОЙ записи, но отсутствующие
            // в новой надо использовать для того, чтобы корректировать записи на предмет УСТРАНЕНИЯ обратных ссылок из таргетных
            // записей. Стрелки, имеющиеся в НОВОЙ записи, но отсутствующие в старой, надо использовать для ДОБАВЛЕНИЯ обратных ссылок
            // в таргетные записи. 

            // Для реализации множеств (стрелок), нам понадобится компаратор, он встроен в определение RLink
            // Старое и новое множество стрелок
            var oSet = dbrec.Props.Where(p => p is RLink).Cast<RLink>().Distinct();
            var nSet = rec.Props.Where(p => p is RLink || (p is RDirect && ((RDirect)p).DRec != null))
                .Select(p => p is RDirect ? new RLink { Prop = p.Prop, Resource = ((RDirect)p).DRec.Id } : p)
                .Cast<RLink>().Distinct();

            // Коррекция стрелок первого множества
            foreach (var arrow in oSet.Except(nSet))
            {
                string target = arrow.Resource;
                // Находим целевую запись
                RRecord trec = rdatabase[target];
                // Устраним в списке свойств обратную ссылку с указанным Prop и идентификатором dbRec.Id
                RProperty[] nprops = trec.Props.Select(p =>
                    (p is RInverseLink)
                    ? ((p.Prop == arrow.Prop && ((RInverseLink)p).Source == dbrec.Id) ? (RProperty)null : p)
                    : p )
                .Where(p => p != null)
                .ToArray();
                trec.Props = nprops;
            }

            // Коллекция стрелок второго множества
            foreach (var arrow in nSet.Except(oSet))
            {
                string target = arrow.Resource;
                // Находим целевую запись
                RRecord trec = rdatabase[target];
                // Добавим к списку свойств обратную ссылку с указанным Prop и идентификатором dbRec.Id
                RProperty[] nprops = trec.Props
                    .Append(new RInverseLink { Prop = arrow.Prop, Source = dbrec.Id })
                    .ToArray();
                trec.Props = nprops;
            }

            // Теперь выполним основное действие - вычислим новое списка свойств записи dbrec.
            // Для этого перепишем обратные ссылки и добавим все остальное, не забыв отфильтровать несущественные значения
            RProperty[] result = rec.Props.Select(p =>
                (p is RDirect) ?
                (((RDirect)p).DRec == null ? (RProperty)null : new RLink { Prop = p.Prop, Resource = ((RDirect)p).DRec.Id }) :
                (string.IsNullOrEmpty(((RField)p).Value) ? (RProperty)null : p) )
                .Concat(
                dbrec.Props
                .Where(p => p is RInverseLink)
                    )
                .Where(p => p != null)
                .ToArray();

            // Поставим на место
            dbrec.Props = result;
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
