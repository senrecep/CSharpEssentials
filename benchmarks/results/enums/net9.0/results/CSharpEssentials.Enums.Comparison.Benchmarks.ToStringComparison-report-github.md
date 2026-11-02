```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                      | Categories | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|---------------------------- |----------- |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| CSharpEssentials_Defined    | Defined    |  0.9135 ns | 0.0011 ns | 0.0009 ns |  1.00 |    0.00 |      - |         - |          NA |
| Bcl_Defined                 | Defined    | 10.2164 ns | 0.0528 ns | 0.0468 ns | 11.18 |    0.05 | 0.0014 |      24 B |          NA |
| EnumsNet_Defined            | Defined    |  0.9925 ns | 0.0023 ns | 0.0020 ns |  1.09 |    0.00 |      - |         - |          NA |
| NetEscapades_Defined        | Defined    |  0.6273 ns | 0.0031 ns | 0.0029 ns |  0.69 |    0.00 |      - |         - |          NA |
|                             |            |            |           |           |       |         |        |           |             |
| CSharpEssentials_Undefined  | Undefined  | 14.7163 ns | 0.2340 ns | 0.2189 ns |  1.00 |    0.02 | 0.0014 |      24 B |        1.00 |
| Bcl_Undefined               | Undefined  | 12.1915 ns | 0.0642 ns | 0.0601 ns |  0.83 |    0.01 | 0.0014 |      24 B |        1.00 |
| EnumsNet_Undefined          | Undefined  |  2.6346 ns | 0.0011 ns | 0.0009 ns |  0.18 |    0.00 |      - |         - |        0.00 |
| NetEscapades_Undefined      | Undefined  |  1.8384 ns | 0.0065 ns | 0.0061 ns |  0.12 |    0.00 |      - |         - |        0.00 |
|                             |            |            |           |           |       |         |        |           |             |
| CSharpEssentials_ToWireName | WireName   |  0.6114 ns | 0.0009 ns | 0.0007 ns |     ? |       ? |      - |         - |           ? |
