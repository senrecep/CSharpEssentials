namespace CSharpEssentials.Any;

public static class AnyExtensions
{
    #region Any<T0, T1>

    public static void Deconstruct<T0, T1>(this Any<T0, T1> any, out T0? first, out T1? second)
    {
        first = any.IsFirst ? (T0?)any.Value : default;
        second = any.IsSecond ? (T1?)any.Value : default;
    }

    public static (T0? First, T1? Second) ToTuple<T0, T1>(this Any<T0, T1> any) =>
        (any.IsFirst ? (T0?)any.Value : default, any.IsSecond ? (T1?)any.Value : default);

    public static bool Is<TTarget, T0, T1>(this Any<T0, T1> any) => any.Value is TTarget;

    public static TTarget? As<TTarget, T0, T1>(this Any<T0, T1> any) =>
        any.Value is TTarget typed ? typed : default;

    public static bool TryAs<TTarget, T0, T1>(this Any<T0, T1> any, out TTarget? value)
    {
        if (any.Value is TTarget typed)
        {
            value = typed;
            return true;
        }
        value = default;
        return false;
    }

    #endregion

    #region Any<T0, T1, T2>

    public static void Deconstruct<T0, T1, T2>(this Any<T0, T1, T2> any, out T0? first, out T1? second, out T2? third)
    {
        first = any.IsFirst ? (T0?)any.Value : default;
        second = any.IsSecond ? (T1?)any.Value : default;
        third = any.IsThird ? (T2?)any.Value : default;
    }

    public static (T0? First, T1? Second, T2? Third) ToTuple<T0, T1, T2>(this Any<T0, T1, T2> any) =>
        (any.IsFirst ? (T0?)any.Value : default, any.IsSecond ? (T1?)any.Value : default, any.IsThird ? (T2?)any.Value : default);

    public static bool Is<TTarget, T0, T1, T2>(this Any<T0, T1, T2> any) => any.Value is TTarget;

    public static TTarget? As<TTarget, T0, T1, T2>(this Any<T0, T1, T2> any) =>
        any.Value is TTarget typed ? typed : default;

    public static bool TryAs<TTarget, T0, T1, T2>(this Any<T0, T1, T2> any, out TTarget? value)
    {
        if (any.Value is TTarget typed)
        {
            value = typed;
            return true;
        }
        value = default;
        return false;
    }

    #endregion

    #region Any<T0, T1, T2, T3>

    public static void Deconstruct<T0, T1, T2, T3>(this Any<T0, T1, T2, T3> any, out T0? first, out T1? second, out T2? third, out T3? fourth)
    {
        first = any.IsFirst ? (T0?)any.Value : default;
        second = any.IsSecond ? (T1?)any.Value : default;
        third = any.IsThird ? (T2?)any.Value : default;
        fourth = any.IsFourth ? (T3?)any.Value : default;
    }

    public static (T0? First, T1? Second, T2? Third, T3? Fourth) ToTuple<T0, T1, T2, T3>(this Any<T0, T1, T2, T3> any) =>
        (any.IsFirst ? (T0?)any.Value : default, any.IsSecond ? (T1?)any.Value : default, any.IsThird ? (T2?)any.Value : default, any.IsFourth ? (T3?)any.Value : default);

    public static bool Is<TTarget, T0, T1, T2, T3>(this Any<T0, T1, T2, T3> any) => any.Value is TTarget;

    public static TTarget? As<TTarget, T0, T1, T2, T3>(this Any<T0, T1, T2, T3> any) =>
        any.Value is TTarget typed ? typed : default;

    public static bool TryAs<TTarget, T0, T1, T2, T3>(this Any<T0, T1, T2, T3> any, out TTarget? value)
    {
        if (any.Value is TTarget typed)
        {
            value = typed;
            return true;
        }
        value = default;
        return false;
    }

    #endregion

    #region Any<T0, T1, T2, T3, T4>

