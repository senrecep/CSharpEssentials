```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                      | Categories | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|---------------------------- |----------- |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| CSharpEssentials_Format     | Format     | 25.0906 ns | 0.1094 ns | 0.0970 ns |  1.00 |    0.01 | 0.0043 |      72 B |        1.00 |
| Bcl_Format                  | Format     | 39.2608 ns | 0.1781 ns | 0.1579 ns |  1.56 |    0.01 | 0.0057 |      96 B |        1.33 |
| EnumsNet_Format             | Format     | 82.6982 ns | 0.7393 ns | 0.6915 ns |  3.30 |    0.03 | 0.0167 |     280 B |        3.89 |
| NetEscapades_Format         | Format     | 37.0195 ns | 0.1356 ns | 0.1202 ns |  1.48 |    0.01 | 0.0057 |      96 B |        1.33 |
|                             |            |            |           |           |       |         |        |           |             |
| Bcl_HasFlag                 | HasFlag    |  0.2184 ns | 0.0072 ns | 0.0068 ns |  1.00 |    0.04 |      - |         - |          NA |
| EnumsNet_HasFlag            | HasFlag    |  0.5718 ns | 0.0051 ns | 0.0045 ns |  2.62 |    0.08 |      - |         - |          NA |
| NetEscapades_HasFlag        | HasFlag    |  0.2994 ns | 0.0087 ns | 0.0081 ns |  1.37 |    0.05 |      - |         - |          NA |
|                             |            |            |           |           |       |         |        |           |             |
| CSharpEssentials_ToWireName | WireName   | 58.2851 ns | 0.2335 ns | 0.2184 ns |     ? |       ? | 0.0091 |     152 B |           ? |
