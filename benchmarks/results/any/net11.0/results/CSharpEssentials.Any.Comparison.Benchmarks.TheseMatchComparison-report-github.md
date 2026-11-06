```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2

Categories=Match  

```
| Method                 | State | Mean      | Error     | StdDev    | Ratio | Allocated | Alloc Ratio |
|----------------------- |------ |----------:|----------:|----------:|------:|----------:|------------:|
| **CSharpEssentials_Match** | **Left**  | **1.9444 ns** | **0.0083 ns** | **0.0074 ns** |  **1.00** |         **-** |          **NA** |
| HandWritten_Match      | Left  | 0.5983 ns | 0.0077 ns | 0.0072 ns |  0.31 |         - |          NA |
|                        |       |           |           |           |       |           |             |
| **CSharpEssentials_Match** | **Right** | **1.7477 ns** | **0.0056 ns** | **0.0049 ns** |  **1.00** |         **-** |          **NA** |
| HandWritten_Match      | Right | 0.5911 ns | 0.0081 ns | 0.0076 ns |  0.34 |         - |          NA |
|                        |       |           |           |           |       |           |             |
| **CSharpEssentials_Match** | **Both**  | **1.5164 ns** | **0.0122 ns** | **0.0115 ns** |  **1.00** |         **-** |          **NA** |
| HandWritten_Match      | Both  | 0.6013 ns | 0.0075 ns | 0.0070 ns |  0.40 |         - |          NA |
