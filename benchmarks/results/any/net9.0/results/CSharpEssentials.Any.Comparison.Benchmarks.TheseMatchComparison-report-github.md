```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2

Categories=Match  

```
| Method                 | State | Mean      | Error     | StdDev    | Ratio | Allocated | Alloc Ratio |
|----------------------- |------ |----------:|----------:|----------:|------:|----------:|------------:|
| **CSharpEssentials_Match** | **Left**  | **1.7596 ns** | **0.0035 ns** | **0.0033 ns** |  **1.00** |         **-** |          **NA** |
| HandWritten_Match      | Left  | 0.8706 ns | 0.0004 ns | 0.0004 ns |  0.49 |         - |          NA |
|                        |       |           |           |           |       |           |             |
| **CSharpEssentials_Match** | **Right** | **1.5718 ns** | **0.0095 ns** | **0.0084 ns** |  **1.00** |         **-** |          **NA** |
| HandWritten_Match      | Right | 0.5822 ns | 0.0001 ns | 0.0001 ns |  0.37 |         - |          NA |
|                        |       |           |           |           |       |           |             |
| **CSharpEssentials_Match** | **Both**  | **1.9242 ns** | **0.0114 ns** | **0.0106 ns** |  **1.00** |         **-** |          **NA** |
| HandWritten_Match      | Both  | 0.6577 ns | 0.0003 ns | 0.0002 ns |  0.34 |         - |          NA |
