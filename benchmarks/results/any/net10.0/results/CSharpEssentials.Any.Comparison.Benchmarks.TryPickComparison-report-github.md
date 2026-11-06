```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                  | Categories    | Branch | Mean      | Error     | StdDev    | Ratio | Allocated | Alloc Ratio |
|------------------------ |-------------- |------- |----------:|----------:|----------:|------:|----------:|------------:|
| **CSharpEssentials_ByCase** | **ByCase**        | **Int**    | **0.5911 ns** | **0.0047 ns** | **0.0044 ns** |  **1.00** |         **-** |          **NA** |
| OneOf_ByCase            | ByCase        | Int    | 0.2980 ns | 0.0012 ns | 0.0011 ns |  0.50 |         - |          NA |
| Dunet_ByCase            | ByCase        | Int    | 0.5853 ns | 0.0002 ns | 0.0002 ns |  0.99 |         - |          NA |
|                         |               |        |           |           |           |       |           |             |
| **CSharpEssentials_ByCase** | **ByCase**        | **Text**   | **0.3475 ns** | **0.0008 ns** | **0.0007 ns** |  **1.00** |         **-** |          **NA** |
| OneOf_ByCase            | ByCase        | Text   | 0.9146 ns | 0.0009 ns | 0.0009 ns |  2.63 |         - |          NA |
| Dunet_ByCase            | ByCase        | Text   | 0.5857 ns | 0.0005 ns | 0.0004 ns |  1.69 |         - |          NA |
|                         |               |        |           |           |           |       |           |             |
| **CSharpEssentials_TryAs**  | **ByRuntimeType** | **Int**    | **0.5854 ns** | **0.0014 ns** | **0.0011 ns** |  **1.00** |         **-** |          **NA** |
|                         |               |        |           |           |           |       |           |             |
| **CSharpEssentials_TryAs**  | **ByRuntimeType** | **Text**   | **0.5915 ns** | **0.0005 ns** | **0.0005 ns** |  **1.00** |         **-** |          **NA** |
