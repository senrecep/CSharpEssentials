```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                      | Categories | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|---------------------------- |----------- |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| CSharpEssentials_Format     | Format     | 22.8844 ns | 0.1833 ns | 0.1715 ns |  1.00 |    0.01 | 0.0043 |      72 B |        1.00 |
| Bcl_Format                  | Format     | 42.3831 ns | 0.4417 ns | 0.4132 ns |  1.85 |    0.02 | 0.0057 |      96 B |        1.33 |
| EnumsNet_Format             | Format     | 95.0625 ns | 0.5764 ns | 0.5392 ns |  4.15 |    0.04 | 0.0167 |     280 B |        3.89 |
| NetEscapades_Format         | Format     | 43.0297 ns | 0.7345 ns | 0.6871 ns |  1.88 |    0.03 | 0.0057 |      96 B |        1.33 |
|                             |            |            |           |           |       |         |        |           |             |
| Bcl_HasFlag                 | HasFlag    |  0.1666 ns | 0.0083 ns | 0.0078 ns |  1.00 |    0.06 |      - |         - |          NA |
| EnumsNet_HasFlag            | HasFlag    |  0.5817 ns | 0.0059 ns | 0.0055 ns |  3.50 |    0.16 |      - |         - |          NA |
| NetEscapades_HasFlag        | HasFlag    |  0.2170 ns | 0.0052 ns | 0.0049 ns |  1.31 |    0.06 |      - |         - |          NA |
|                             |            |            |           |           |       |         |        |           |             |
| CSharpEssentials_ToWireName | WireName   | 64.3528 ns | 0.1748 ns | 0.1635 ns |     ? |       ? | 0.0091 |     152 B |           ? |
