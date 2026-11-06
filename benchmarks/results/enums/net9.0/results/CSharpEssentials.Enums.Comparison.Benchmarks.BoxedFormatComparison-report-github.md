```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                     | Categories | Mean       | Error     | StdDev    | Ratio | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |----------- |-----------:|----------:|----------:|------:|-------:|----------:|------------:|
| CSharpEssentials           | Format     |  27.950 ns | 0.0262 ns | 0.0245 ns |  1.00 |      - |         - |          NA |
| CSharpEssentials_HandleAll | Format     |  29.995 ns | 0.0893 ns | 0.0791 ns |  1.07 |      - |         - |          NA |
| CSharpEssentials_Typed     | Format     |   7.249 ns | 0.0376 ns | 0.0351 ns |  0.26 |      - |         - |          NA |
| Bcl                        | Format     |   5.594 ns | 0.0524 ns | 0.0491 ns |  0.20 |      - |         - |          NA |
| EnumsNet                   | Format     |  15.200 ns | 0.0419 ns | 0.0392 ns |  0.54 |      - |         - |          NA |
|                            |            |            |           |           |       |        |           |             |
| CSharpEssentials_Many      | FormatMany | 615.678 ns | 0.8349 ns | 0.7401 ns |  1.00 | 0.0134 |     224 B |        1.00 |
| Bcl_Many                   | FormatMany | 493.746 ns | 0.7141 ns | 0.6679 ns |  0.80 | 0.0134 |     224 B |        1.00 |
| EnumsNet_Many              | FormatMany | 474.508 ns | 0.6757 ns | 0.5990 ns |  0.77 | 0.0134 |     224 B |        1.00 |
