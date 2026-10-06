using System.Globalization;
using CSharpEssentials.EntityFrameworkCore.Converters;
using CSharpEssentials.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Json;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CSharpEssentials.EntityFrameworkCore;

/// <summary>
/// Configures storage, column facets and check constraints of enum properties once user configuration is complete (design section 11).
/// </summary>
internal sealed class EnumStorageConvention : IModelFinalizingConvention
{
    internal const string PostgresProvider = "Npgsql.EntityFrameworkCore.PostgreSQL";
    internal const string SqliteProvider = "Microsoft.EntityFrameworkCore.Sqlite";

    private readonly EnumConventions _conventions;
    private readonly EnumStoredAs? _existingStorage;
    private readonly Func<Type, IEnumInfo>? _reflectionFallback;
    private readonly string? _providerName;
    private readonly ITypeMappingSource _typeMappingSource;
    private readonly IRelationalTypeMappingSource? _relationalTypeMappingSource;
    private readonly ISqlGenerationHelper? _sqlGenerationHelper;
    private readonly Dictionary<(StoreObjectIdentifier Table, string Name), (IConventionEntityType Owner, string Sql)> _constraints = [];
    private readonly HashSet<(StoreObjectIdentifier Table, string Name)> _conflicts = [];

    public EnumStorageConvention(
        EnumConventions conventions,
        EnumStoredAs? existingStorage,
        Func<Type, IEnumInfo>? reflectionFallback,
        string? providerName,
        ITypeMappingSource typeMappingSource,
        IRelationalTypeMappingSource? relationalTypeMappingSource,
        ISqlGenerationHelper? sqlGenerationHelper)
    {
        _conventions = conventions;
        _existingStorage = existingStorage;
        _reflectionFallback = reflectionFallback;
        _providerName = providerName;
        _typeMappingSource = typeMappingSource;
        _relationalTypeMappingSource = relationalTypeMappingSource;
        _sqlGenerationHelper = sqlGenerationHelper;
    }

    private bool IsPostgres => _providerName == PostgresProvider;

    private bool IsRelational => _sqlGenerationHelper is not null && _relationalTypeMappingSource is not null;

    public void ProcessModelFinalizing(IConventionModelBuilder modelBuilder, IConventionContext<IConventionModelBuilder> context)
    {
        IConventionModel model = modelBuilder.Metadata;
        foreach (IConventionEntityType entityType in model.GetEntityTypes().ToList())
            ProcessType(model, entityType, entityType, IsRelational && entityType.IsMappedToJson());
    }

    private void ProcessType(IConventionModel model, IConventionTypeBase type, IConventionEntityType tableEntity, bool inJson)
    {
        foreach (IConventionProperty property in type.GetDeclaredProperties().ToList())
            ProcessProperty(model, property, tableEntity, inJson);
        foreach (IConventionComplexProperty complexProperty in type.GetDeclaredComplexProperties().ToList())
        {
            IConventionComplexType complexType = complexProperty.ComplexType;
            ProcessType(model, complexType, tableEntity, inJson || IsRelational && complexType.IsMappedToJson());
        }
    }

