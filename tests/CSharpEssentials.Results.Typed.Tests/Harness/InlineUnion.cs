namespace CSharpEssentials.Results.Typed.Tests.Harness;

/// <summary>
/// A minimal tagged <c>[Union]</c> struct with the K1/K3 shape (one value field, one error field, a tag, a public constructor
/// per case, pure <c>TryGetValue</c>), written into the snippet so the harness can prove the compiler behaviour it asserts
/// without depending on the shipped typed result.
/// </summary>
internal static class InlineUnion
{
    private const string Polyfill = """
        namespace System.Runtime.CompilerServices
        {
            [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
            internal sealed class UnionAttribute : System.Attribute
            {
            }

            internal interface IUnion
            {
                object? Value { get; }
            }
        }

        """;

    private const string Union = """
        using System.Runtime.CompilerServices;
        using CSharpEssentials.Errors;

        [Union]
        public readonly struct R<T, E1, E2> : IUnion
            where T : notnull where E1 : notnull where E2 : notnull
        {
            private readonly T _value;
            private readonly object? _error;
            private readonly byte _tag;

            public R(T value) { _value = value; _error = null; _tag = 1; }
            public R(E1 error) { _value = default!; _error = error; _tag = 2; }
            public R(E2 error) { _value = default!; _error = error; _tag = 3; }

            public object? Value => _tag switch { 1 => _value, 2 or 3 => _error, _ => null };
            public bool HasValue => _tag != 0;

            public bool TryGetValue(out T value) { value = _value; return _tag == 1; }
            public bool TryGetValue(out E1 value) { value = _tag == 2 ? (E1)_error! : default!; return _tag == 2; }
            public bool TryGetValue(out E2 value) { value = _tag == 3 ? (E2)_error! : default!; return _tag == 3; }

            public static implicit operator R<T, E1, E2>(T value) => new(value);
            public static implicit operator R<T, E1, E2>(E1 error) => new(error);
            public static implicit operator R<T, E1, E2>(E2 error) => new(error);
        }

        public sealed class Dto
        {
            public Dto(int id) => Id = id;
            public int Id { get; }
        }

        public static class Probe
        {
            public static readonly NotFoundError NotFound = new("User.NotFound", "User not found");
            public static readonly ConflictError Locked = new("Order.Locked", "Order is locked");

            public static R<Dto, NotFoundError, ConflictError> Get(int id) => id switch { 0 => NotFound, 7 => Locked, _ => new Dto(id) };

        """;

    /// <summary>
    /// Wraps <paramref name="members"/> into the probe class next to the union, adding the internal UnionAttribute and IUnion
    /// polyfill when the referenced framework does not ship them (every framework before net11.0).
    /// </summary>
    public static string With(string members) =>
        Union + "    " + members + "\n}\n" + (ReferencePack.Current.HasUnionTypes ? string.Empty : Polyfill);
}
