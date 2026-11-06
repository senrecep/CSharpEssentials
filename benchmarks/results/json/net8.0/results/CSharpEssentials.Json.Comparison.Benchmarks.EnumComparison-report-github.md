```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                           | Categories      | Mean         | Error      | StdDev    | Ratio | Gen0   | Allocated | Alloc Ratio |
|--------------------------------- |---------------- |-------------:|-----------:|----------:|------:|-------:|----------:|------------:|
| CSharpEssentials_InvalidFallback | InvalidFallback |    100.89 ns |   0.057 ns |  0.050 ns |  1.00 |      - |         - |          NA |
| Macross_InvalidFallback          | InvalidFallback |    111.77 ns |   0.034 ns |  0.026 ns |  1.11 | 0.0019 |      32 B |          NA |
|                                  |                 |              |            |           |       |        |           |             |
| CSharpEssentials_InvalidThrow    | InvalidThrow    | 26,026.04 ns |  72.692 ns | 64.439 ns |  1.00 | 0.0610 |    1360 B |        1.00 |
| Bcl_InvalidThrow                 | InvalidThrow    | 27,470.87 ns | 105.817 ns | 93.804 ns |  1.06 | 0.0610 |    1488 B |        1.09 |
| Macross_InvalidThrow             | InvalidThrow    | 25,562.08 ns |  50.214 ns | 46.970 ns |  0.98 | 0.0610 |    1504 B |        1.11 |
|                                  |                 |              |            |           |       |        |           |             |
| CSharpEssentials_Read            | Read            |  1,196.57 ns |   1.181 ns |  1.047 ns |  1.00 | 0.0172 |     304 B |        1.00 |
| Bcl_Read                         | Read            |  1,120.36 ns |   1.979 ns |  1.653 ns |  0.94 | 0.0572 |     968 B |        3.18 |
| Macross_Read                     | Read            |  1,149.52 ns |   2.299 ns |  2.150 ns |  0.96 | 0.0572 |     968 B |        3.18 |
|                                  |                 |              |            |           |       |        |           |             |
| CSharpEssentials_ReadNumber      | ReadNumber      |     71.30 ns |   0.042 ns |  0.037 ns |  1.00 |      - |         - |          NA |
| Bcl_ReadNumber                   | ReadNumber      |     61.87 ns |   0.064 ns |  0.060 ns |  0.87 |      - |         - |          NA |
| Macross_ReadNumber               | ReadNumber      |    120.86 ns |   1.045 ns |  0.977 ns |  1.70 | 0.0014 |      24 B |          NA |
|                                  |                 |              |            |           |       |        |           |             |
| CSharpEssentials_Write           | Write           |    388.36 ns |   0.871 ns |  0.772 ns |  1.00 | 0.0229 |     384 B |        1.00 |
| Bcl_Write                        | Write           |    368.09 ns |   2.200 ns |  2.057 ns |  0.95 | 0.0229 |     384 B |        1.00 |
| Macross_Write                    | Write           |    582.15 ns |   1.339 ns |  1.187 ns |  1.50 | 0.0229 |     384 B |        1.00 |