    private void ProcessProperty(IConventionModel model, IConventionProperty property, IConventionEntityType tableEntity, bool inJson)
    {
        EnumStorage? storage = Take<string>(property, EnumPropertyBuilderExtensions.StorageAnnotation) is { } storageText
            ? Enum.Parse<EnumStorage>(storageText)
            : null;
        EnumStoredAs? legacy = Take<string>(property, EnumPropertyBuilderExtensions.LegacyStorageAnnotation) is { } legacyText
            ? Enum.Parse<EnumStoredAs>(legacyText)
            : null;
        bool? checkConstraint = Take<bool?>(property, EnumPropertyBuilderExtensions.CheckConstraintAnnotation);
        bool configured = storage is not null || legacy is not null || checkConstraint is not null;

        Type? enumType = EnumTypeOf(property.ClrType);
        IConventionElementType? element = enumType is null ? property.GetElementType() : null;
        if (element is not null)
            enumType = EnumTypeOf(element.ClrType);

        if (enumType is null)
        {
            if (configured)
            {
                throw new InvalidOperationException(
                    $"'{Describe(property)}' is not an enum, nullable enum or enum collection property. HasEnumStorage, HasLegacyEnumStorage and HasEnumCheckConstraint apply only to those.");
            }

            return;
        }

        if (HasUserConversion(property) || HasUserConversion(element))
            return;

        IEnumInfo? info = Resolve(enumType, property, configured);
        if (info is null)
            return;

        EnumStoredAs? legacyFormat = legacy ?? (storage is null ? _existingStorage : null);
        EnumStorage resolvedStorage = storage ?? info.Storage;
        if (resolvedStorage == EnumStorage.Default)
            resolvedStorage = info.IsFlags ? _conventions.FlagsStorage : _conventions.Storage;
        if (resolvedStorage == EnumStorage.Default)
            resolvedStorage = info.IsFlags ? EnumStorage.Integer : EnumStorage.String;

        info.Accept(new PropertyConfigurator(
            this,
            model,
            property,
            element,
            tableEntity,
            inJson,
            resolvedStorage,
            legacyFormat,
            checkConstraint ?? true));
    }

    private IEnumInfo? Resolve(Type enumType, IConventionProperty property, bool configured)
    {
        if (EnumMetadata.TryGet(enumType, out IEnumInfo? info))
            return configured || _conventions.CanHandle(enumType) ? info : null;
        if (_reflectionFallback is not null && (configured || _conventions.CanHandle(enumType)))
            return _reflectionFallback(enumType);
        if (enumType.IsDefined(typeof(StringEnumAttribute), inherit: false))
        {
            throw new InvalidOperationException(
                $"Enum '{enumType.FullName}' is marked [StringEnum] but has no generated metadata. Rebuild its project with the CSharpEssentials.Enums 5.0 generator (C# 9 or newer) and make the enum and its containing types public or internal (not private, protected, file-local or nested in a generic type).");
        }

        return configured
            ? throw new InvalidOperationException(
                $"'{Describe(property)}' uses HasEnumStorage, HasLegacyEnumStorage or HasEnumCheckConstraint, but enum '{enumType.FullName}' has no generated metadata. Mark it [StringEnum], or use ConfigureEnumConventionsWithReflection.")
            : null;
    }

    private static T? Take<T>(IConventionProperty property, string name)
    {
        IConventionAnnotation? annotation = property.FindAnnotation(name);
        if (annotation is null)
            return default;

        property.RemoveAnnotation(name);
        return (T?)annotation.Value;
    }

    private static Type? EnumTypeOf(Type clrType)
    {
        Type type = Nullable.GetUnderlyingType(clrType) ?? clrType;
        return type.IsEnum ? type : null;
    }

    private static bool HasUserConversion(IConventionProperty property) =>
        property.GetValueConverter() is not null || property.GetProviderClrType() is not null;

    private static bool HasUserConversion(IConventionElementType? element) =>
        element is not null && (element.GetValueConverter() is not null || element.GetProviderClrType() is not null);

    private static string Describe(IConventionProperty property) => $"{property.DeclaringType.DisplayName()}.{property.Name}";

