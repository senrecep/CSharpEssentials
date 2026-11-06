```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2

Categories=MapFlatMapMatch  

```
| Method                    | State | Mean       | Error     | StdDev    | Ratio | Allocated | Alloc Ratio |
|-------------------------- |------ |-----------:|----------:|----------:|------:|----------:|------------:|
| **CSharpEssentials_Pipeline** | **Left**  | **12.7056 ns** | **0.0029 ns** | **0.0027 ns** |  **1.00** |         **-** |          **NA** |
| HandWritten_Pipeline      | Left  |  1.0679 ns | 0.0007 ns | 0.0006 ns |  0.08 |         - |          NA |
|                           |       |            |           |           |       |           |             |
| **CSharpEssentials_Pipeline** | **Right** | **25.6079 ns** | **0.0060 ns** | **0.0053 ns** |  **1.00** |         **-** |          **NA** |
| HandWritten_Pipeline      | Right |  1.6429 ns | 0.0011 ns | 0.0011 ns |  0.06 |         - |          NA |
|                           |       |            |           |           |       |           |             |
| **CSharpEssentials_Pipeline** | **Both**  | **15.1796 ns** | **0.0317 ns** | **0.0281 ns** |  **1.00** |         **-** |          **NA** |
| HandWritten_Pipeline      | Both  |  0.7583 ns | 0.0007 ns | 0.0006 ns |  0.05 |         - |          NA |
