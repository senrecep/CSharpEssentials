```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2


```
| Method                               | Categories | Mean        | Error     | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------------------- |----------- |------------:|----------:|---------:|------:|--------:|-------:|----------:|------------:|
| CSharpEssentials_For_Short           | For        |    72.76 ns |  0.669 ns | 0.626 ns |  1.00 |    0.01 | 0.0110 |     184 B |        1.00 |
| CSharpEssentials_For_Long            | For        | 1,669.90 ns | 10.316 ns | 9.649 ns | 22.95 |    0.23 | 0.1469 |    2464 B |       13.39 |
|                                      |            |             |           |          |       |         |        |           |             |
| CSharpEssentials_ToStorageKey        | StorageKey |    77.93 ns |  1.279 ns | 1.134 ns |  1.00 |    0.02 | 0.0138 |     232 B |        1.00 |
| CSharpEssentials_Scope_UserAndTenant | StorageKey |   187.67 ns |  2.395 ns | 2.240 ns |  2.41 |    0.04 | 0.0401 |     672 B |        2.90 |
|                                      |            |             |           |          |       |         |        |           |             |
| HandWritten_UuidV5                   | Uuid       |   745.18 ns |  4.488 ns | 4.198 ns |  1.00 |    0.01 |      - |         - |          NA |
| CSharpEssentials_ToUuidV5            | Uuid       | 1,316.23 ns |  3.248 ns | 3.038 ns |  1.77 |    0.01 | 0.0286 |     488 B |          NA |
