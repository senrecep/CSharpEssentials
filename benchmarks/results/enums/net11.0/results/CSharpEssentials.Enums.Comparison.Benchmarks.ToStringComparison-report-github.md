```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                      | Categories | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|---------------------------- |----------- |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| CSharpEssentials_Defined    | Defined    |  0.6133 ns | 0.0004 ns | 0.0003 ns |  1.00 |    0.00 |      - |         - |          NA |
| Bcl_Defined                 | Defined    | 10.1011 ns | 0.0396 ns | 0.0351 ns | 16.47 |    0.06 | 0.0014 |      24 B |          NA |
| EnumsNet_Defined            | Defined    |  1.1906 ns | 0.0003 ns | 0.0003 ns |  1.94 |    0.00 |      - |         - |          NA |
| NetEscapades_Defined        | Defined    |  0.6135 ns | 0.0008 ns | 0.0007 ns |  1.00 |    0.00 |      - |         - |          NA |
|                             |            |            |           |           |       |         |        |           |             |
| CSharpEssentials_Undefined  | Undefined  |  1.4798 ns | 0.0009 ns | 0.0008 ns |  1.00 |    0.00 |      - |         - |          NA |
| Bcl_Undefined               | Undefined  |  9.8266 ns | 0.0451 ns | 0.0421 ns |  6.64 |    0.03 | 0.0014 |      24 B |          NA |
| EnumsNet_Undefined          | Undefined  |  1.1915 ns | 0.0014 ns | 0.0011 ns |  0.81 |    0.00 |      - |         - |          NA |
| NetEscapades_Undefined      | Undefined  |  1.6093 ns | 0.0225 ns | 0.0210 ns |  1.09 |    0.01 |      - |         - |          NA |
|                             |            |            |           |           |       |         |        |           |             |
| CSharpEssentials_ToWireName | WireName   |  0.6143 ns | 0.0013 ns | 0.0012 ns |     ? |       ? |      - |         - |           ? |
