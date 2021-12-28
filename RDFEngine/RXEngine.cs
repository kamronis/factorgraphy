using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RDFEngine
{
    public class RXEngine : IEngine
    {
        public void Build()
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<RRecord> RSearch(string searchstring)
        {
            var res = OAData.OADB.SearchByName(searchstring)
                .Select(x => new RRecord
                {
                    Id = x.Attribute("id").Value,
                    Tp = x.Attribute("type").Value,
                    Props = x.Elements()
                        .Select(e =>
                        {
                            if (e.Name == "field") return new RField { Prop = e.Attribute("prop").Value, Value = e.Value };
                            return null;
                        }).ToArray()

                }).ToArray();
            return res;
        }

        public IEnumerable<RRecord> RSearch(string searchstring, string type)
        {
            throw new NotImplementedException();
        }

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

        public bool DeleteRecord(string id)
        {
            throw new NotImplementedException();
        }

        public RRecord GetRRecord(string id)
        {
            var item = OAData.OADB.GetItemByIdBasic(id, true);
            RRecord rec = new RRecord
            {
                Id = item.Attribute("id").Value,
                Tp = item.Attribute("type").Value,
                Props = item.Elements().Select(px => 
                    {
                        if (px.Name == "field")
                        {
                            return new RField { Prop = px.Attribute("prop").Value, Value = px.Value };
                        }
                        else if (px.Name == "direct")
                        {
                            RLink rl = new RLink
                            {
                                Prop = px.Attribute("prop").Value,
                                Resource = px.Element("record").Attribute("id").Value
                            };
                            return rl;
                        }
                        else if (px.Name == "inverse")
                        {
                            RInverseLink ril = new RInverseLink
                            {
                                Prop = px.Attribute("prop").Value,
                                Source = px.Element("record").Attribute("id").Value
                            };
                            return ril;
                        }
                        else
                        {
                            return (RProperty)null;
                        }
                    })
                .Where(p => p != null)
                .ToArray()
            };
            return rec;
        }

        public void Load(IEnumerable<XElement> records)
        {
            throw new NotImplementedException();
        }

        public string NewRecord(string type, string name)
        {
            throw new NotImplementedException();
        }

        public string NewRelation(string type, string inverseprop, string source)
        {
            throw new NotImplementedException();
        }


        public void Update(RRecord record)
        {
            throw new NotImplementedException();
        }
    }
}
