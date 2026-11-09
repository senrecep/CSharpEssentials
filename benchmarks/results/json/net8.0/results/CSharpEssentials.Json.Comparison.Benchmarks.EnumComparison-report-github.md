```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2


```
| Method                           | Categories      | Mean         | Error     | StdDev    | Ratio | Gen0   | Allocated | Alloc Ratio |
|--------------------------------- |---------------- |-------------:|----------:|----------:|------:|-------:|----------:|------------:|
| CSharpEssentials_InvalidFallback | InvalidFallback |    102.73 ns |  0.171 ns |  0.160 ns |  1.00 |      - |         - |          NA |
| Macross_InvalidFallback          | InvalidFallback |    105.42 ns |  0.150 ns |  0.117 ns |  1.03 | 0.0019 |      32 B |          NA |
|                                  |                 |              |           |           |       |        |           |             |
| CSharpEssentials_InvalidThrow    | InvalidThrow    | 26,213.45 ns | 91.861 ns | 85.927 ns |  1.00 | 0.0610 |    1360 B |        1.00 |
| Bcl_InvalidThrow                 | InvalidThrow    | 27,809.70 ns | 63.625 ns | 59.515 ns |  1.06 | 0.0610 |    1488 B |        1.09 |
| Macross_InvalidThrow             | InvalidThrow    | 25,707.75 ns | 94.367 ns | 83.654 ns |  0.98 | 0.0610 |    1504 B |        1.11 |
|                                  |                 |              |           |           |       |        |           |             |
| CSharpEssentials_Read            | Read            |  1,034.08 ns |  1.713 ns |  1.430 ns |  1.00 | 0.0172 |     304 B |        1.00 |
| Bcl_Read                         | Read            |  1,119.15 ns |  3.567 ns |  2.978 ns |  1.08 | 0.0572 |     968 B |        3.18 |
| Macross_Read                     | Read            |  1,123.87 ns |  5.667 ns |  5.300 ns |  1.09 | 0.0572 |     968 B |        3.18 |
|                                  |                 |              |           |           |       |        |           |             |
| CSharpEssentials_ReadNumber      | ReadNumber      |     72.32 ns |  0.092 ns |  0.086 ns |  1.00 |      - |         - |          NA |
| Bcl_ReadNumber                   | ReadNumber      |     64.77 ns |  0.110 ns |  0.103 ns |  0.90 |      - |         - |          NA |
| Macross_ReadNumber               | ReadNumber      |    121.08 ns |  0.341 ns |  0.319 ns |  1.67 | 0.0014 |      24 B |          NA |
|                                  |                 |              |           |           |       |        |           |             |
| CSharpEssentials_Write           | Write           |    360.79 ns |  1.633 ns |  1.528 ns |  1.00 | 0.0229 |     384 B |        1.00 |
| Bcl_Write                        | Write           |    362.73 ns |  2.056 ns |  1.923 ns |  1.01 | 0.0229 |     384 B |        1.00 |
| Macross_Write                    | Write           |    629.03 ns |  3.384 ns |  3.000 ns |  1.74 | 0.0229 |     384 B |        1.00 |
