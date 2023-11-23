namespace bot_fy.Extensions;

public static class LinqExtensions
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

        return new(lista);
    }

    public static TimeSpan Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, TimeSpan?> func)
    {
        return new(source.Sum(x => func(x).HasValue ? func(x)!.Value.Ticks : 0));
    }

    public static string ToStringTime(this TimeSpan timeSpan)
    {
        return timeSpan.ToString(timeSpan.TotalHours >= 1 ? @"h\hmm\mss\s" : @"mm\mss\s")!;
    }

    public static string ToStringTime(this TimeSpan? timeSpan)
    {
        if (timeSpan!.Value.Ticks == 0)
        {
            return "Ao Vivo";
        }
        return timeSpan!.Value.ToStringTime();
    }
}
