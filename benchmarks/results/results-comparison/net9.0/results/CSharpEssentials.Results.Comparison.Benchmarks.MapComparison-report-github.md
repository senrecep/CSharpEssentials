```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                     | Succeed | Mean        | Error     | StdDev    | Ratio  | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |-------- |------------:|----------:|----------:|-------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials**           | **False**   |   **0.9035 ns** | **0.0068 ns** | **0.0064 ns** |   **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions | False   |   8.9394 ns | 0.0067 ns | 0.0056 ns |   9.89 |    0.07 |      - |         - |          NA |
| ErrorOr                    | False   |   0.8891 ns | 0.0115 ns | 0.0102 ns |   0.98 |    0.01 |      - |         - |          NA |
| FluentResults              | False   | 139.0722 ns | 0.5405 ns | 0.5056 ns | 153.93 |    1.18 | 0.0205 |     344 B |          NA |
|                            |         |             |           |           |        |         |        |           |             |
| **CSharpEssentials**           | **True**    |   **0.8478 ns** | **0.0003 ns** | **0.0002 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions | True    |   9.1264 ns | 0.0065 ns | 0.0061 ns |  10.76 |    0.01 |      - |         - |          NA |
| ErrorOr                    | True    |   0.8482 ns | 0.0008 ns | 0.0007 ns |   1.00 |    0.00 |      - |         - |          NA |
| FluentResults              | True    | 105.4259 ns | 0.5389 ns | 0.5041 ns | 124.35 |    0.58 | 0.0153 |     256 B |          NA |
