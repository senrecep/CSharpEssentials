```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                                            | Categories         | Mean         | Error       | StdDev      | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|-------------------------------------------------- |------------------- |-------------:|------------:|------------:|------:|--------:|-------:|-------:|----------:|------------:|
| Ef_New                                            | CreateDispose      |  50,928.0 ns |   997.87 ns | 1,431.12 ns | 1.001 |    0.04 | 2.5635 | 0.1831 |   43784 B |       1.000 |
| Ef_AddDbContext_Scope                             | CreateDispose      |  64,698.5 ns |   367.44 ns |   325.73 ns | 1.271 |    0.04 | 3.5400 | 0.2441 |   60029 B |       1.371 |
| Ef_PooledDbContextFactory                         | CreateDispose      |     245.1 ns |     4.89 ns |     5.03 ns | 0.005 |    0.00 | 0.0067 |      - |     112 B |       0.003 |
| CSharpEssentials_BaseDbContext                    | CreateDispose      |  69,587.6 ns | 1,363.79 ns | 1,275.69 ns | 1.367 |    0.04 | 3.5400 | 0.2441 |   60195 B |       1.375 |
| CSharpEssentials_BaseDbContext_Interceptors       | CreateDispose      |  74,789.2 ns | 1,484.85 ns | 2,355.12 ns | 1.470 |    0.06 | 3.9063 | 0.2441 |   66535 B |       1.520 |
|                                                   |                    |              |             |             |       |         |        |        |           |             |
| Ef_New_Query                                      | CreateQueryDispose |  95,389.9 ns |   411.05 ns |   384.49 ns |  1.00 |    0.01 | 2.6855 |      - |   48817 B |        1.00 |
| Ef_AddDbContext_Scope_Query                       | CreateQueryDispose | 112,085.0 ns |   559.18 ns |   495.69 ns |  1.18 |    0.01 | 3.6621 | 0.2441 |   65062 B |        1.33 |
| Ef_PooledDbContextFactory_Query                   | CreateQueryDispose |  14,692.4 ns |    74.19 ns |    65.77 ns |  0.15 |    0.00 | 0.2594 |      - |    4504 B |        0.09 |
| CSharpEssentials_BaseDbContext_Query              | CreateQueryDispose | 112,902.6 ns |   516.01 ns |   482.68 ns |  1.18 |    0.01 | 3.6621 | 0.2441 |   65230 B |        1.34 |
| CSharpEssentials_BaseDbContext_Interceptors_Query | CreateQueryDispose | 124,157.0 ns |   861.87 ns |   672.89 ns |  1.30 |    0.01 | 4.1504 | 0.2441 |   72669 B |        1.49 |
