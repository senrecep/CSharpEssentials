```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                     | Categories | Mean       | Error     | StdDev    | Ratio | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |----------- |-----------:|----------:|----------:|------:|-------:|----------:|------------:|
| CSharpEssentials           | Format     |  15.084 ns | 0.0168 ns | 0.0140 ns |  1.00 |      - |         - |          NA |
| CSharpEssentials_HandleAll | Format     |  15.155 ns | 0.0206 ns | 0.0182 ns |  1.00 |      - |         - |          NA |
| CSharpEssentials_Typed     | Format     |   3.344 ns | 0.0010 ns | 0.0009 ns |  0.22 |      - |         - |          NA |
| Bcl                        | Format     |   5.552 ns | 0.0152 ns | 0.0142 ns |  0.37 |      - |         - |          NA |
| EnumsNet                   | Format     |   5.168 ns | 0.0101 ns | 0.0094 ns |  0.34 |      - |         - |          NA |
|                            |            |            |           |           |       |        |           |             |
| CSharpEssentials_Many      | FormatMany | 284.671 ns | 1.1061 ns | 0.9236 ns |  1.00 | 0.0134 |     224 B |        1.00 |
| Bcl_Many                   | FormatMany | 157.674 ns | 0.6674 ns | 0.5573 ns |  0.55 | 0.0134 |     224 B |        1.00 |
| EnumsNet_Many              | FormatMany | 151.202 ns | 0.4071 ns | 0.3808 ns |  0.53 | 0.0134 |     224 B |        1.00 |
