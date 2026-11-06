```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2

Categories=Match  

```
| Method                 | State | Mean      | Error     | StdDev    | Ratio | Allocated | Alloc Ratio |
|----------------------- |------ |----------:|----------:|----------:|------:|----------:|------------:|
| **CSharpEssentials_Match** | **Left**  | **2.1713 ns** | **0.0029 ns** | **0.0027 ns** |  **1.00** |         **-** |          **NA** |
| HandWritten_Match      | Left  | 0.5869 ns | 0.0049 ns | 0.0046 ns |  0.27 |         - |          NA |
|                        |       |           |           |           |       |           |             |
| **CSharpEssentials_Match** | **Right** | **1.9989 ns** | **0.0044 ns** | **0.0039 ns** |  **1.00** |         **-** |          **NA** |
| HandWritten_Match      | Right | 0.5826 ns | 0.0002 ns | 0.0002 ns |  0.29 |         - |          NA |
|                        |       |           |           |           |       |           |             |
| **CSharpEssentials_Match** | **Both**  | **1.9973 ns** | **0.0033 ns** | **0.0031 ns** |  **1.00** |         **-** |          **NA** |
| HandWritten_Match      | Both  | 0.8701 ns | 0.0002 ns | 0.0002 ns |  0.44 |         - |          NA |
