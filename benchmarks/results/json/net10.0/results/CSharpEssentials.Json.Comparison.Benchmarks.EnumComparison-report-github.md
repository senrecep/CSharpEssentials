```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2


```
| Method                           | Categories      | Mean        | Error     | StdDev    | Ratio | Gen0   | Allocated | Alloc Ratio |
|--------------------------------- |---------------- |------------:|----------:|----------:|------:|-------:|----------:|------------:|
| CSharpEssentials_InvalidFallback | InvalidFallback |    81.32 ns |  0.118 ns |  0.111 ns |  1.00 |      - |         - |          NA |
| Macross_InvalidFallback          | InvalidFallback |    82.55 ns |  0.309 ns |  0.289 ns |  1.02 | 0.0019 |      32 B |          NA |
|                                  |                 |             |           |           |       |        |           |             |
| CSharpEssentials_InvalidThrow    | InvalidThrow    | 5,311.48 ns | 17.307 ns | 16.189 ns |  1.00 | 0.0687 |    1208 B |        1.00 |
| Bcl_InvalidThrow                 | InvalidThrow    | 5,618.19 ns | 13.473 ns | 11.943 ns |  1.06 | 0.0916 |    1648 B |        1.36 |
| Macross_InvalidThrow             | InvalidThrow    | 5,206.36 ns | 12.113 ns | 11.331 ns |  0.98 | 0.0839 |    1432 B |        1.19 |
|                                  |                 |             |           |           |       |        |           |             |
| CSharpEssentials_Read            | Read            |   936.46 ns |  2.786 ns |  2.606 ns |  1.00 | 0.0181 |     304 B |        1.00 |
| Bcl_Read                         | Read            |   835.56 ns |  2.077 ns |  1.943 ns |  0.89 | 0.0181 |     304 B |        1.00 |
| Macross_Read                     | Read            |   893.70 ns |  5.166 ns |  4.832 ns |  0.95 | 0.0572 |     968 B |        3.18 |
|                                  |                 |             |           |           |       |        |           |             |
| CSharpEssentials_ReadNumber      | ReadNumber      |    61.94 ns |  0.134 ns |  0.125 ns |  1.00 |      - |         - |          NA |
| Bcl_ReadNumber                   | ReadNumber      |    53.46 ns |  0.064 ns |  0.057 ns |  0.86 |      - |         - |          NA |
| Macross_ReadNumber               | ReadNumber      |    64.56 ns |  0.298 ns |  0.279 ns |  1.04 | 0.0014 |      24 B |          NA |
|                                  |                 |             |           |           |       |        |           |             |
| CSharpEssentials_Write           | Write           |   365.57 ns |  1.259 ns |  1.178 ns |  1.00 | 0.0229 |     384 B |        1.00 |
| Bcl_Write                        | Write           |   355.26 ns |  1.905 ns |  1.782 ns |  0.97 | 0.0229 |     384 B |        1.00 |
| Macross_Write                    | Write           |   565.31 ns |  1.957 ns |  1.831 ns |  1.55 | 0.0229 |     384 B |        1.00 |
