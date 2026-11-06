```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2

Categories=Match  

```
| Method                 | State | Mean      | Error     | StdDev    | Ratio | Allocated | Alloc Ratio |
|----------------------- |------ |----------:|----------:|----------:|------:|----------:|------------:|
| **CSharpEssentials_Match** | **Left**  | **2.3879 ns** | **0.0024 ns** | **0.0021 ns** |  **1.00** |         **-** |          **NA** |
| HandWritten_Match      | Left  | 0.8710 ns | 0.0006 ns | 0.0005 ns |  0.36 |         - |          NA |
|                        |       |           |           |           |       |           |             |
| **CSharpEssentials_Match** | **Right** | **2.3364 ns** | **0.0007 ns** | **0.0006 ns** |  **1.00** |         **-** |          **NA** |
| HandWritten_Match      | Right | 0.8702 ns | 0.0001 ns | 0.0001 ns |  0.37 |         - |          NA |
|                        |       |           |           |           |       |           |             |
| **CSharpEssentials_Match** | **Both**  | **2.6370 ns** | **0.0021 ns** | **0.0019 ns** |  **1.00** |         **-** |          **NA** |
| HandWritten_Match      | Both  | 0.7757 ns | 0.0002 ns | 0.0001 ns |  0.29 |         - |          NA |
