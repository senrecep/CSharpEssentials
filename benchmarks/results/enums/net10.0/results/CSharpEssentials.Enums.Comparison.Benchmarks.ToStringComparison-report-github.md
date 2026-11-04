```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                      | Categories | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|---------------------------- |----------- |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| CSharpEssentials_Defined    | Defined    |  0.5439 ns | 0.0031 ns | 0.0028 ns |  1.00 |    0.01 |      - |         - |          NA |
| Bcl_Defined                 | Defined    |  8.6687 ns | 0.0652 ns | 0.0578 ns | 15.94 |    0.13 | 0.0014 |      24 B |          NA |
| EnumsNet_Defined            | Defined    |  1.1192 ns | 0.0016 ns | 0.0013 ns |  2.06 |    0.01 |      - |         - |          NA |
| NetEscapades_Defined        | Defined    |  0.5437 ns | 0.0039 ns | 0.0036 ns |  1.00 |    0.01 |      - |         - |          NA |
|                             |            |            |           |           |       |         |        |           |             |
| CSharpEssentials_Undefined  | Undefined  | 12.8220 ns | 0.1146 ns | 0.1072 ns |  1.00 |    0.01 | 0.0014 |      24 B |        1.00 |
| Bcl_Undefined               | Undefined  | 11.8073 ns | 0.0705 ns | 0.0659 ns |  0.92 |    0.01 | 0.0014 |      24 B |        1.00 |
| EnumsNet_Undefined          | Undefined  |  2.0876 ns | 0.0037 ns | 0.0031 ns |  0.16 |    0.00 |      - |         - |        0.00 |
| NetEscapades_Undefined      | Undefined  |  1.6986 ns | 0.0045 ns | 0.0042 ns |  0.13 |    0.00 |      - |         - |        0.00 |
|                             |            |            |           |           |       |         |        |           |             |
| CSharpEssentials_ToWireName | WireName   |  0.5423 ns | 0.0015 ns | 0.0014 ns |     ? |       ? |      - |         - |           ? |
