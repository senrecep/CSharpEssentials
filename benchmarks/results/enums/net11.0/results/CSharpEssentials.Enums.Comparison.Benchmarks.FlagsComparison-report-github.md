```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                      | Categories | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|---------------------------- |----------- |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| CSharpEssentials_Format     | Format     | 44.6887 ns | 0.9326 ns | 1.1453 ns |  1.00 |    0.04 | 0.0057 |      96 B |        1.00 |
| Bcl_Format                  | Format     | 44.9506 ns | 0.7975 ns | 0.7460 ns |  1.01 |    0.03 | 0.0057 |      96 B |        1.00 |
| EnumsNet_Format             | Format     | 83.5635 ns | 0.9196 ns | 0.7679 ns |  1.87 |    0.05 | 0.0167 |     280 B |        2.92 |
| NetEscapades_Format         | Format     | 44.6293 ns | 0.8376 ns | 0.7835 ns |  1.00 |    0.03 | 0.0057 |      96 B |        1.00 |
|                             |            |            |           |           |       |         |        |           |             |
| Bcl_HasFlag                 | HasFlag    |  0.1713 ns | 0.0052 ns | 0.0048 ns |  1.00 |    0.04 |      - |         - |          NA |
| EnumsNet_HasFlag            | HasFlag    |  0.5766 ns | 0.0049 ns | 0.0043 ns |  3.37 |    0.09 |      - |         - |          NA |
| NetEscapades_HasFlag        | HasFlag    |  0.2318 ns | 0.0056 ns | 0.0050 ns |  1.35 |    0.05 |      - |         - |          NA |
|                             |            |            |           |           |       |         |        |           |             |
| CSharpEssentials_ToWireName | WireName   | 61.0658 ns | 0.3034 ns | 0.2368 ns |     ? |       ? | 0.0091 |     152 B |           ? |
