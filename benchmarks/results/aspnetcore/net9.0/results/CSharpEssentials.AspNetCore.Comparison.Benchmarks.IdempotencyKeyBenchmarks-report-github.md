```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                               | Categories | Mean        | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------------------- |----------- |------------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| CSharpEssentials_For_Short           | For        |    65.85 ns | 0.337 ns | 0.315 ns |  1.00 |    0.01 | 0.0110 |     184 B |        1.00 |
| CSharpEssentials_For_Long            | For        | 1,795.39 ns | 5.158 ns | 4.825 ns | 27.26 |    0.14 | 0.1469 |    2464 B |       13.39 |
|                                      |            |             |          |          |       |         |        |           |             |
| CSharpEssentials_ToStorageKey        | StorageKey |    72.27 ns | 0.375 ns | 0.351 ns |  1.00 |    0.01 | 0.0138 |     232 B |        1.00 |
| CSharpEssentials_Scope_UserAndTenant | StorageKey |   189.07 ns | 1.714 ns | 1.519 ns |  2.62 |    0.02 | 0.0401 |     672 B |        2.90 |
|                                      |            |             |          |          |       |         |        |           |             |
| HandWritten_UuidV5                   | Uuid       |   736.82 ns | 0.248 ns | 0.207 ns |  1.00 |    0.00 |      - |         - |          NA |
| CSharpEssentials_ToUuidV5            | Uuid       | 1,305.69 ns | 3.039 ns | 2.843 ns |  1.77 |    0.00 | 0.0286 |     488 B |          NA |
