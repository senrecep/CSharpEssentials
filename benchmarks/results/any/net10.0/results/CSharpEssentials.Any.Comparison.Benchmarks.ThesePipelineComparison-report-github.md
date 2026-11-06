```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2

Categories=MapFlatMapMatch  

```
| Method                    | State | Mean       | Error     | StdDev    | Ratio | Allocated | Alloc Ratio |
|-------------------------- |------ |-----------:|----------:|----------:|------:|----------:|------------:|
| **CSharpEssentials_Pipeline** | **Left**  | **12.8644 ns** | **0.0137 ns** | **0.0122 ns** |  **1.00** |         **-** |          **NA** |
| HandWritten_Pipeline      | Left  |  1.7627 ns | 0.0034 ns | 0.0030 ns |  0.14 |         - |          NA |
|                           |       |            |           |           |       |           |             |
| **CSharpEssentials_Pipeline** | **Right** | **22.3331 ns** | **0.0222 ns** | **0.0208 ns** |  **1.00** |         **-** |          **NA** |
| HandWritten_Pipeline      | Right |  1.3662 ns | 0.0024 ns | 0.0023 ns |  0.06 |         - |          NA |
|                           |       |            |           |           |       |           |             |
| **CSharpEssentials_Pipeline** | **Both**  |  **3.9609 ns** | **0.0051 ns** | **0.0043 ns** |  **1.00** |         **-** |          **NA** |
| HandWritten_Pipeline      | Both  |  0.4886 ns | 0.0009 ns | 0.0008 ns |  0.12 |         - |          NA |
