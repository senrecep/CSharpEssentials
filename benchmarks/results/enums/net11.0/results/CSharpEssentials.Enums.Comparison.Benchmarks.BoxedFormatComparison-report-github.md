```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2


```
| Method                     | Categories | Mean       | Error     | StdDev    | Ratio | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |----------- |-----------:|----------:|----------:|------:|-------:|----------:|------------:|
| CSharpEssentials           | Format     |  14.442 ns | 0.0147 ns | 0.0138 ns |  1.00 |      - |         - |          NA |
| CSharpEssentials_HandleAll | Format     |  15.414 ns | 0.0189 ns | 0.0177 ns |  1.07 |      - |         - |          NA |
| CSharpEssentials_Typed     | Format     |   3.567 ns | 0.0040 ns | 0.0035 ns |  0.25 |      - |         - |          NA |
| Bcl                        | Format     |   5.219 ns | 0.0497 ns | 0.0415 ns |  0.36 |      - |         - |          NA |
| EnumsNet                   | Format     |   5.888 ns | 0.0114 ns | 0.0101 ns |  0.41 |      - |         - |          NA |
|                            |            |            |           |           |       |        |           |             |
| CSharpEssentials_Many      | FormatMany | 189.764 ns | 0.4410 ns | 0.4125 ns |  1.00 | 0.0134 |     224 B |        1.00 |
| Bcl_Many                   | FormatMany | 152.002 ns | 0.8155 ns | 0.7628 ns |  0.80 | 0.0134 |     224 B |        1.00 |
| EnumsNet_Many              | FormatMany | 141.655 ns | 0.4590 ns | 0.4069 ns |  0.75 | 0.0134 |     224 B |        1.00 |
