```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                      | Categories | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|---------------------------- |----------- |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| CSharpEssentials_Defined    | Defined    |  0.3245 ns | 0.0007 ns | 0.0006 ns |  1.00 |    0.00 |      - |         - |          NA |
| Bcl_Defined                 | Defined    | 10.2689 ns | 0.1036 ns | 0.0919 ns | 31.65 |    0.28 | 0.0014 |      24 B |          NA |
| EnumsNet_Defined            | Defined    |  0.9036 ns | 0.0024 ns | 0.0020 ns |  2.78 |    0.01 |      - |         - |          NA |
| NetEscapades_Defined        | Defined    |  0.6137 ns | 0.0007 ns | 0.0006 ns |  1.89 |    0.00 |      - |         - |          NA |
|                             |            |            |           |           |       |         |        |           |             |
| CSharpEssentials_Undefined  | Undefined  |  2.0573 ns | 0.0011 ns | 0.0010 ns |  1.00 |    0.00 |      - |         - |          NA |
| Bcl_Undefined               | Undefined  | 12.4408 ns | 0.1040 ns | 0.0973 ns |  6.05 |    0.05 | 0.0014 |      24 B |          NA |
| EnumsNet_Undefined          | Undefined  |  2.6341 ns | 0.0006 ns | 0.0005 ns |  1.28 |    0.00 |      - |         - |          NA |
| NetEscapades_Undefined      | Undefined  |  1.7694 ns | 0.0018 ns | 0.0017 ns |  0.86 |    0.00 |      - |         - |          NA |
|                             |            |            |           |           |       |         |        |           |             |
| CSharpEssentials_ToWireName | WireName   |  0.9020 ns | 0.0005 ns | 0.0004 ns |     ? |       ? |      - |         - |           ? |
