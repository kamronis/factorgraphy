using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDFEngine
{
    public class RRecord
    {
        public string Id { get; set; }
        public string Tp { get; set; }
        public RProperty[] Props { get; set; }
        public override string ToString()
        {
            var query = Props.Select(p =>
            {
                string prop = p.Prop;
                if (p is RField) return "f^{<" + prop + ">, \"" + ((RField)p).Value + "\"}";
                else             return "l^{<" + prop + ">, <" + ((RLink)p).Resource + ">}";
            }).Aggregate((a, s) => a + ", " + s);
            return "{ <" + Id + ">, <" + Tp + ">, " +         query                + "}";
        }
    }
    public abstract class RProperty
    {
        public string Prop { get; set; }
    }
    public class RField : RProperty 
    {
        public string Value { get; set; }
    }
    public class RLink : RProperty
    {
        public string Resource { get; set; }
    }
}
