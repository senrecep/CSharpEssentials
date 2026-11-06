```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method           | Count | Mean       | Error     | StdDev    | Ratio | Gen0   | Allocated | Alloc Ratio |
|----------------- |------ |-----------:|----------:|----------:|------:|-------:|----------:|------------:|
| **Prebuilt**         | **5**     | **111.082 ns** | **0.0830 ns** | **0.0776 ns** |  **1.00** |      **-** |         **-** |          **NA** |
| RebuiltStatic    | 5     | 239.537 ns | 0.6252 ns | 0.5848 ns |  2.16 | 0.0215 |     360 B |          NA |
| RebuiltExtension | 5     | 134.668 ns | 0.3135 ns | 0.2932 ns |  1.21 | 0.0076 |     128 B |          NA |
| HandWritten      | 5     |   4.970 ns | 0.0045 ns | 0.0040 ns |  0.04 |      - |         - |          NA |
|                  |       |            |           |           |       |        |           |             |
| **Prebuilt**         | **10**    | **172.648 ns** | **0.0510 ns** | **0.0426 ns** |  **1.00** |      **-** |         **-** |          **NA** |
| RebuiltStatic    | 10    | 435.532 ns | 0.6159 ns | 0.5460 ns |  2.52 | 0.0405 |     680 B |          NA |
| RebuiltExtension | 10    | 296.987 ns | 0.4448 ns | 0.3714 ns |  1.72 | 0.0172 |     288 B |          NA |
| HandWritten      | 10    |   8.443 ns | 0.0200 ns | 0.0187 ns |  0.05 |      - |         - |          NA |
