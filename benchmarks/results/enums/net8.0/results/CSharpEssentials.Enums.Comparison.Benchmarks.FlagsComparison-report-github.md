```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                      | Categories | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|---------------------------- |----------- |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| CSharpEssentials_Format     | Format     | 48.2274 ns | 0.2259 ns | 0.2002 ns |  1.00 |    0.01 | 0.0057 |      96 B |        1.00 |
| Bcl_Format                  | Format     | 44.2197 ns | 0.9428 ns | 0.9682 ns |  0.92 |    0.02 | 0.0057 |      96 B |        1.00 |
| EnumsNet_Format             | Format     | 89.9792 ns | 1.2396 ns | 1.1595 ns |  1.87 |    0.02 | 0.0167 |     280 B |        2.92 |
| NetEscapades_Format         | Format     | 37.9471 ns | 0.1976 ns | 0.1849 ns |  0.79 |    0.00 | 0.0057 |      96 B |        1.00 |
|                             |            |            |           |           |       |         |        |           |             |
| Bcl_HasFlag                 | HasFlag    |  0.1632 ns | 0.0023 ns | 0.0020 ns |  1.00 |    0.02 |      - |         - |          NA |
| EnumsNet_HasFlag            | HasFlag    |  0.7586 ns | 0.0050 ns | 0.0044 ns |  4.65 |    0.06 |      - |         - |          NA |
| NetEscapades_HasFlag        | HasFlag    |  0.2353 ns | 0.0044 ns | 0.0037 ns |  1.44 |    0.03 |      - |         - |          NA |
|                             |            |            |           |           |       |         |        |           |             |
| CSharpEssentials_ToWireName | WireName   | 74.1504 ns | 0.2738 ns | 0.2561 ns |     ? |       ? | 0.0091 |     152 B |           ? |