    public static void Deconstruct<T0, T1, T2, T3, T4>(this Any<T0, T1, T2, T3, T4> any, out T0? first, out T1? second, out T2? third, out T3? fourth, out T4? fifth)
    {
        first = any.IsFirst ? (T0?)any.Value : default;
        second = any.IsSecond ? (T1?)any.Value : default;
        third = any.IsThird ? (T2?)any.Value : default;
        fourth = any.IsFourth ? (T3?)any.Value : default;
        fifth = any.IsFifth ? (T4?)any.Value : default;
    }

    public static (T0? First, T1? Second, T2? Third, T3? Fourth, T4? Fifth) ToTuple<T0, T1, T2, T3, T4>(this Any<T0, T1, T2, T3, T4> any) =>
        (any.IsFirst ? (T0?)any.Value : default, any.IsSecond ? (T1?)any.Value : default, any.IsThird ? (T2?)any.Value : default, any.IsFourth ? (T3?)any.Value : default, any.IsFifth ? (T4?)any.Value : default);

    public static bool Is<TTarget, T0, T1, T2, T3, T4>(this Any<T0, T1, T2, T3, T4> any) => any.Value is TTarget;

    public static TTarget? As<TTarget, T0, T1, T2, T3, T4>(this Any<T0, T1, T2, T3, T4> any) =>
        any.Value is TTarget typed ? typed : default;

    public static bool TryAs<TTarget, T0, T1, T2, T3, T4>(this Any<T0, T1, T2, T3, T4> any, out TTarget? value)
    {
        if (any.Value is TTarget typed)
        {
            value = typed;
            return true;
        }
        value = default;
        return false;
    }

    #endregion

    #region Any<T0, T1, T2, T3, T4, T5>

    public static void Deconstruct<T0, T1, T2, T3, T4, T5>(this Any<T0, T1, T2, T3, T4, T5> any, out T0? first, out T1? second, out T2? third, out T3? fourth, out T4? fifth, out T5? sixth)
    {
        first = any.IsFirst ? (T0?)any.Value : default;
        second = any.IsSecond ? (T1?)any.Value : default;
        third = any.IsThird ? (T2?)any.Value : default;
        fourth = any.IsFourth ? (T3?)any.Value : default;
        fifth = any.IsFifth ? (T4?)any.Value : default;
        sixth = any.IsSixth ? (T5?)any.Value : default;
    }

    public static (T0? First, T1? Second, T2? Third, T3? Fourth, T4? Fifth, T5? Sixth) ToTuple<T0, T1, T2, T3, T4, T5>(this Any<T0, T1, T2, T3, T4, T5> any) =>
        (any.IsFirst ? (T0?)any.Value : default, any.IsSecond ? (T1?)any.Value : default, any.IsThird ? (T2?)any.Value : default, any.IsFourth ? (T3?)any.Value : default, any.IsFifth ? (T4?)any.Value : default, any.IsSixth ? (T5?)any.Value : default);

    public static bool Is<TTarget, T0, T1, T2, T3, T4, T5>(this Any<T0, T1, T2, T3, T4, T5> any) => any.Value is TTarget;

    public static TTarget? As<TTarget, T0, T1, T2, T3, T4, T5>(this Any<T0, T1, T2, T3, T4, T5> any) =>
        any.Value is TTarget typed ? typed : default;

    public static bool TryAs<TTarget, T0, T1, T2, T3, T4, T5>(this Any<T0, T1, T2, T3, T4, T5> any, out TTarget? value)
    {
        if (any.Value is TTarget typed)
        {
            value = typed;
            return true;
        }
        value = default;
        return false;
    }

    #endregion

    #region Any<T0, T1, T2, T3, T4, T5, T6>

    public static void Deconstruct<T0, T1, T2, T3, T4, T5, T6>(this Any<T0, T1, T2, T3, T4, T5, T6> any, out T0? first, out T1? second, out T2? third, out T3? fourth, out T4? fifth, out T5? sixth, out T6? seventh)
    {
        first = any.IsFirst ? (T0?)any.Value : default;
        second = any.IsSecond ? (T1?)any.Value : default;
        third = any.IsThird ? (T2?)any.Value : default;
        fourth = any.IsFourth ? (T3?)any.Value : default;
        fifth = any.IsFifth ? (T4?)any.Value : default;
        sixth = any.IsSixth ? (T5?)any.Value : default;
        seventh = any.IsSeventh ? (T6?)any.Value : default;
    }

