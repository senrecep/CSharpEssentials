```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2


```
| Method                     | Categories | Mean       | Error     | StdDev    | Ratio | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |----------- |-----------:|----------:|----------:|------:|-------:|----------:|------------:|
| CSharpEssentials           | Format     |  15.566 ns | 0.0403 ns | 0.0377 ns |  1.00 |      - |         - |          NA |
| CSharpEssentials_HandleAll | Format     |  16.868 ns | 0.0371 ns | 0.0347 ns |  1.08 |      - |         - |          NA |
| CSharpEssentials_Typed     | Format     |   3.247 ns | 0.0055 ns | 0.0049 ns |  0.21 |      - |         - |          NA |
| Bcl                        | Format     |   5.052 ns | 0.0124 ns | 0.0116 ns |  0.32 |      - |         - |          NA |
| EnumsNet                   | Format     |   5.543 ns | 0.0175 ns | 0.0164 ns |  0.36 |      - |         - |          NA |
|                            |            |            |           |           |       |        |           |             |
| CSharpEssentials_Many      | FormatMany | 190.594 ns | 0.8044 ns | 0.7524 ns |  1.00 | 0.0134 |     224 B |        1.00 |
| Bcl_Many                   | FormatMany | 157.100 ns | 0.5762 ns | 0.5390 ns |  0.82 | 0.0134 |     224 B |        1.00 |
| EnumsNet_Many              | FormatMany | 149.443 ns | 0.7262 ns | 0.6793 ns |  0.78 | 0.0134 |     224 B |        1.00 |
