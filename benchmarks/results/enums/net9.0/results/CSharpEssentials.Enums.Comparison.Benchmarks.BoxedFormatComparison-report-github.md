```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2


```
| Method                     | Categories | Mean       | Error     | StdDev    | Ratio | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |----------- |-----------:|----------:|----------:|------:|-------:|----------:|------------:|
| CSharpEssentials           | Format     |  27.535 ns | 0.0160 ns | 0.0142 ns |  1.00 |      - |         - |          NA |
| CSharpEssentials_HandleAll | Format     |  29.437 ns | 0.0300 ns | 0.0266 ns |  1.07 |      - |         - |          NA |
| CSharpEssentials_Typed     | Format     |   7.145 ns | 0.0031 ns | 0.0027 ns |  0.26 |      - |         - |          NA |
| Bcl                        | Format     |   7.255 ns | 0.0014 ns | 0.0011 ns |  0.26 |      - |         - |          NA |
| EnumsNet                   | Format     |  15.077 ns | 0.0187 ns | 0.0175 ns |  0.55 |      - |         - |          NA |
|                            |            |            |           |           |       |        |           |             |
| CSharpEssentials_Many      | FormatMany | 498.842 ns | 1.6760 ns | 1.5677 ns |  1.00 | 0.0134 |     224 B |        1.00 |
| Bcl_Many                   | FormatMany | 486.657 ns | 1.0459 ns | 0.9783 ns |  0.98 | 0.0134 |     224 B |        1.00 |
| EnumsNet_Many              | FormatMany | 469.709 ns | 1.1416 ns | 1.0120 ns |  0.94 | 0.0134 |     224 B |        1.00 |
