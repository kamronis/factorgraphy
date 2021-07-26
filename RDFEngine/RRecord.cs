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
                if (p is RField)      return "f^{<" + prop + ">, \"" + ((RField)p).Value + "\"}";
                else if (p is RLink) return "l^{<" + prop + ">, <" + ((RLink)p).Resource + ">}";
                // Добавленный вариант обратной ссылки
                else if (p is RInverseLink) return "il^{<" + prop + ">, <" + ((RInverseLink)p).Source + ">}";
                else if (p is RDirect) return "d^{<" + prop + ">, <" + ((RDirect)p).DRec.ToString() + ">}";
                /*else if (p is RDirect)*/ return "i^{<" + prop + ">, <" + ((RInverse)p).IRec.ToString() + ">}";
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

    // Расширение вводится на странице 11 пособия "Делаем фактографию"
    public class RInverseLink : RProperty
    {
        public string Source { get; set; }
    }

    // Новое расширение
    public class RDirect : RProperty
    {
        public RRecord DRec { get; set; }
    }
    public class RInverse : RProperty
    {
        public RRecord IRec { get; set; }
    }
}
