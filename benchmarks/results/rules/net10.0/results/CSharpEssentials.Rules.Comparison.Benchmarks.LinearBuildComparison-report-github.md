```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2


```
| Method           | Count | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------- |------ |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **Prebuilt**         | **5**     |  **71.942 ns** | **0.1234 ns** | **0.1154 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| RebuiltStatic    | 5     | 186.231 ns | 0.8250 ns | 0.7717 ns |  2.59 |    0.01 | 0.0215 |     360 B |          NA |
| RebuiltExtension | 5     | 122.964 ns | 1.3647 ns | 1.2098 ns |  1.71 |    0.02 | 0.0076 |     128 B |          NA |
| HandWritten      | 5     |   5.203 ns | 0.0078 ns | 0.0069 ns |  0.07 |    0.00 |      - |         - |          NA |
|                  |       |            |           |           |       |         |        |           |             |
| **Prebuilt**         | **10**    | **157.993 ns** | **1.9728 ns** | **1.7488 ns** |  **1.00** |    **0.02** |      **-** |         **-** |          **NA** |
| RebuiltStatic    | 10    | 380.141 ns | 1.6718 ns | 1.5638 ns |  2.41 |    0.03 | 0.0405 |     680 B |          NA |
| RebuiltExtension | 10    | 267.953 ns | 1.9731 ns | 1.8456 ns |  1.70 |    0.02 | 0.0172 |     288 B |          NA |
| HandWritten      | 10    |   7.753 ns | 0.0082 ns | 0.0077 ns |  0.05 |    0.00 |      - |         - |          NA |