    private sealed class PropertyConfigurator(
        EnumStorageConvention convention,
        IConventionModel model,
        IConventionProperty property,
        IConventionElementType? element,
        IConventionEntityType tableEntity,
        bool inJson,
        EnumStorage storage,
        EnumStoredAs? legacyFormat,
        bool checkConstraint) : IEnumInfoVisitor<bool>
    {
        public bool Visit<TEnum>(EnumInfo<TEnum> info) where TEnum : struct, Enum
        {
            StoreObjectIdentifier? table = inJson ? null : StoreObjectIdentifier.Create(tableEntity, StoreObjectType.Table);
            string? column = table is { } t ? property.GetColumnName(t) : null;
            string label = table is { } named && column is not null
                ? $"{named.DisplayName()}.{column} ({Describe(property)})"
                : Describe(property);

            bool integer = legacyFormat == EnumStoredAs.Integer || legacyFormat is null && storage == EnumStorage.Integer;
            EnumColumnCodec<TEnum> codec = new(info, convention._conventions, integer ? null : legacyFormat, label);
            bool flagsArray = element is null && !inJson && info.IsFlags && !integer && legacyFormat is null;

            ValueConverter converter = CreateConverter(info, codec, integer, flagsArray, label);

            if (element is not null)
                element.Builder.HasConversion(converter);
            else
                property.Builder.HasConversion(converter);

            // A text[] provider mapping brings an array comparer; the property itself holds a single enum value.
            if (flagsArray && convention.IsPostgres)
                property.Builder.HasValueComparer(ValueComparer.CreateDefault<TEnum>(favorStructuralComparisons: false));

            if (inJson && element is null)
                SetJsonReaderWriter(new EnumJsonValueReaderWriter<TEnum>(info, convention._conventions, storage, legacyFormat));

            if (!convention.IsRelational || table is null || column is null || legacyFormat is not null)
                return true;

            if (element is null && !info.IsFlags && !integer && property.GetMaxLength() is null &&
                convention._providerName is not PostgresProvider and not SqliteProvider && info.WireNames.Count > 0)
            {
                property.Builder.HasMaxLength(RoundUp(info.WireNames.Max(name => name.Length)));
            }

            if (checkConstraint && convention._conventions.CheckConstraints &&
                CreateCheckSql(info, integer, column) is { } sql)
            {
                convention.AddCheckConstraint(model, tableEntity, table.Value, column, sql);
            }

            return true;
        }

        private void SetJsonReaderWriter(JsonValueReaderWriter readerWriter)
        {
            CoreTypeMapping? mapping = convention._typeMappingSource.FindMapping((IProperty)property);
            if (mapping is null)
                return;

            // EF sets a convention-level reader/writer on enum properties in JSON, which would win over the mapping's.
            if (property.GetJsonValueReaderWriterTypeConfigurationSource() == ConfigurationSource.Convention)
                property.SetJsonValueReaderWriterType(null);
            property.Builder.HasTypeMapping(mapping.WithComposedConverter(null, jsonValueReaderWriter: readerWriter));
        }

        private string? CreateCheckSql<TEnum>(EnumInfo<TEnum> info, bool integer, string column) where TEnum : struct, Enum
        {
            ISqlGenerationHelper sql = convention._sqlGenerationHelper!;
            IRelationalTypeMappingSource mappings = convention._relationalTypeMappingSource!;
            string columnSql = sql.DelimitIdentifier(column);
            if (info.TypedMembers.Count == 0)
                return null;

            RelationalTypeMapping numberMapping = mappings.FindMapping(info.UnderlyingType)!;
            RelationalTypeMapping textMapping = mappings.FindMapping(typeof(string))!;
            string numbers = string.Join(", ", info.TypedMembers
                .Select(member => member.RawValue)
                .Distinct()
                .Select(raw => numberMapping.GenerateSqlLiteral(ToNumber(info, raw))));
            string names = string.Join(", ", info.WireNames.Select(name => textMapping.GenerateSqlLiteral(name)));

            if (element is not null)
            {
                if (!convention.IsPostgres)
                    return null;

                string arraySql = element.IsNullable ? $"array_remove({columnSql}, NULL)" : columnSql;
                return integer
                    ? $"{arraySql} <@ ARRAY[{numbers}]::{numberMapping.StoreType}[]"
                    : $"{arraySql} <@ ARRAY[{names}]::text[]";
            }

            if (info.IsFlags)
            {
                if (!integer)
                    return convention.IsPostgres ? $"{columnSql} <@ ARRAY[{names}]::text[]" : null;
                if (info.UnderlyingType == typeof(uint) || info.UnderlyingType == typeof(ulong))
                    return null;

                return $"({columnSql} & ~{numberMapping.GenerateSqlLiteral(ToNumber(info, info.DefinedMask))}) = 0";
            }

            return $"{columnSql} IN ({(integer ? numbers : names)})";
        }

        private ValueConverter CreateConverter<TEnum>(EnumInfo<TEnum> info, EnumColumnCodec<TEnum> codec, bool integer, bool flagsArray, string label)
            where TEnum : struct, Enum
        {
            if (integer)
                return CreateIntegerConverter(info, label);
            if (legacyFormat is not null)
                return new ValueConverter<TEnum, string>(value => codec.ToText(value), text => codec.FromText(text));
            if (!flagsArray)
                return new EnumWireNameConverter<TEnum>(info, convention._conventions, label);

            return convention.IsPostgres
                ? new ValueConverter<TEnum, string[]>(value => codec.ToArray(value), names => codec.FromArray(names))
                : new ValueConverter<TEnum, string>(value => codec.ToJsonArray(value), text => codec.FromJsonArray(text));
        }

        private ValueConverter CreateIntegerConverter<TEnum>(EnumInfo<TEnum> info, string label) where TEnum : struct, Enum
        {
            EnumConventions c = convention._conventions;
            Type type = info.UnderlyingType;
            if (type == typeof(sbyte))
                return new EnumIntegerConverter<TEnum, sbyte>(info, c, label);
            if (type == typeof(byte))
                return new EnumIntegerConverter<TEnum, byte>(info, c, label);
            if (type == typeof(short))
                return new EnumIntegerConverter<TEnum, short>(info, c, label);
            if (type == typeof(ushort))
                return new EnumIntegerConverter<TEnum, ushort>(info, c, label);
            if (type == typeof(uint))
                return new EnumIntegerConverter<TEnum, uint>(info, c, label);
            if (type == typeof(long))
                return new EnumIntegerConverter<TEnum, long>(info, c, label);
            if (type == typeof(ulong))
                return new EnumIntegerConverter<TEnum, ulong>(info, c, label);
            return new EnumIntegerConverter<TEnum, int>(info, c, label);
        }

        private static object ToNumber<TEnum>(EnumInfo<TEnum> info, ulong raw) where TEnum : struct, Enum =>
            Convert.ChangeType(info.FromRawValue(raw), info.UnderlyingType, CultureInfo.InvariantCulture);

        private static int RoundUp(int length) => (length + 15) / 16 * 16;
    }

