```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                           | Categories      | Mean         | Error     | StdDev    | Ratio | Gen0   | Allocated | Alloc Ratio |
|--------------------------------- |---------------- |-------------:|----------:|----------:|------:|-------:|----------:|------------:|
| CSharpEssentials_InvalidFallback | InvalidFallback |     97.71 ns |  0.222 ns |  0.197 ns |  1.00 |      - |         - |          NA |
| Macross_InvalidFallback          | InvalidFallback |    106.25 ns |  0.183 ns |  0.171 ns |  1.09 | 0.0019 |      32 B |          NA |
|                                  |                 |              |           |           |       |        |           |             |
| CSharpEssentials_InvalidThrow    | InvalidThrow    |  9,767.04 ns | 66.948 ns | 62.623 ns |  1.00 | 0.0610 |    1208 B |        1.00 |
| Bcl_InvalidThrow                 | InvalidThrow    | 10,369.04 ns | 57.668 ns | 53.942 ns |  1.06 | 0.0916 |    1648 B |        1.36 |
| Macross_InvalidThrow             | InvalidThrow    |  9,575.05 ns | 54.901 ns | 48.669 ns |  0.98 | 0.0763 |    1432 B |        1.19 |
|                                  |                 |              |           |           |       |        |           |             |
| CSharpEssentials_Read            | Read            |  1,155.36 ns |  6.370 ns |  5.647 ns |  1.00 | 0.0172 |     304 B |        1.00 |
| Bcl_Read                         | Read            |    988.82 ns | 13.827 ns | 12.934 ns |  0.86 | 0.0172 |     304 B |        1.00 |
| Macross_Read                     | Read            |  1,163.00 ns |  4.740 ns |  4.202 ns |  1.01 | 0.0572 |     968 B |        3.18 |
|                                  |                 |              |           |           |       |        |           |             |
| CSharpEssentials_ReadNumber      | ReadNumber      |     75.99 ns |  0.517 ns |  0.483 ns |  1.00 |      - |         - |          NA |
| Bcl_ReadNumber                   | ReadNumber      |     64.75 ns |  0.331 ns |  0.294 ns |  0.85 |      - |         - |          NA |
| Macross_ReadNumber               | ReadNumber      |     79.43 ns |  0.363 ns |  0.322 ns |  1.05 | 0.0014 |      24 B |          NA |
|                                  |                 |              |           |           |       |        |           |             |
| CSharpEssentials_Write           | Write           |    431.13 ns |  1.867 ns |  1.655 ns |  1.00 | 0.0229 |     384 B |        1.00 |
| Bcl_Write                        | Write           |    352.30 ns |  5.075 ns |  4.747 ns |  0.82 | 0.0229 |     384 B |        1.00 |
| Macross_Write                    | Write           |    677.14 ns |  4.606 ns |  3.846 ns |  1.57 | 0.0229 |     384 B |        1.00 |
