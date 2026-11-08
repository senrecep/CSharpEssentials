```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2


```
| Method           | Count | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------- |------ |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **Prebuilt**         | **5**     |  **71.929 ns** | **0.1423 ns** | **0.1331 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| RebuiltStatic    | 5     | 183.288 ns | 0.7603 ns | 0.7112 ns |  2.55 |    0.01 | 0.0215 |     360 B |          NA |
| RebuiltExtension | 5     | 119.519 ns | 2.0793 ns | 1.9450 ns |  1.66 |    0.03 | 0.0076 |     128 B |          NA |
| HandWritten      | 5     |   5.200 ns | 0.0070 ns | 0.0062 ns |  0.07 |    0.00 |      - |         - |          NA |
|                  |       |            |           |           |       |         |        |           |             |
| **Prebuilt**         | **10**    | **154.847 ns** | **0.3748 ns** | **0.3505 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| RebuiltStatic    | 10    | 387.168 ns | 4.9755 ns | 4.6541 ns |  2.50 |    0.03 | 0.0405 |     680 B |          NA |
| RebuiltExtension | 10    | 258.988 ns | 2.0108 ns | 1.8809 ns |  1.67 |    0.01 | 0.0172 |     288 B |          NA |
| HandWritten      | 10    |   7.747 ns | 0.0100 ns | 0.0089 ns |  0.05 |    0.00 |      - |         - |          NA |
