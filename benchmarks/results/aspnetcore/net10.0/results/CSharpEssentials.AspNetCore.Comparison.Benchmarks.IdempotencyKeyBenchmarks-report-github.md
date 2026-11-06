```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                               | Categories | Mean        | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------------------- |----------- |------------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| CSharpEssentials_For_Short           | For        |    57.98 ns | 0.948 ns | 0.887 ns |  1.00 |    0.02 | 0.0091 |     152 B |        1.00 |
| CSharpEssentials_For_Long            | For        | 1,373.50 ns | 4.301 ns | 4.023 ns | 23.69 |    0.36 | 0.1450 |    2432 B |       16.00 |
|                                      |            |             |          |          |       |         |        |           |             |
| CSharpEssentials_ToStorageKey        | StorageKey |    80.71 ns | 0.348 ns | 0.326 ns |  1.00 |    0.01 | 0.0138 |     232 B |        1.00 |
| CSharpEssentials_Scope_UserAndTenant | StorageKey |   174.48 ns | 1.141 ns | 1.067 ns |  2.16 |    0.02 | 0.0377 |     632 B |        2.72 |
|                                      |            |             |          |          |       |         |        |           |             |
| HandWritten_UuidV5                   | Uuid       |   582.37 ns | 0.617 ns | 0.515 ns |  1.00 |    0.00 |      - |         - |          NA |
| CSharpEssentials_ToUuidV5            | Uuid       |   891.03 ns | 1.834 ns | 1.716 ns |  1.53 |    0.00 | 0.0267 |     456 B |          NA |
