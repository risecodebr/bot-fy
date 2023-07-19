using System;
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
    }
}
