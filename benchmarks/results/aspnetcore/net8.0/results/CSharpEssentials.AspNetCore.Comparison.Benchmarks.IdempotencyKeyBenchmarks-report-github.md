```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                               | Categories | Mean        | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------------------- |----------- |------------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| CSharpEssentials_For_Short           | For        |    73.17 ns | 1.019 ns | 0.953 ns |  1.00 |    0.02 | 0.0110 |     184 B |        1.00 |
| CSharpEssentials_For_Long            | For        | 1,814.16 ns | 5.850 ns | 5.185 ns | 24.80 |    0.32 | 0.1469 |    2464 B |       13.39 |
|                                      |            |             |          |          |       |         |        |           |             |
| CSharpEssentials_ToStorageKey        | StorageKey |    84.83 ns | 0.431 ns | 0.403 ns |  1.00 |    0.01 | 0.0138 |     232 B |        1.00 |
| CSharpEssentials_Scope_UserAndTenant | StorageKey |   185.25 ns | 1.028 ns | 0.911 ns |  2.18 |    0.01 | 0.0401 |     672 B |        2.90 |
|                                      |            |             |          |          |       |         |        |           |             |
| HandWritten_UuidV5                   | Uuid       |   733.36 ns | 0.431 ns | 0.382 ns |  1.00 |    0.00 |      - |         - |          NA |
| CSharpEssentials_ToUuidV5            | Uuid       | 1,400.02 ns | 1.547 ns | 1.371 ns |  1.91 |    0.00 | 0.0286 |     488 B |          NA |
