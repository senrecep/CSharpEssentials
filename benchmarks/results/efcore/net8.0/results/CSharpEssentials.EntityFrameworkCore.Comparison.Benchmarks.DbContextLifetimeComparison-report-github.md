```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                                            | Categories         | Mean         | Error     | StdDev    | Ratio | Gen0   | Gen1   | Allocated | Alloc Ratio |
|-------------------------------------------------- |------------------- |-------------:|----------:|----------:|------:|-------:|-------:|----------:|------------:|
| Ef_New                                            | CreateDispose      |  53,390.5 ns | 218.07 ns | 203.98 ns | 1.000 | 2.6245 | 0.1831 |   43969 B |       1.000 |
| Ef_AddDbContext_Scope                             | CreateDispose      |  68,526.6 ns | 160.10 ns | 149.76 ns | 1.284 | 3.4180 | 0.2441 |   58011 B |       1.319 |
| Ef_PooledDbContextFactory                         | CreateDispose      |     286.6 ns |   0.27 ns |   0.24 ns | 0.005 | 0.0091 |      - |     152 B |       0.003 |
| CSharpEssentials_BaseDbContext                    | CreateDispose      |  70,903.3 ns | 227.88 ns | 202.01 ns | 1.328 | 3.4180 | 0.2441 |   58178 B |       1.323 |
| CSharpEssentials_BaseDbContext_Interceptors       | CreateDispose      |  76,859.1 ns | 268.64 ns | 238.15 ns | 1.440 | 3.7842 | 0.2441 |   64316 B |       1.463 |
|                                                   |                    |              |           |           |       |        |        |           |             |
| Ef_New_Query                                      | CreateQueryDispose | 100,673.9 ns | 318.29 ns | 297.73 ns |  1.00 | 2.6855 |      - |   48393 B |        1.00 |
| Ef_AddDbContext_Scope_Query                       | CreateQueryDispose | 118,032.4 ns | 193.34 ns | 171.39 ns |  1.17 | 3.6621 | 0.2441 |   62436 B |        1.29 |
| Ef_PooledDbContextFactory_Query                   | CreateQueryDispose |  14,790.0 ns |  34.80 ns |  32.55 ns |  0.15 | 0.2289 |      - |    3984 B |        0.08 |
| CSharpEssentials_BaseDbContext_Query              | CreateQueryDispose | 118,058.1 ns | 394.10 ns | 368.64 ns |  1.17 | 3.6621 | 0.2441 |   62605 B |        1.29 |
| CSharpEssentials_BaseDbContext_Interceptors_Query | CreateQueryDispose | 129,211.5 ns | 321.13 ns | 284.67 ns |  1.28 | 4.1504 | 0.2441 |   69846 B |        1.44 |
