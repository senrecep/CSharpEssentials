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

        Action<T0> addFirst = first.Add;
        Action<T1> addSecond = second.Add;

        foreach (Any<T0, T1> item in source)
            item.Switch(first: addFirst, second: addSecond);

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

        Action<T0> addFirst = first.Add;
        Action<T1> addSecond = second.Add;
        Action<T2> addThird = third.Add;

        foreach (Any<T0, T1, T2> item in source)
            item.Switch(first: addFirst, second: addSecond, third: addThird);

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

        Action<T0> addFirst = first.Add;
        Action<T1> addSecond = second.Add;
        Action<T2> addThird = third.Add;
        Action<T3> addFourth = fourth.Add;

        foreach (Any<T0, T1, T2, T3> item in source)
            item.Switch(first: addFirst, second: addSecond, third: addThird, fourth: addFourth);

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

        Action<T0> addFirst = first.Add;
        Action<T1> addSecond = second.Add;
        Action<T2> addThird = third.Add;
        Action<T3> addFourth = fourth.Add;
        Action<T4> addFifth = fifth.Add;

        foreach (Any<T0, T1, T2, T3, T4> item in source)
            item.Switch(first: addFirst, second: addSecond, third: addThird, fourth: addFourth, fifth: addFifth);

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

        Action<T0> addFirst = first.Add;
        Action<T1> addSecond = second.Add;
        Action<T2> addThird = third.Add;
        Action<T3> addFourth = fourth.Add;
        Action<T4> addFifth = fifth.Add;
        Action<T5> addSixth = sixth.Add;

        foreach (Any<T0, T1, T2, T3, T4, T5> item in source)
            item.Switch(first: addFirst, second: addSecond, third: addThird, fourth: addFourth, fifth: addFifth, sixth: addSixth);

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

        Action<T0> addFirst = first.Add;
        Action<T1> addSecond = second.Add;
        Action<T2> addThird = third.Add;
        Action<T3> addFourth = fourth.Add;
        Action<T4> addFifth = fifth.Add;
        Action<T5> addSixth = sixth.Add;
        Action<T6> addSeventh = seventh.Add;

        foreach (Any<T0, T1, T2, T3, T4, T5, T6> item in source)
            item.Switch(first: addFirst, second: addSecond, third: addThird, fourth: addFourth, fifth: addFifth, sixth: addSixth, seventh: addSeventh);

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

        Action<T0> addFirst = first.Add;
        Action<T1> addSecond = second.Add;
        Action<T2> addThird = third.Add;
        Action<T3> addFourth = fourth.Add;
        Action<T4> addFifth = fifth.Add;
        Action<T5> addSixth = sixth.Add;
        Action<T6> addSeventh = seventh.Add;
        Action<T7> addEighth = eighth.Add;

        foreach (Any<T0, T1, T2, T3, T4, T5, T6, T7> item in source)
            item.Switch(first: addFirst, second: addSecond, third: addThird, fourth: addFourth, fifth: addFifth, sixth: addSixth, seventh: addSeventh, eighth: addEighth);

        return (first.ToArray(), second.ToArray(), third.ToArray(), fourth.ToArray(), fifth.ToArray(), sixth.ToArray(), seventh.ToArray(), eighth.ToArray());
    }
}
