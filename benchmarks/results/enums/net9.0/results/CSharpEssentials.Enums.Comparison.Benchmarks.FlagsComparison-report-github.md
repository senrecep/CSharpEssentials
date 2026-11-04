```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                      | Categories | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|---------------------------- |----------- |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| CSharpEssentials_Format     | Format     | 40.5164 ns | 0.4793 ns | 0.4483 ns |  1.00 |    0.02 | 0.0057 |      96 B |        1.00 |
| Bcl_Format                  | Format     | 40.7572 ns | 0.8072 ns | 0.7550 ns |  1.01 |    0.02 | 0.0057 |      96 B |        1.00 |
| EnumsNet_Format             | Format     | 86.9533 ns | 0.3909 ns | 0.3656 ns |  2.15 |    0.02 | 0.0167 |     280 B |        2.92 |
| NetEscapades_Format         | Format     | 41.1450 ns | 0.4880 ns | 0.4565 ns |  1.02 |    0.02 | 0.0057 |      96 B |        1.00 |
|                             |            |            |           |           |       |         |        |           |             |
| Bcl_HasFlag                 | HasFlag    |  0.1782 ns | 0.0060 ns | 0.0056 ns |  1.00 |    0.04 |      - |         - |          NA |
| EnumsNet_HasFlag            | HasFlag    |  0.5855 ns | 0.0086 ns | 0.0077 ns |  3.29 |    0.11 |      - |         - |          NA |
| NetEscapades_HasFlag        | HasFlag    |  0.2425 ns | 0.0137 ns | 0.0128 ns |  1.36 |    0.08 |      - |         - |          NA |
|                             |            |            |           |           |       |         |        |           |             |
| CSharpEssentials_ToWireName | WireName   | 62.7575 ns | 0.4242 ns | 0.3968 ns |     ? |       ? | 0.0091 |     152 B |           ? |
