```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2


```
| Method                               | Categories | Mean        | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------------------- |----------- |------------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| CSharpEssentials_For_Short           | For        |    73.54 ns | 0.614 ns | 0.574 ns |  1.00 |    0.01 | 0.0110 |     184 B |        1.00 |
| CSharpEssentials_For_Long            | For        | 1,723.46 ns | 9.083 ns | 8.496 ns | 23.44 |    0.21 | 0.1469 |    2464 B |       13.39 |
|                                      |            |             |          |          |       |         |        |           |             |
| CSharpEssentials_ToStorageKey        | StorageKey |    74.18 ns | 0.455 ns | 0.426 ns |  1.00 |    0.01 | 0.0138 |     232 B |        1.00 |
| CSharpEssentials_Scope_UserAndTenant | StorageKey |   176.76 ns | 1.119 ns | 1.047 ns |  2.38 |    0.02 | 0.0401 |     672 B |        2.90 |
|                                      |            |             |          |          |       |         |        |           |             |
| HandWritten_UuidV5                   | Uuid       |   735.50 ns | 1.784 ns | 1.669 ns |  1.00 |    0.00 |      - |         - |          NA |
| CSharpEssentials_ToUuidV5            | Uuid       | 1,273.68 ns | 4.366 ns | 4.084 ns |  1.73 |    0.01 | 0.0286 |     488 B |          NA |
