```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2


```
| Method                               | Categories | Mean        | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------------------- |----------- |------------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| CSharpEssentials_For_Short           | For        |    59.33 ns | 0.775 ns | 0.725 ns |  1.00 |    0.02 | 0.0091 |     152 B |        1.00 |
| CSharpEssentials_For_Long            | For        | 1,381.04 ns | 7.259 ns | 6.435 ns | 23.28 |    0.29 | 0.1450 |    2432 B |       16.00 |
|                                      |            |             |          |          |       |         |        |           |             |
| CSharpEssentials_ToStorageKey        | StorageKey |    79.96 ns | 0.591 ns | 0.524 ns |  1.00 |    0.01 | 0.0138 |     232 B |        1.00 |
| CSharpEssentials_Scope_UserAndTenant | StorageKey |   172.63 ns | 3.495 ns | 2.919 ns |  2.16 |    0.04 | 0.0377 |     632 B |        2.72 |
|                                      |            |             |          |          |       |         |        |           |             |
| HandWritten_UuidV5                   | Uuid       |   583.47 ns | 0.669 ns | 0.626 ns |  1.00 |    0.00 |      - |         - |          NA |
| CSharpEssentials_ToUuidV5            | Uuid       |   645.09 ns | 1.666 ns | 1.558 ns |  1.11 |    0.00 | 0.0086 |     152 B |          NA |
