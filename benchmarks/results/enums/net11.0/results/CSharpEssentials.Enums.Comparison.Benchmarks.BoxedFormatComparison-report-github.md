```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                     | Categories | Mean       | Error     | StdDev    | Ratio | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |----------- |-----------:|----------:|----------:|------:|-------:|----------:|------------:|
| CSharpEssentials           | Format     |  14.741 ns | 0.0163 ns | 0.0144 ns |  1.00 |      - |         - |          NA |
| CSharpEssentials_HandleAll | Format     |  14.606 ns | 0.0072 ns | 0.0064 ns |  0.99 |      - |         - |          NA |
| CSharpEssentials_Typed     | Format     |   3.394 ns | 0.0016 ns | 0.0015 ns |  0.23 |      - |         - |          NA |
| Bcl                        | Format     |   5.523 ns | 0.0092 ns | 0.0082 ns |  0.37 |      - |         - |          NA |
| EnumsNet                   | Format     |   5.524 ns | 0.0025 ns | 0.0021 ns |  0.37 |      - |         - |          NA |
|                            |            |            |           |           |       |        |           |             |
| CSharpEssentials_Many      | FormatMany | 321.773 ns | 1.9497 ns | 1.8237 ns |  1.00 | 0.0134 |     224 B |        1.00 |
| Bcl_Many                   | FormatMany | 157.496 ns | 1.2599 ns | 1.1785 ns |  0.49 | 0.0134 |     224 B |        1.00 |
| EnumsNet_Many              | FormatMany | 145.819 ns | 0.6705 ns | 0.6271 ns |  0.45 | 0.0134 |     224 B |        1.00 |
