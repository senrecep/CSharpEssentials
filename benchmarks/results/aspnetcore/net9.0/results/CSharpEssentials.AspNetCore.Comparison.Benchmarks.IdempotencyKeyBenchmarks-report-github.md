```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2


```
| Method                               | Categories | Mean        | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------------------- |----------- |------------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| CSharpEssentials_For_Short           | For        |    64.61 ns |  0.549 ns |  0.486 ns |  1.00 |    0.01 | 0.0110 |     184 B |        1.00 |
| CSharpEssentials_For_Long            | For        | 1,746.08 ns | 12.605 ns | 11.791 ns | 27.03 |    0.27 | 0.1469 |    2464 B |       13.39 |
|                                      |            |             |           |           |       |         |        |           |             |
| CSharpEssentials_ToStorageKey        | StorageKey |    75.08 ns |  0.911 ns |  0.808 ns |  1.00 |    0.01 | 0.0138 |     232 B |        1.00 |
| CSharpEssentials_Scope_UserAndTenant | StorageKey |   176.55 ns |  1.637 ns |  1.452 ns |  2.35 |    0.03 | 0.0401 |     672 B |        2.90 |
|                                      |            |             |           |           |       |         |        |           |             |
| HandWritten_UuidV5                   | Uuid       |   736.03 ns |  1.992 ns |  1.863 ns |  1.00 |    0.00 |      - |         - |          NA |
| CSharpEssentials_ToUuidV5            | Uuid       |   814.64 ns |  3.788 ns |  3.543 ns |  1.11 |    0.01 | 0.0105 |     184 B |          NA |
