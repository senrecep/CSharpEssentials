```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2


```
| Method           | Count | Mean       | Error     | StdDev    | Ratio | Gen0   | Allocated | Alloc Ratio |
|----------------- |------ |-----------:|----------:|----------:|------:|-------:|----------:|------------:|
| **Prebuilt**         | **5**     | **100.842 ns** | **0.2167 ns** | **0.2027 ns** |  **1.00** |      **-** |         **-** |          **NA** |
| RebuiltStatic    | 5     | 241.907 ns | 1.4674 ns | 1.3726 ns |  2.40 | 0.0215 |     360 B |          NA |
| RebuiltExtension | 5     | 192.274 ns | 0.6769 ns | 0.6332 ns |  1.91 | 0.0076 |     128 B |          NA |
| HandWritten      | 5     |   5.242 ns | 0.0072 ns | 0.0063 ns |  0.05 |      - |         - |          NA |
|                  |       |            |           |           |       |        |           |             |
| **Prebuilt**         | **10**    | **218.557 ns** | **0.2861 ns** | **0.2389 ns** |  **1.00** |      **-** |         **-** |          **NA** |
| RebuiltStatic    | 10    | 484.639 ns | 2.0845 ns | 1.9499 ns |  2.22 | 0.0401 |     680 B |          NA |
| RebuiltExtension | 10    | 334.246 ns | 2.1211 ns | 1.7712 ns |  1.53 | 0.0172 |     288 B |          NA |
| HandWritten      | 10    |  12.814 ns | 0.0164 ns | 0.0153 ns |  0.06 |      - |         - |          NA |
