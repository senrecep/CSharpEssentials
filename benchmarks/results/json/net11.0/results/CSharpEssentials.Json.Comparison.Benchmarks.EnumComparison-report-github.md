```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                           | Categories      | Mean        | Error    | StdDev   | Ratio | Gen0   | Allocated | Alloc Ratio |
|--------------------------------- |---------------- |------------:|---------:|---------:|------:|-------:|----------:|------------:|
| CSharpEssentials_InvalidFallback | InvalidFallback |    84.08 ns | 0.550 ns | 0.515 ns |  1.00 |      - |         - |          NA |
| Macross_InvalidFallback          | InvalidFallback |    83.54 ns | 0.519 ns | 0.460 ns |  0.99 | 0.0019 |      32 B |          NA |
|                                  |                 |             |          |          |       |        |           |             |
| CSharpEssentials_InvalidThrow    | InvalidThrow    | 4,306.06 ns | 7.427 ns | 6.948 ns |  1.00 | 0.0687 |    1208 B |        1.00 |
| Bcl_InvalidThrow                 | InvalidThrow    | 4,708.88 ns | 5.916 ns | 5.534 ns |  1.09 | 0.0916 |    1648 B |        1.36 |
| Macross_InvalidThrow             | InvalidThrow    | 4,181.40 ns | 6.041 ns | 5.045 ns |  0.97 | 0.0839 |    1432 B |        1.19 |
|                                  |                 |             |          |          |       |        |           |             |
| CSharpEssentials_Read            | Read            |   970.96 ns | 0.685 ns | 0.572 ns |  1.00 | 0.0172 |     304 B |        1.00 |
| Bcl_Read                         | Read            |   838.82 ns | 0.935 ns | 0.780 ns |  0.86 | 0.0181 |     304 B |        1.00 |
| Macross_Read                     | Read            |   925.57 ns | 1.381 ns | 1.225 ns |  0.95 | 0.0572 |     968 B |        3.18 |
|                                  |                 |             |          |          |       |        |           |             |
| CSharpEssentials_ReadNumber      | ReadNumber      |    62.14 ns | 0.449 ns | 0.420 ns |  1.00 |      - |         - |          NA |
| Bcl_ReadNumber                   | ReadNumber      |    52.99 ns | 0.038 ns | 0.030 ns |  0.85 |      - |         - |          NA |
| Macross_ReadNumber               | ReadNumber      |    64.82 ns | 0.258 ns | 0.241 ns |  1.04 | 0.0014 |      24 B |          NA |
|                                  |                 |             |          |          |       |        |           |             |
| CSharpEssentials_Write           | Write           |   372.48 ns | 0.923 ns | 0.818 ns |  1.00 | 0.0229 |     384 B |        1.00 |
| Bcl_Write                        | Write           |   321.64 ns | 2.846 ns | 2.662 ns |  0.86 | 0.0229 |     384 B |        1.00 |
| Macross_Write                    | Write           |   488.50 ns | 1.075 ns | 1.005 ns |  1.31 | 0.0229 |     384 B |        1.00 |
