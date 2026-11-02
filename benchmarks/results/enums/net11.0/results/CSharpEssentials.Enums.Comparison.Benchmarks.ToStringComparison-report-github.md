```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                      | Categories | Mean      | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|---------------------------- |----------- |----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| CSharpEssentials_Defined    | Defined    | 0.8345 ns | 0.0049 ns | 0.0043 ns |  1.00 |    0.01 |      - |         - |          NA |
| Bcl_Defined                 | Defined    | 9.9005 ns | 0.0927 ns | 0.0867 ns | 11.86 |    0.12 | 0.0014 |      24 B |          NA |
| EnumsNet_Defined            | Defined    | 1.2649 ns | 0.0035 ns | 0.0031 ns |  1.52 |    0.01 |      - |         - |          NA |
| NetEscapades_Defined        | Defined    | 0.5599 ns | 0.0045 ns | 0.0042 ns |  0.67 |    0.01 |      - |         - |          NA |
|                             |            |           |           |           |       |         |        |           |             |
| CSharpEssentials_Undefined  | Undefined  | 9.8920 ns | 0.0179 ns | 0.0149 ns |  1.00 |    0.00 | 0.0014 |      24 B |        1.00 |
| Bcl_Undefined               | Undefined  | 9.6459 ns | 0.0305 ns | 0.0271 ns |  0.98 |    0.00 | 0.0014 |      24 B |        1.00 |
| EnumsNet_Undefined          | Undefined  | 1.5561 ns | 0.0043 ns | 0.0040 ns |  0.16 |    0.00 |      - |         - |        0.00 |
| NetEscapades_Undefined      | Undefined  | 1.5464 ns | 0.0030 ns | 0.0028 ns |  0.16 |    0.00 |      - |         - |        0.00 |
|                             |            |           |           |           |       |         |        |           |             |
| CSharpEssentials_ToWireName | WireName   | 0.8373 ns | 0.0015 ns | 0.0013 ns |     ? |       ? |      - |         - |           ? |
