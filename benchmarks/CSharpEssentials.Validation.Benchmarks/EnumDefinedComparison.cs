using BenchmarkDotNet.Attributes;
using CSharpEssentials.Enums;
using Annotations = System.ComponentModel.DataAnnotations;

namespace CSharpEssentials.Validation.Benchmarks;

/// <summary>
/// Is the enum property a defined member: CSE <c>IsDefinedEnum</c>, FluentValidation <c>IsInEnum</c>, DataAnnotations
/// <c>[EnumDataType]</c> and a hand-written <c>Enum.IsDefined</c> baseline. Like-for-like on outcome. With
/// <see cref="MetadataKind.Generated"/> the CSE row reads the <c>[StringEnum]</c> metadata (<c>EnumInfo</c>); the other libraries
/// treat <see cref="ShipmentChannel"/> as a plain enum. The invalid path differs in error cost: CSE builds an
/// <c>EnumValueError</c> message that lists the allowed values. Each row returns whether the input is valid; the setup checks
/// the full outcome of the same validator instances.
/// </summary>
[MemoryDiagnoser]
public class EnumDefinedComparison
{
    private readonly CseStatusValidator _cseStatus = new();
    private readonly FvStatusValidator _fvStatus = new();
    private readonly CseChannelValidator _cseChannel = new();
    private readonly FvChannelValidator _fvChannel = new();
    private StatusRequest _status = null!;
    private ChannelRequest _channel = null!;
    private bool _plain;

    [Params(MetadataKind.Plain, MetadataKind.Generated)]
    public MetadataKind Metadata { get; set; }

    [Params(true, false)]
    public bool Valid { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        if (!EnumMetadata.TryGet<ShipmentChannel>(out _))
            throw new InvalidOperationException("Self-check failed in EnumDefinedComparison: ShipmentChannel has no generated metadata.");
        _plain = Metadata == MetadataKind.Plain;
        _status = new StatusRequest { Status = Valid ? ShipmentStatus.Pending : (ShipmentStatus)42 };
        _channel = new ChannelRequest { Channel = Valid ? ShipmentChannel.Web : (ShipmentChannel)42 };
        string property = _plain ? nameof(StatusRequest.Status) : nameof(ChannelRequest.Channel);
        string expected = Valid ? "ok" : "fail:" + property;
        Parity.Same(nameof(EnumDefinedComparison),
            ("expected", expected),
            (nameof(CSharpEssentials), _plain ? Parity.Of(_cseStatus.ValidateAsync(_status).Run()) : Parity.Of(_cseChannel.ValidateAsync(_channel).Run())),
            (nameof(FluentValidation), _plain ? Parity.Of(_fvStatus.Validate(_status)) : Parity.Of(_fvChannel.Validate(_channel))),
            (nameof(DataAnnotations), Parity.Of(Annotate(_plain ? _status : _channel))),
            (nameof(HandWritten), HandWritten() ? "ok" : "fail:" + property));
        Parity.Same(nameof(EnumDefinedComparison),
            ("expected", Valid.ToString()),
            (nameof(CSharpEssentials), CSharpEssentials().ToString()),
            (nameof(FluentValidation), FluentValidation().ToString()),
            (nameof(DataAnnotations), DataAnnotations().ToString()));
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Validation/Readme.MD#enums-tenum-tenum
    [Benchmark(Baseline = true)]
    public bool CSharpEssentials() =>
        _plain ? _cseStatus.ValidateAsync(_status).Run().IsSuccess : _cseChannel.ValidateAsync(_channel).Run().IsSuccess;

    // https://docs.fluentvalidation.net/en/latest/built-in-validators.html#enum-validator
    [Benchmark]
    public bool FluentValidation() => _plain ? _fvStatus.Validate(_status).IsValid : _fvChannel.Validate(_channel).IsValid;

    // https://learn.microsoft.com/dotnet/api/system.componentmodel.dataannotations.enumdatatypeattribute
    [Benchmark]
    public bool DataAnnotations()
    {
        object model = _plain ? _status : _channel;
        var results = new List<Annotations.ValidationResult>();
        return Annotations.Validator.TryValidateObject(model, new Annotations.ValidationContext(model), results, validateAllProperties: true);
    }

    // https://learn.microsoft.com/dotnet/api/system.enum.isdefined
    [Benchmark]
    public bool HandWritten() => _plain ? Enum.IsDefined(_status.Status) : Enum.IsDefined(_channel.Channel);

    private static List<Annotations.ValidationResult> Annotate(object model)
    {
        var results = new List<Annotations.ValidationResult>();
        Annotations.Validator.TryValidateObject(model, new Annotations.ValidationContext(model), results, validateAllProperties: true);
        return results;
    }
}
