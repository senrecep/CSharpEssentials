```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2


```
| Method                               | Categories | Mean        | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------------------- |----------- |------------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| CSharpEssentials_For_Short           | For        |    42.98 ns | 0.144 ns | 0.128 ns |  1.00 |    0.00 | 0.0091 |     152 B |        1.00 |
| CSharpEssentials_For_Long            | For        | 1,270.77 ns | 7.748 ns | 7.248 ns | 29.57 |    0.18 | 0.1450 |    2432 B |       16.00 |
|                                      |            |             |          |          |       |         |        |           |             |
| CSharpEssentials_ToStorageKey        | StorageKey |    56.35 ns | 0.234 ns | 0.183 ns |  1.00 |    0.00 | 0.0138 |     232 B |        1.00 |
| CSharpEssentials_Scope_UserAndTenant | StorageKey |   127.49 ns | 1.243 ns | 1.163 ns |  2.26 |    0.02 | 0.0377 |     632 B |        2.72 |
|                                      |            |             |          |          |       |         |        |           |             |
| HandWritten_UuidV5                   | Uuid       |   587.64 ns | 1.144 ns | 1.014 ns |  1.00 |    0.00 |      - |         - |          NA |
| CSharpEssentials_ToUuidV5            | Uuid       |   633.89 ns | 1.291 ns | 1.144 ns |  1.08 |    0.00 | 0.0086 |     152 B |          NA |
