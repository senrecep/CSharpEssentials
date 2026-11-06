```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                      | Categories | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|---------------------------- |----------- |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| CSharpEssentials_Defined    | Defined    |  0.6127 ns | 0.0006 ns | 0.0005 ns |  1.00 |    0.00 |      - |         - |          NA |
| Bcl_Defined                 | Defined    | 11.5296 ns | 0.0364 ns | 0.0341 ns | 18.82 |    0.06 | 0.0014 |      24 B |          NA |
| EnumsNet_Defined            | Defined    |  0.9037 ns | 0.0030 ns | 0.0027 ns |  1.47 |    0.00 |      - |         - |          NA |
| NetEscapades_Defined        | Defined    |  0.9022 ns | 0.0008 ns | 0.0006 ns |  1.47 |    0.00 |      - |         - |          NA |
|                             |            |            |           |           |       |         |        |           |             |
| CSharpEssentials_Undefined  | Undefined  |  1.7688 ns | 0.0017 ns | 0.0015 ns |  1.00 |    0.00 |      - |         - |          NA |
| Bcl_Undefined               | Undefined  | 14.2128 ns | 0.0440 ns | 0.0412 ns |  8.04 |    0.02 | 0.0014 |      24 B |          NA |
| EnumsNet_Undefined          | Undefined  |  2.9234 ns | 0.0015 ns | 0.0014 ns |  1.65 |    0.00 |      - |         - |          NA |
| NetEscapades_Undefined      | Undefined  |  1.7692 ns | 0.0019 ns | 0.0017 ns |  1.00 |    0.00 |      - |         - |          NA |
|                             |            |            |           |           |       |         |        |           |             |
| CSharpEssentials_ToWireName | WireName   |  0.3252 ns | 0.0020 ns | 0.0016 ns |     ? |       ? |      - |         - |           ? |