    public static (T0? First, T1? Second, T2? Third, T3? Fourth, T4? Fifth, T5? Sixth, T6? Seventh) ToTuple<T0, T1, T2, T3, T4, T5, T6>(this Any<T0, T1, T2, T3, T4, T5, T6> any) =>
        (any.IsFirst ? (T0?)any.Value : default, any.IsSecond ? (T1?)any.Value : default, any.IsThird ? (T2?)any.Value : default, any.IsFourth ? (T3?)any.Value : default, any.IsFifth ? (T4?)any.Value : default, any.IsSixth ? (T5?)any.Value : default, any.IsSeventh ? (T6?)any.Value : default);

    public static bool Is<TTarget, T0, T1, T2, T3, T4, T5, T6>(this Any<T0, T1, T2, T3, T4, T5, T6> any) => any.Value is TTarget;

    public static TTarget? As<TTarget, T0, T1, T2, T3, T4, T5, T6>(this Any<T0, T1, T2, T3, T4, T5, T6> any) =>
        any.Value is TTarget typed ? typed : default;

    public static bool TryAs<TTarget, T0, T1, T2, T3, T4, T5, T6>(this Any<T0, T1, T2, T3, T4, T5, T6> any, out TTarget? value)
    {
        if (any.Value is TTarget typed)
        {
            value = typed;
            return true;
        }
        value = default;
        return false;
    }

    #endregion

    #region Any<T0, T1, T2, T3, T4, T5, T6, T7>

    public static void Deconstruct<T0, T1, T2, T3, T4, T5, T6, T7>(this Any<T0, T1, T2, T3, T4, T5, T6, T7> any, out T0? first, out T1? second, out T2? third, out T3? fourth, out T4? fifth, out T5? sixth, out T6? seventh, out T7? eighth)
    {
        first = any.IsFirst ? (T0?)any.Value : default;
        second = any.IsSecond ? (T1?)any.Value : default;
        third = any.IsThird ? (T2?)any.Value : default;
        fourth = any.IsFourth ? (T3?)any.Value : default;
        fifth = any.IsFifth ? (T4?)any.Value : default;
        sixth = any.IsSixth ? (T5?)any.Value : default;
        seventh = any.IsSeventh ? (T6?)any.Value : default;
        eighth = any.IsEighth ? (T7?)any.Value : default;
    }

    public static (T0? First, T1? Second, T2? Third, T3? Fourth, T4? Fifth, T5? Sixth, T6? Seventh, T7? Eighth) ToTuple<T0, T1, T2, T3, T4, T5, T6, T7>(this Any<T0, T1, T2, T3, T4, T5, T6, T7> any) =>
        (any.IsFirst ? (T0?)any.Value : default, any.IsSecond ? (T1?)any.Value : default, any.IsThird ? (T2?)any.Value : default, any.IsFourth ? (T3?)any.Value : default, any.IsFifth ? (T4?)any.Value : default, any.IsSixth ? (T5?)any.Value : default, any.IsSeventh ? (T6?)any.Value : default, any.IsEighth ? (T7?)any.Value : default);

    public static bool Is<TTarget, T0, T1, T2, T3, T4, T5, T6, T7>(this Any<T0, T1, T2, T3, T4, T5, T6, T7> any) => any.Value is TTarget;

    public static TTarget? As<TTarget, T0, T1, T2, T3, T4, T5, T6, T7>(this Any<T0, T1, T2, T3, T4, T5, T6, T7> any) =>
        any.Value is TTarget typed ? typed : default;

    public static bool TryAs<TTarget, T0, T1, T2, T3, T4, T5, T6, T7>(this Any<T0, T1, T2, T3, T4, T5, T6, T7> any, out TTarget? value)
    {
        if (any.Value is TTarget typed)
        {
            value = typed;
            return true;
        }
        value = default;
        return false;
    }

    #endregion
}
