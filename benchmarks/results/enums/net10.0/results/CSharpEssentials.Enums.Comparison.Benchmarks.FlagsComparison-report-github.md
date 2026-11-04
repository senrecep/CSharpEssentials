```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                      | Categories | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|---------------------------- |----------- |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| CSharpEssentials_Format     | Format     | 36.1870 ns | 0.6990 ns | 0.6538 ns |  1.00 |    0.02 | 0.0057 |      96 B |        1.00 |
| Bcl_Format                  | Format     | 34.8196 ns | 0.1248 ns | 0.0975 ns |  0.96 |    0.02 | 0.0057 |      96 B |        1.00 |
| EnumsNet_Format             | Format     | 84.5527 ns | 0.4612 ns | 0.4089 ns |  2.34 |    0.04 | 0.0167 |     280 B |        2.92 |
| NetEscapades_Format         | Format     | 39.8499 ns | 0.2198 ns | 0.2056 ns |  1.10 |    0.02 | 0.0057 |      96 B |        1.00 |
|                             |            |            |           |           |       |         |        |           |             |
| Bcl_HasFlag                 | HasFlag    |  0.1638 ns | 0.0145 ns | 0.0135 ns |  1.01 |    0.12 |      - |         - |          NA |
| EnumsNet_HasFlag            | HasFlag    |  0.5823 ns | 0.0165 ns | 0.0146 ns |  3.58 |    0.32 |      - |         - |          NA |
| NetEscapades_HasFlag        | HasFlag    |  0.1761 ns | 0.0188 ns | 0.0157 ns |  1.08 |    0.13 |      - |         - |          NA |
|                             |            |            |           |           |       |         |        |           |             |
| CSharpEssentials_ToWireName | WireName   | 56.6902 ns | 0.2961 ns | 0.2770 ns |     ? |       ? | 0.0091 |     152 B |           ? |
