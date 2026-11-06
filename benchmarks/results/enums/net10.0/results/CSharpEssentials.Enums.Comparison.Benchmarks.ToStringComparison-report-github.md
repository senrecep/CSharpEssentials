```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                      | Categories | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|---------------------------- |----------- |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| CSharpEssentials_Defined    | Defined    |  0.5418 ns | 0.0009 ns | 0.0009 ns |  1.00 |    0.00 |      - |         - |          NA |
| Bcl_Defined                 | Defined    |  9.0266 ns | 0.1127 ns | 0.1054 ns | 16.66 |    0.19 | 0.0014 |      24 B |          NA |
| EnumsNet_Defined            | Defined    |  1.1186 ns | 0.0003 ns | 0.0003 ns |  2.06 |    0.00 |      - |         - |          NA |
| NetEscapades_Defined        | Defined    |  0.5414 ns | 0.0008 ns | 0.0007 ns |  1.00 |    0.00 |      - |         - |          NA |
|                             |            |            |           |           |       |         |        |           |             |
| CSharpEssentials_Undefined  | Undefined  |  3.3626 ns | 0.0420 ns | 0.0393 ns |  1.00 |    0.02 |      - |         - |          NA |
| Bcl_Undefined               | Undefined  | 12.4800 ns | 0.1810 ns | 0.1693 ns |  3.71 |    0.06 | 0.0014 |      24 B |          NA |
| EnumsNet_Undefined          | Undefined  |  1.9853 ns | 0.0013 ns | 0.0011 ns |  0.59 |    0.01 |      - |         - |          NA |
| NetEscapades_Undefined      | Undefined  |  1.7021 ns | 0.0072 ns | 0.0067 ns |  0.51 |    0.01 |      - |         - |          NA |
|                             |            |            |           |           |       |         |        |           |             |
| CSharpEssentials_ToWireName | WireName   |  0.5418 ns | 0.0015 ns | 0.0013 ns |     ? |       ? |      - |         - |           ? |
