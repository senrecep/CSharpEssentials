```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2


```
| Method                     | Categories | Mean       | Error     | StdDev    | Ratio | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |----------- |-----------:|----------:|----------:|------:|-------:|----------:|------------:|
| CSharpEssentials           | Format     |  30.214 ns | 0.0388 ns | 0.0344 ns |  1.00 |      - |         - |          NA |
| CSharpEssentials_HandleAll | Format     |  30.846 ns | 0.0982 ns | 0.0918 ns |  1.02 |      - |         - |          NA |
| CSharpEssentials_Typed     | Format     |   8.229 ns | 0.0022 ns | 0.0018 ns |  0.27 |      - |         - |          NA |
| Bcl                        | Format     |   6.316 ns | 0.0033 ns | 0.0031 ns |  0.21 |      - |         - |          NA |
| EnumsNet                   | Format     |  14.789 ns | 0.0090 ns | 0.0075 ns |  0.49 |      - |         - |          NA |
|                            |            |            |           |           |       |        |           |             |
| CSharpEssentials_Many      | FormatMany | 634.760 ns | 1.2632 ns | 1.1816 ns |  1.00 | 0.0134 |     224 B |        1.00 |
| Bcl_Many                   | FormatMany | 539.468 ns | 5.2464 ns | 4.9075 ns |  0.85 | 0.0134 |     224 B |        1.00 |
| EnumsNet_Many              | FormatMany | 529.243 ns | 1.9199 ns | 1.7958 ns |  0.83 | 0.0134 |     224 B |        1.00 |
