```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                      | Categories | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|---------------------------- |----------- |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| CSharpEssentials_Defined    | Defined    |  0.5580 ns | 0.0223 ns | 0.0198 ns |  1.00 |    0.05 |      - |         - |          NA |
| Bcl_Defined                 | Defined    | 11.5723 ns | 0.1534 ns | 0.1435 ns | 20.76 |    0.73 | 0.0014 |      24 B |          NA |
| EnumsNet_Defined            | Defined    |  0.8226 ns | 0.0014 ns | 0.0012 ns |  1.48 |    0.05 |      - |         - |          NA |
| NetEscapades_Defined        | Defined    |  0.5428 ns | 0.0047 ns | 0.0039 ns |  0.97 |    0.03 |      - |         - |          NA |
|                             |            |            |           |           |       |         |        |           |             |
| CSharpEssentials_Undefined  | Undefined  | 14.2804 ns | 0.2402 ns | 0.2247 ns |  1.00 |    0.02 | 0.0014 |      24 B |        1.00 |
| Bcl_Undefined               | Undefined  | 16.1811 ns | 0.0689 ns | 0.0575 ns |  1.13 |    0.02 | 0.0014 |      24 B |        1.00 |
| EnumsNet_Undefined          | Undefined  |  2.5721 ns | 0.0154 ns | 0.0144 ns |  0.18 |    0.00 |      - |         - |        0.00 |
| NetEscapades_Undefined      | Undefined  |  1.9885 ns | 0.0090 ns | 0.0080 ns |  0.14 |    0.00 |      - |         - |        0.00 |
|                             |            |            |           |           |       |         |        |           |             |
| CSharpEssentials_ToWireName | WireName   |  0.2522 ns | 0.0035 ns | 0.0031 ns |     ? |       ? |      - |         - |           ? |
