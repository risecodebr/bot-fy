﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bot_fy.Discord.Extensions
{
    public static class QueueExtensions
    {
        public static Queue<T> Shuffle<T>(this IEnumerable<T> list)
        {
            List<T> lista = new(list);
            Random random = new();

            for (int i = 0; i < lista.Count; i++)
            {
                int j = random.Next(i, lista.Count);
                T temp = lista[i];
                lista[i] = lista[j];
                lista[j] = temp;
            }

            return new Queue<T>(lista);
        }

        public static TimeSpan Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, TimeSpan?> func)
        {
            return new TimeSpan(source.Sum(item => func(item)!.Value!.Ticks));
        }

        public static string ToStringTime(this TimeSpan timeSpan)
        {
            return timeSpan.ToString(timeSpan.TotalHours >= 1 ? @"h\hmm\mss\s" : @"mm\mss\s")!;
        }

        public static string ToStringTime(this TimeSpan? timeSpan)
        {
            return timeSpan!.Value.ToStringTime();
        }
    }
}
