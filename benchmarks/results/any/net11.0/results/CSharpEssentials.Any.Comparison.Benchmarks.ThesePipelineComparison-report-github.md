```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2

Categories=MapFlatMapMatch  

```
| Method                    | State | Mean      | Error     | StdDev    | Ratio | Allocated | Alloc Ratio |
|-------------------------- |------ |----------:|----------:|----------:|------:|----------:|------------:|
| **CSharpEssentials_Pipeline** | **Left**  | **15.043 ns** | **0.0141 ns** | **0.0125 ns** |  **1.00** |         **-** |          **NA** |
| HandWritten_Pipeline      | Left  |  1.715 ns | 0.0114 ns | 0.0107 ns |  0.11 |         - |          NA |
|                           |       |           |           |           |       |           |             |
| **CSharpEssentials_Pipeline** | **Right** | **25.157 ns** | **0.0019 ns** | **0.0017 ns** |  **1.00** |         **-** |          **NA** |
| HandWritten_Pipeline      | Right |  1.353 ns | 0.0046 ns | 0.0043 ns |  0.05 |         - |          NA |
|                           |       |           |           |           |       |           |             |
| **CSharpEssentials_Pipeline** | **Both**  |  **3.957 ns** | **0.0012 ns** | **0.0011 ns** |  **1.00** |         **-** |          **NA** |
| HandWritten_Pipeline      | Both  |  2.202 ns | 0.0009 ns | 0.0008 ns |  0.56 |         - |          NA |
