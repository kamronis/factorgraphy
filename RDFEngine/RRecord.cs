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
        //public RRecord Target { get; set; }
    }
}
