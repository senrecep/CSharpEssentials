```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                  | Categories    | Branch | Mean      | Error     | StdDev    | Ratio | Allocated | Alloc Ratio |
|------------------------ |-------------- |------- |----------:|----------:|----------:|------:|----------:|------------:|
| **CSharpEssentials_ByCase** | **ByCase**        | **Int**    | **1.1606 ns** | **0.0002 ns** | **0.0002 ns** |  **1.00** |         **-** |          **NA** |
| OneOf_ByCase            | ByCase        | Int    | 0.8732 ns | 0.0002 ns | 0.0001 ns |  0.75 |         - |          NA |
| Dunet_ByCase            | ByCase        | Int    | 0.8708 ns | 0.0005 ns | 0.0005 ns |  0.75 |         - |          NA |
|                         |               |        |           |           |           |       |           |             |
| **CSharpEssentials_ByCase** | **ByCase**        | **Text**   | **0.6145 ns** | **0.0007 ns** | **0.0006 ns** |  **1.00** |         **-** |          **NA** |
| OneOf_ByCase            | ByCase        | Text   | 1.4851 ns | 0.0003 ns | 0.0003 ns |  2.42 |         - |          NA |
| Dunet_ByCase            | ByCase        | Text   | 0.5858 ns | 0.0002 ns | 0.0002 ns |  0.95 |         - |          NA |
|                         |               |        |           |           |           |       |           |             |
| **CSharpEssentials_TryAs**  | **ByRuntimeType** | **Int**    | **0.7271 ns** | **0.0001 ns** | **0.0001 ns** |  **1.00** |         **-** |          **NA** |
|                         |               |        |           |           |           |       |           |             |
| **CSharpEssentials_TryAs**  | **ByRuntimeType** | **Text**   | **0.6570 ns** | **0.0003 ns** | **0.0002 ns** |  **1.00** |         **-** |          **NA** |