    private void AddCheckConstraint(IConventionModel model, IConventionEntityType entityType, StoreObjectIdentifier table, string column, string sql)
    {
        string name = ConstraintName($"ck_{table.Name}_{column}_enum", model.GetMaxIdentifierLength());
        (StoreObjectIdentifier, string) key = (table, name);
        if (_conflicts.Contains(key))
            return;

        IConventionEntityType owner = entityType.GetRootType() is var root && StoreObjectIdentifier.Create(root, StoreObjectType.Table) == table
            ? root
            : entityType;
        if (_constraints.TryGetValue(key, out (IConventionEntityType Owner, string Sql) existing))
        {
            if (existing.Sql == sql)
                return;

            existing.Owner.RemoveCheckConstraint(name);
            _conflicts.Add(key);
            return;
        }

        if (owner.Builder.HasCheckConstraint(name, sql) is not null)
            _constraints[key] = (owner, sql);
    }

    internal static string ConstraintName(string name, int maxLength)
    {
        if (name.Length <= maxLength)
            return name;

        uint hash = 2166136261;
        foreach (char c in name)
            hash = (hash ^ c) * 16777619;
        string suffix = "_" + hash.ToString("x8", CultureInfo.InvariantCulture);
        return string.Concat(name.AsSpan(0, maxLength - suffix.Length), suffix);
    }
}
