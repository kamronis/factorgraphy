using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;


namespace RDFEngine
{
    public interface IEngine
    {
        public void Clear();
        public void Load(IEnumerable<XElement> records);
        public void Build();
        public IEnumerable<XElement> Search(string searchstring);
        public XElement GetRecord(string id, bool addinverse);
    }
}
