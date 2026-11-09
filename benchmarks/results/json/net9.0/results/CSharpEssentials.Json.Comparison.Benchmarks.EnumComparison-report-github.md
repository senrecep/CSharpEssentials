```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2


```
| Method                           | Categories      | Mean         | Error     | StdDev    | Ratio | Gen0   | Allocated | Alloc Ratio |
|--------------------------------- |---------------- |-------------:|----------:|----------:|------:|-------:|----------:|------------:|
| CSharpEssentials_InvalidFallback | InvalidFallback |     97.89 ns |  0.209 ns |  0.195 ns |  1.00 |      - |         - |          NA |
| Macross_InvalidFallback          | InvalidFallback |    105.01 ns |  0.224 ns |  0.187 ns |  1.07 | 0.0019 |      32 B |          NA |
|                                  |                 |              |           |           |       |        |           |             |
| CSharpEssentials_InvalidThrow    | InvalidThrow    |  9,893.63 ns | 26.687 ns | 24.963 ns |  1.00 | 0.0610 |    1208 B |        1.00 |
| Bcl_InvalidThrow                 | InvalidThrow    | 10,493.41 ns | 25.576 ns | 23.923 ns |  1.06 | 0.0916 |    1648 B |        1.36 |
| Macross_InvalidThrow             | InvalidThrow    |  9,700.32 ns | 27.789 ns | 25.994 ns |  0.98 | 0.0763 |    1432 B |        1.19 |
|                                  |                 |              |           |           |       |        |           |             |
| CSharpEssentials_Read            | Read            |  1,066.00 ns |  3.638 ns |  3.403 ns |  1.00 | 0.0172 |     304 B |        1.00 |
| Bcl_Read                         | Read            |  1,012.47 ns |  2.911 ns |  2.581 ns |  0.95 | 0.0172 |     304 B |        1.00 |
| Macross_Read                     | Read            |  1,120.96 ns |  6.400 ns |  5.987 ns |  1.05 | 0.0572 |     968 B |        3.18 |
|                                  |                 |              |           |           |       |        |           |             |
| CSharpEssentials_ReadNumber      | ReadNumber      |     73.26 ns |  0.174 ns |  0.162 ns |  1.00 |      - |         - |          NA |
| Bcl_ReadNumber                   | ReadNumber      |     63.78 ns |  0.102 ns |  0.080 ns |  0.87 |      - |         - |          NA |
| Macross_ReadNumber               | ReadNumber      |     79.45 ns |  0.295 ns |  0.276 ns |  1.08 | 0.0014 |      24 B |          NA |
|                                  |                 |              |           |           |       |        |           |             |
| CSharpEssentials_Write           | Write           |    375.99 ns |  1.443 ns |  1.279 ns |  1.00 | 0.0229 |     384 B |        1.00 |
| Bcl_Write                        | Write           |    347.32 ns |  2.081 ns |  1.947 ns |  0.92 | 0.0229 |     384 B |        1.00 |
| Macross_Write                    | Write           |    657.89 ns |  2.889 ns |  2.703 ns |  1.75 | 0.0229 |     384 B |        1.00 |
