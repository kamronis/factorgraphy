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
        public IEnumerable<RRecord> RSearch(string searchstring);
        public RRecord GetRRecord(string id);
        public RRecord GetRTree(string id, int level, string forbidden);

        // Константы для удобства работы с RDF/XML
        public static XName rdfabout = XName.Get("about", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
        public static XName rdfresource = XName.Get("resource", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");

    }
}
