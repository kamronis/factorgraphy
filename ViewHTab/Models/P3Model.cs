﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RDFEngine;

namespace ViewHTab.Models
{
    /// Вспомогательный класс - группировка списков обратных свойств
    public class InversePropType
    {
        public string Prop;
        public InverseType[] lists;
    }
    /// Вспомогательный класс - группировка списков по типам ссылающихся записей
    public class InverseType
    {
        public string Tp;
        public RProperty[] list;
    }
    /// Класс модели
    public class P3Model
    {
        public string Id, Tp;
        public RProperty[] row;
        public InversePropType[] inv;

        /// "Неправильное" преобразование расширенной записи в модель
        public P3Model Build0(RRecord erec)
        {
            var query = erec.Props.Where(p => p is RInverse)
                .Cast<RInverse>()
                .GroupBy(d => d.Prop)
                .Select(kd => new InversePropType { Prop = kd.Key,
                    lists =
                    kd.GroupBy(d => d.IRec.Tp)
                        .Select(dd => {
                            var qu = dd.Select(x => x).ToArray();
                            return new InverseType
                            {
                                Tp = dd.Key,
                                list = qu
                            };
                        }).ToArray()
                });
            return new P3Model
            {
                Id = erec.Id, Tp = erec.Tp,
                row = erec.Props.Where(p => p is RField || p is RDirect).ToArray(),
                inv = query.ToArray()
            };
        }

    }

}
