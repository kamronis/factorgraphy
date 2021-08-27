using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RDFEngine;
namespace FactographyView.Models
{
    /// Вспомогательный класс - группировка списков обратных свойств
    /// 
    public class InverseProps
    {
        public string Prop;
        public InverseTypes[] lists; 
    }
    public class InverseTypes
    {
        public string Tp;
        public RProperty[] list;
    }

    /// <summary>
    /// Класс состоит из элементов и структур, требуемых для отрисовки портрета
    /// Логическая структура отрисовки: тип, идентификатор, name, uri
    /// Потом - массив полей и прямых свойств
    /// Потом - массив групп обратных свойств. Каждая группа определяется именем предиката ОС и типом записи ОС
    /// </summary>
    public class P3Model
    {
        public string Id, Tp, name, uri;
        public RProperty[] row;
        public InverseProps[] inv;
        public P3Model(RRecord erec)
        {
            this.Id = erec.Id;
            this.Tp = erec.Tp;
            List<RDFEngine.RProperty> rowlist = new List<RDFEngine.RProperty>();
            List<RInverse> inverselist = new List<RInverse>();
            foreach (var p in erec.Props)
            {
                if (p is RField)
                {
                    RField f = (RField)p;
                    if (f.Prop == "name") name = f.Value;
                    else if (f.Prop == "uri") uri = f.Value;
                    else
                    {
                        rowlist.Add(new RField() { Prop = f.Prop, Value = f.Value });
                    }
                }
                else if (p is RDirect)
                {
                    RDirect d = (RDirect)p;
                    rowlist.Add(new RDirect() { Prop = d.Prop, DRec = d.DRec });
                }
                else if (p is RInverse)
                {
                    RInverse ri = (RInverse)p;
                    inverselist.Add(new RInverse() { Prop = ri.Prop, IRec = ri.IRec });
                }
            }
            this.row = rowlist.ToArray();
            List<InverseProps> lip = new List<InverseProps>();
            foreach (var pair in inverselist.GroupBy<RInverse, string>(p => p.Prop))
            {
                var k = pair.Key;
                var v = pair.ToArray();//.GroupBy<RInverse, string>(ri => ri.IRec.Tp).ToArray();
                var w = v.GroupBy<RInverse, string>(ris => ris.IRec.Tp)
                    .Select(pa => new InverseTypes() { Tp = pa.Key, list = pa.ToArray() }).ToArray();
                lip.Add(new InverseProps() { Prop = k, lists = w });
            }
            this.inv = lip.ToArray();

        }
    }

}
