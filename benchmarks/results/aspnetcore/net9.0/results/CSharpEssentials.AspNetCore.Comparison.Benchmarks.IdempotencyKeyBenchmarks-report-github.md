```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2


```
| Method                               | Categories | Mean        | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------------------- |----------- |------------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| CSharpEssentials_For_Short           | For        |    63.48 ns | 0.908 ns | 0.758 ns |  1.00 |    0.02 | 0.0110 |     184 B |        1.00 |
| CSharpEssentials_For_Long            | For        | 1,694.34 ns | 7.574 ns | 7.085 ns | 26.70 |    0.32 | 0.1469 |    2464 B |       13.39 |
|                                      |            |             |          |          |       |         |        |           |             |
| CSharpEssentials_ToStorageKey        | StorageKey |    67.78 ns | 0.406 ns | 0.360 ns |  1.00 |    0.01 | 0.0138 |     232 B |        1.00 |
| CSharpEssentials_Scope_UserAndTenant | StorageKey |   175.62 ns | 1.182 ns | 1.106 ns |  2.59 |    0.02 | 0.0401 |     672 B |        2.90 |
|                                      |            |             |          |          |       |         |        |           |             |
| HandWritten_UuidV5                   | Uuid       |   738.56 ns | 0.966 ns | 0.903 ns |  1.00 |    0.00 |      - |         - |          NA |
| CSharpEssentials_ToUuidV5            | Uuid       | 1,269.74 ns | 3.530 ns | 3.302 ns |  1.72 |    0.00 | 0.0286 |     488 B |          NA |
