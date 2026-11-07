```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2


```
| Method                     | Categories | Mean       | Error     | StdDev     | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |----------- |-----------:|----------:|-----------:|------:|--------:|-------:|----------:|------------:|
| CSharpEssentials           | Format     |  15.197 ns | 0.0252 ns |  0.0223 ns |  1.00 |    0.00 |      - |         - |          NA |
| CSharpEssentials_HandleAll | Format     |  17.358 ns | 0.1507 ns |  0.1336 ns |  1.14 |    0.01 |      - |         - |          NA |
| CSharpEssentials_Typed     | Format     |   3.486 ns | 0.0078 ns |  0.0073 ns |  0.23 |    0.00 |      - |         - |          NA |
| Bcl                        | Format     |   5.023 ns | 0.0075 ns |  0.0070 ns |  0.33 |    0.00 |      - |         - |          NA |
| EnumsNet                   | Format     |   5.522 ns | 0.0026 ns |  0.0022 ns |  0.36 |    0.00 |      - |         - |          NA |
|                            |            |            |           |            |       |         |        |           |             |
| CSharpEssentials_Many      | FormatMany | 225.045 ns | 5.6360 ns | 16.5295 ns |  1.01 |    0.10 | 0.0134 |     224 B |        1.00 |
| Bcl_Many                   | FormatMany | 154.828 ns | 0.3941 ns |  0.3494 ns |  0.69 |    0.05 | 0.0134 |     224 B |        1.00 |
| EnumsNet_Many              | FormatMany | 146.268 ns | 0.6656 ns |  0.6226 ns |  0.65 |    0.05 | 0.0134 |     224 B |        1.00 |
