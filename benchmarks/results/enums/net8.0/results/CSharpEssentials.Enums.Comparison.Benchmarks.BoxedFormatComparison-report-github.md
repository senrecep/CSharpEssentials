```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                     | Categories | Mean       | Error     | StdDev    | Ratio | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |----------- |-----------:|----------:|----------:|------:|-------:|----------:|------------:|
| CSharpEssentials           | Format     |  30.357 ns | 0.0716 ns | 0.0670 ns |  1.00 |      - |         - |          NA |
| CSharpEssentials_HandleAll | Format     |  31.342 ns | 0.0795 ns | 0.0743 ns |  1.03 |      - |         - |          NA |
| CSharpEssentials_Typed     | Format     |   8.386 ns | 0.0018 ns | 0.0016 ns |  0.28 |      - |         - |          NA |
| Bcl                        | Format     |   8.836 ns | 0.0286 ns | 0.0268 ns |  0.29 |      - |         - |          NA |
| EnumsNet                   | Format     |  14.803 ns | 0.0090 ns | 0.0075 ns |  0.49 |      - |         - |          NA |
|                            |            |            |           |           |       |        |           |             |
| CSharpEssentials_Many      | FormatMany | 721.441 ns | 0.4286 ns | 0.3799 ns |  1.00 | 0.0134 |     224 B |        1.00 |
| Bcl_Many                   | FormatMany | 574.047 ns | 0.5275 ns | 0.4405 ns |  0.80 | 0.0134 |     224 B |        1.00 |
| EnumsNet_Many              | FormatMany | 520.411 ns | 0.5103 ns | 0.4523 ns |  0.72 | 0.0134 |     224 B |        1.00 |
