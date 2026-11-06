```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                  | Categories    | Branch | Mean      | Error     | StdDev    | Ratio | Allocated | Alloc Ratio |
|------------------------ |-------------- |------- |----------:|----------:|----------:|------:|----------:|------------:|
| **CSharpEssentials_ByCase** | **ByCase**        | **Int**    | **0.8713 ns** | **0.0014 ns** | **0.0011 ns** |  **1.00** |         **-** |          **NA** |
| OneOf_ByCase            | ByCase        | Int    | 0.4089 ns | 0.0003 ns | 0.0002 ns |  0.47 |         - |          NA |
| Dunet_ByCase            | ByCase        | Int    | 0.8708 ns | 0.0010 ns | 0.0009 ns |  1.00 |         - |          NA |
|                         |               |        |           |           |           |       |           |             |
| **CSharpEssentials_ByCase** | **ByCase**        | **Text**   | **0.5906 ns** | **0.0050 ns** | **0.0047 ns** |  **1.00** |         **-** |          **NA** |
| OneOf_ByCase            | ByCase        | Text   | 0.8934 ns | 0.0042 ns | 0.0035 ns |  1.51 |         - |          NA |
| Dunet_ByCase            | ByCase        | Text   | 0.8709 ns | 0.0014 ns | 0.0011 ns |  1.47 |         - |          NA |
|                         |               |        |           |           |           |       |           |             |
| **CSharpEssentials_TryAs**  | **ByRuntimeType** | **Int**    | **0.5847 ns** | **0.0001 ns** | **0.0001 ns** |  **1.00** |         **-** |          **NA** |
|                         |               |        |           |           |           |       |           |             |
| **CSharpEssentials_TryAs**  | **ByRuntimeType** | **Text**   | **0.5874 ns** | **0.0030 ns** | **0.0028 ns** |  **1.00** |         **-** |          **NA** |
