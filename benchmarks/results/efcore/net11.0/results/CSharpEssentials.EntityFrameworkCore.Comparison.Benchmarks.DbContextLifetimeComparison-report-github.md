```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                                            | Categories         | Mean         | Error     | StdDev    | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|-------------------------------------------------- |------------------- |-------------:|----------:|----------:|------:|--------:|-------:|-------:|----------:|------------:|
| Ef_New                                            | CreateDispose      |  42,676.1 ns | 406.48 ns | 380.22 ns | 1.000 |    0.01 | 2.5635 | 0.1831 |   43529 B |       1.000 |
| Ef_AddDbContext_Scope                             | CreateDispose      |  58,869.6 ns | 190.39 ns | 178.09 ns | 1.380 |    0.01 | 3.5400 | 0.2441 |   60075 B |       1.380 |
| Ef_PooledDbContextFactory                         | CreateDispose      |     192.2 ns |   0.29 ns |   0.27 ns | 0.005 |    0.00 | 0.0043 |      - |      72 B |       0.002 |
| CSharpEssentials_BaseDbContext                    | CreateDispose      |  59,028.3 ns | 303.02 ns | 283.44 ns | 1.383 |    0.01 | 3.5400 | 0.2441 |   60245 B |       1.384 |
| CSharpEssentials_BaseDbContext_Interceptors       | CreateDispose      |  65,919.2 ns | 354.67 ns | 314.41 ns | 1.545 |    0.02 | 3.9063 | 0.2441 |   66583 B |       1.530 |
|                                                   |                    |              |           |           |       |         |        |        |           |             |
| Ef_New_Query                                      | CreateQueryDispose |  86,530.2 ns | 672.78 ns | 629.32 ns |  1.00 |    0.01 | 2.4414 |      - |   48523 B |        1.00 |
| Ef_AddDbContext_Scope_Query                       | CreateQueryDispose | 103,705.1 ns | 466.55 ns | 436.41 ns |  1.20 |    0.01 | 3.4180 |      - |   65070 B |        1.34 |
| Ef_PooledDbContextFactory_Query                   | CreateQueryDispose |  13,518.7 ns |  90.59 ns |  75.65 ns |  0.16 |    0.00 | 0.2441 |      - |    4464 B |        0.09 |
| CSharpEssentials_BaseDbContext_Query              | CreateQueryDispose | 103,408.8 ns | 246.99 ns | 192.84 ns |  1.20 |    0.01 | 3.4180 |      - |   65236 B |        1.34 |
| CSharpEssentials_BaseDbContext_Interceptors_Query | CreateQueryDispose | 112,195.7 ns | 747.42 ns | 624.13 ns |  1.30 |    0.01 | 3.9063 |      - |   72680 B |        1.50 |
