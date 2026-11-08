```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2


```
| Method                               | Categories | Mean        | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------------------- |----------- |------------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| CSharpEssentials_For_Short           | For        |    43.63 ns | 0.469 ns | 0.438 ns |  1.00 |    0.01 | 0.0091 |     152 B |        1.00 |
| CSharpEssentials_For_Long            | For        | 1,258.60 ns | 9.118 ns | 8.083 ns | 28.85 |    0.33 | 0.1450 |    2432 B |       16.00 |
|                                      |            |             |          |          |       |         |        |           |             |
| CSharpEssentials_ToStorageKey        | StorageKey |    56.34 ns | 0.338 ns | 0.316 ns |  1.00 |    0.01 | 0.0138 |     232 B |        1.00 |
| CSharpEssentials_Scope_UserAndTenant | StorageKey |   132.33 ns | 1.042 ns | 0.975 ns |  2.35 |    0.02 | 0.0377 |     632 B |        2.72 |
|                                      |            |             |          |          |       |         |        |           |             |
| HandWritten_UuidV5                   | Uuid       |   587.31 ns | 0.779 ns | 0.691 ns |  1.00 |    0.00 |      - |         - |          NA |
| CSharpEssentials_ToUuidV5            | Uuid       |   862.30 ns | 2.936 ns | 2.746 ns |  1.47 |    0.00 | 0.0267 |     456 B |          NA |
