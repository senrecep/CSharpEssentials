```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2


```
| Method                               | Categories | Mean        | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------------------- |----------- |------------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| CSharpEssentials_For_Short           | For        |    57.85 ns | 1.041 ns | 0.974 ns |  1.00 |    0.02 | 0.0091 |     152 B |        1.00 |
| CSharpEssentials_For_Long            | For        | 1,342.18 ns | 7.876 ns | 7.367 ns | 23.21 |    0.39 | 0.1450 |    2432 B |       16.00 |
|                                      |            |             |          |          |       |         |        |           |             |
| CSharpEssentials_ToStorageKey        | StorageKey |    75.38 ns | 0.467 ns | 0.437 ns |  1.00 |    0.01 | 0.0138 |     232 B |        1.00 |
| CSharpEssentials_Scope_UserAndTenant | StorageKey |   166.16 ns | 1.285 ns | 1.139 ns |  2.20 |    0.02 | 0.0377 |     632 B |        2.72 |
|                                      |            |             |          |          |       |         |        |           |             |
| HandWritten_UuidV5                   | Uuid       |   584.51 ns | 0.678 ns | 0.601 ns |  1.00 |    0.00 |      - |         - |          NA |
| CSharpEssentials_ToUuidV5            | Uuid       |   918.42 ns | 2.462 ns | 2.303 ns |  1.57 |    0.00 | 0.0267 |     456 B |          NA |
