using NetEscapades.EnumGenerators;

namespace CSharpEssentials.Enums.Comparison.Benchmarks;

/// <summary>Eight single bits, <see cref="None"/> and one combination member, <see cref="ReadWrite"/>.</summary>
[StringEnum]
[Flags]
[EnumExtensions(ExtensionClassNamespace = Generated.Namespace, ExtensionClassName = "PermissionsFastExtensions")]
public enum Permissions
{
    /// <summary>No permission.</summary>
    None = 0,

    /// <summary>Read.</summary>
    Read = 1,

    /// <summary>Write.</summary>
    Write = 2,

    /// <summary>Execute.</summary>
    Execute = 4,

    /// <summary>Delete.</summary>
    Delete = 8,

    /// <summary>Share.</summary>
    Share = 16,

    /// <summary>Archive.</summary>
    Archive = 32,

    /// <summary>Audit.</summary>
    Audit = 64,

    /// <summary>Admin.</summary>
    Admin = 128,

    /// <summary>Read and write.</summary>
    ReadWrite = Read | Write,
}
