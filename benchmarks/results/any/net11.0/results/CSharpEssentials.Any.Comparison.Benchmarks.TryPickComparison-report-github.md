```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                  | Categories    | Branch | Mean      | Error     | StdDev    | Ratio | Allocated | Alloc Ratio |
|------------------------ |-------------- |------- |----------:|----------:|----------:|------:|----------:|------------:|
| **CSharpEssentials_ByCase** | **ByCase**        | **Int**    | **0.5898 ns** | **0.0017 ns** | **0.0015 ns** |  **1.00** |         **-** |          **NA** |
| OneOf_ByCase            | ByCase        | Int    | 0.2974 ns | 0.0002 ns | 0.0001 ns |  0.50 |         - |          NA |
| Dunet_ByCase            | ByCase        | Int    | 0.5831 ns | 0.0002 ns | 0.0002 ns |  0.99 |         - |          NA |
|                         |               |        |           |           |           |       |           |             |
| **CSharpEssentials_ByCase** | **ByCase**        | **Text**   | **0.3481 ns** | **0.0009 ns** | **0.0008 ns** |  **1.00** |         **-** |          **NA** |
| OneOf_ByCase            | ByCase        | Text   | 0.8900 ns | 0.0005 ns | 0.0004 ns |  2.56 |         - |          NA |
| Dunet_ByCase            | ByCase        | Text   | 0.5839 ns | 0.0004 ns | 0.0003 ns |  1.68 |         - |          NA |
|                         |               |        |           |           |           |       |           |             |
| **CSharpEssentials_TryAs**  | **ByRuntimeType** | **Int**    | **0.5828 ns** | **0.0002 ns** | **0.0001 ns** |  **1.00** |         **-** |          **NA** |
|                         |               |        |           |           |           |       |           |             |
| **CSharpEssentials_TryAs**  | **ByRuntimeType** | **Text**   | **0.5841 ns** | **0.0001 ns** | **0.0001 ns** |  **1.00** |         **-** |          **NA** |
