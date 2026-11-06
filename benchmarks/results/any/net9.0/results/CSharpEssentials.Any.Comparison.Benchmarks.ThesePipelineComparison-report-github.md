```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2

Categories=MapFlatMapMatch  

```
| Method                    | State | Mean      | Error     | StdDev    | Ratio | Allocated | Alloc Ratio |
|-------------------------- |------ |----------:|----------:|----------:|------:|----------:|------------:|
| **CSharpEssentials_Pipeline** | **Left**  | **15.408 ns** | **0.0236 ns** | **0.0197 ns** |  **1.00** |         **-** |          **NA** |
| HandWritten_Pipeline      | Left  |  1.720 ns | 0.0143 ns | 0.0127 ns |  0.11 |         - |          NA |
|                           |       |           |           |           |       |           |             |
| **CSharpEssentials_Pipeline** | **Right** | **25.494 ns** | **0.0137 ns** | **0.0128 ns** |  **1.00** |         **-** |          **NA** |
| HandWritten_Pipeline      | Right |  1.526 ns | 0.0037 ns | 0.0031 ns |  0.06 |         - |          NA |
|                           |       |           |           |           |       |           |             |
| **CSharpEssentials_Pipeline** | **Both**  |  **3.957 ns** | **0.0008 ns** | **0.0007 ns** |  **1.00** |         **-** |          **NA** |
| HandWritten_Pipeline      | Both  |  2.202 ns | 0.0014 ns | 0.0012 ns |  0.56 |         - |          NA |
