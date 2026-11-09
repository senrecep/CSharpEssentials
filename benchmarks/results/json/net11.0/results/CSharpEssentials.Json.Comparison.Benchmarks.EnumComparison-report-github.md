```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2


```
| Method                           | Categories      | Mean        | Error     | StdDev    | Ratio | Gen0   | Allocated | Alloc Ratio |
|--------------------------------- |---------------- |------------:|----------:|----------:|------:|-------:|----------:|------------:|
| CSharpEssentials_InvalidFallback | InvalidFallback |    83.37 ns |  0.159 ns |  0.149 ns |  1.00 |      - |         - |          NA |
| Macross_InvalidFallback          | InvalidFallback |    88.02 ns |  0.247 ns |  0.219 ns |  1.06 | 0.0019 |      32 B |          NA |
|                                  |                 |             |           |           |       |        |           |             |
| CSharpEssentials_InvalidThrow    | InvalidThrow    | 4,250.55 ns |  7.087 ns |  6.629 ns |  1.00 | 0.0687 |    1208 B |        1.00 |
| Bcl_InvalidThrow                 | InvalidThrow    | 4,609.53 ns | 17.033 ns | 15.933 ns |  1.08 | 0.0916 |    1648 B |        1.36 |
| Macross_InvalidThrow             | InvalidThrow    | 4,121.34 ns | 11.649 ns | 10.326 ns |  0.97 | 0.0839 |    1432 B |        1.19 |
|                                  |                 |             |           |           |       |        |           |             |
| CSharpEssentials_Read            | Read            |   953.19 ns |  1.911 ns |  1.694 ns |  1.00 | 0.0181 |     304 B |        1.00 |
| Bcl_Read                         | Read            |   834.27 ns |  2.762 ns |  2.448 ns |  0.88 | 0.0181 |     304 B |        1.00 |
| Macross_Read                     | Read            |   893.69 ns |  9.770 ns |  9.139 ns |  0.94 | 0.0572 |     968 B |        3.18 |
|                                  |                 |             |           |           |       |        |           |             |
| CSharpEssentials_ReadNumber      | ReadNumber      |    61.91 ns |  0.114 ns |  0.107 ns |  1.00 |      - |         - |          NA |
| Bcl_ReadNumber                   | ReadNumber      |    52.98 ns |  0.071 ns |  0.063 ns |  0.86 |      - |         - |          NA |
| Macross_ReadNumber               | ReadNumber      |    67.16 ns |  0.224 ns |  0.198 ns |  1.08 | 0.0014 |      24 B |          NA |
|                                  |                 |             |           |           |       |        |           |             |
| CSharpEssentials_Write           | Write           |   319.60 ns |  1.149 ns |  1.019 ns |  1.00 | 0.0229 |     384 B |        1.00 |
| Bcl_Write                        | Write           |   308.16 ns |  1.885 ns |  1.763 ns |  0.96 | 0.0229 |     384 B |        1.00 |
| Macross_Write                    | Write           |   489.11 ns |  2.064 ns |  1.830 ns |  1.53 | 0.0229 |     384 B |        1.00 |
