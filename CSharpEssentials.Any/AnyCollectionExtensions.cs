namespace CSharpEssentials.Any;

public static partial class AnyExtensions
{
    private static IEnumerable<T> EnsureSource<T>(IEnumerable<T> source)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(source);
#else
        if (source is null)
            throw new ArgumentNullException(nameof(source));
#endif

        return source;
    }

    private static Func<TSource, TResult> EnsureSelector<TSource, TResult>(Func<TSource, TResult> selector)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(selector);
#else
        if (selector is null)
            throw new ArgumentNullException(nameof(selector));
#endif

        return selector;
    }

    public static (T0[] First, T1[] Second) Traverse<TSource, T0, T1>(this IEnumerable<TSource> source, Func<TSource, Any<T0, T1>> selector) =>
        EnsureSource(source).Select(EnsureSelector(selector)).Partition();

    public static (T0[] First, T1[] Second) Partition<T0, T1>(this IEnumerable<Any<T0, T1>> source)
    {
        source = EnsureSource(source);
        List<T0> first = [];
        List<T1> second = [];

        foreach (Any<T0, T1> item in source)
            item.Switch(first: first.Add, second: second.Add);

        return (first.ToArray(), second.ToArray());
    }

    public static (T0[] First, T1[] Second, T2[] Third) Traverse<TSource, T0, T1, T2>(this IEnumerable<TSource> source, Func<TSource, Any<T0, T1, T2>> selector) =>
        EnsureSource(source).Select(EnsureSelector(selector)).Partition();

    public static (T0[] First, T1[] Second, T2[] Third) Partition<T0, T1, T2>(this IEnumerable<Any<T0, T1, T2>> source)
    {
        source = EnsureSource(source);
        List<T0> first = [];
        List<T1> second = [];
        List<T2> third = [];

        foreach (Any<T0, T1, T2> item in source)
            item.Switch(first: first.Add, second: second.Add, third: third.Add);

        return (first.ToArray(), second.ToArray(), third.ToArray());
    }

    public static (T0[] First, T1[] Second, T2[] Third, T3[] Fourth) Traverse<TSource, T0, T1, T2, T3>(this IEnumerable<TSource> source, Func<TSource, Any<T0, T1, T2, T3>> selector) =>
        EnsureSource(source).Select(EnsureSelector(selector)).Partition();

    public static (T0[] First, T1[] Second, T2[] Third, T3[] Fourth) Partition<T0, T1, T2, T3>(this IEnumerable<Any<T0, T1, T2, T3>> source)
    {
        source = EnsureSource(source);
        List<T0> first = [];
        List<T1> second = [];
        List<T2> third = [];
        List<T3> fourth = [];

        foreach (Any<T0, T1, T2, T3> item in source)
            item.Switch(first: first.Add, second: second.Add, third: third.Add, fourth: fourth.Add);

        return (first.ToArray(), second.ToArray(), third.ToArray(), fourth.ToArray());
    }

    public static (T0[] First, T1[] Second, T2[] Third, T3[] Fourth, T4[] Fifth) Traverse<TSource, T0, T1, T2, T3, T4>(this IEnumerable<TSource> source, Func<TSource, Any<T0, T1, T2, T3, T4>> selector) =>
        EnsureSource(source).Select(EnsureSelector(selector)).Partition();

    public static (T0[] First, T1[] Second, T2[] Third, T3[] Fourth, T4[] Fifth) Partition<T0, T1, T2, T3, T4>(this IEnumerable<Any<T0, T1, T2, T3, T4>> source)
    {
        source = EnsureSource(source);
        List<T0> first = [];
        List<T1> second = [];
        List<T2> third = [];
        List<T3> fourth = [];
        List<T4> fifth = [];

        foreach (Any<T0, T1, T2, T3, T4> item in source)
            item.Switch(first: first.Add, second: second.Add, third: third.Add, fourth: fourth.Add, fifth: fifth.Add);

        return (first.ToArray(), second.ToArray(), third.ToArray(), fourth.ToArray(), fifth.ToArray());
    }

    public static (T0[] First, T1[] Second, T2[] Third, T3[] Fourth, T4[] Fifth, T5[] Sixth) Traverse<TSource, T0, T1, T2, T3, T4, T5>(this IEnumerable<TSource> source, Func<TSource, Any<T0, T1, T2, T3, T4, T5>> selector) =>
        EnsureSource(source).Select(EnsureSelector(selector)).Partition();

    public static (T0[] First, T1[] Second, T2[] Third, T3[] Fourth, T4[] Fifth, T5[] Sixth) Partition<T0, T1, T2, T3, T4, T5>(this IEnumerable<Any<T0, T1, T2, T3, T4, T5>> source)
    {
        source = EnsureSource(source);
        List<T0> first = [];
        List<T1> second = [];
        List<T2> third = [];
        List<T3> fourth = [];
        List<T4> fifth = [];
        List<T5> sixth = [];

        foreach (Any<T0, T1, T2, T3, T4, T5> item in source)
            item.Switch(first: first.Add, second: second.Add, third: third.Add, fourth: fourth.Add, fifth: fifth.Add, sixth: sixth.Add);

        return (first.ToArray(), second.ToArray(), third.ToArray(), fourth.ToArray(), fifth.ToArray(), sixth.ToArray());
    }

    public static (T0[] First, T1[] Second, T2[] Third, T3[] Fourth, T4[] Fifth, T5[] Sixth, T6[] Seventh) Traverse<TSource, T0, T1, T2, T3, T4, T5, T6>(this IEnumerable<TSource> source, Func<TSource, Any<T0, T1, T2, T3, T4, T5, T6>> selector) =>
        EnsureSource(source).Select(EnsureSelector(selector)).Partition();

    public static (T0[] First, T1[] Second, T2[] Third, T3[] Fourth, T4[] Fifth, T5[] Sixth, T6[] Seventh) Partition<T0, T1, T2, T3, T4, T5, T6>(this IEnumerable<Any<T0, T1, T2, T3, T4, T5, T6>> source)
    {
        source = EnsureSource(source);
        List<T0> first = [];
        List<T1> second = [];
        List<T2> third = [];
        List<T3> fourth = [];
        List<T4> fifth = [];
        List<T5> sixth = [];
        List<T6> seventh = [];

        foreach (Any<T0, T1, T2, T3, T4, T5, T6> item in source)
            item.Switch(first: first.Add, second: second.Add, third: third.Add, fourth: fourth.Add, fifth: fifth.Add, sixth: sixth.Add, seventh: seventh.Add);

        return (first.ToArray(), second.ToArray(), third.ToArray(), fourth.ToArray(), fifth.ToArray(), sixth.ToArray(), seventh.ToArray());
    }

    public static (T0[] First, T1[] Second, T2[] Third, T3[] Fourth, T4[] Fifth, T5[] Sixth, T6[] Seventh, T7[] Eighth) Traverse<TSource, T0, T1, T2, T3, T4, T5, T6, T7>(this IEnumerable<TSource> source, Func<TSource, Any<T0, T1, T2, T3, T4, T5, T6, T7>> selector) =>
        EnsureSource(source).Select(EnsureSelector(selector)).Partition();

    public static (T0[] First, T1[] Second, T2[] Third, T3[] Fourth, T4[] Fifth, T5[] Sixth, T6[] Seventh, T7[] Eighth) Partition<T0, T1, T2, T3, T4, T5, T6, T7>(this IEnumerable<Any<T0, T1, T2, T3, T4, T5, T6, T7>> source)
    {
        source = EnsureSource(source);
        List<T0> first = [];
        List<T1> second = [];
        List<T2> third = [];
        List<T3> fourth = [];
        List<T4> fifth = [];
        List<T5> sixth = [];
        List<T6> seventh = [];
        List<T7> eighth = [];

        foreach (Any<T0, T1, T2, T3, T4, T5, T6, T7> item in source)
            item.Switch(first: first.Add, second: second.Add, third: third.Add, fourth: fourth.Add, fifth: fifth.Add, sixth: sixth.Add, seventh: seventh.Add, eighth: eighth.Add);

        return (first.ToArray(), second.ToArray(), third.ToArray(), fourth.ToArray(), fifth.ToArray(), sixth.ToArray(), seventh.ToArray(), eighth.ToArray());
    }
}
