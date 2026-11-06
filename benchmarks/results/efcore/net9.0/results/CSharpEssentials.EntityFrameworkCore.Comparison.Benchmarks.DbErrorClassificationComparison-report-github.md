```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                                               | Categories | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------------------------------------------- |----------- |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| HandWritten_SqlStateSwitch                           | SqlState   |  0.9612 ns | 0.0198 ns | 0.0185 ns |  1.00 |    0.03 |      - |         - |          NA |
| CSharpEssentials_SqlStateErrorTranslator             | SqlState   | 81.4948 ns | 0.8145 ns | 0.7619 ns | 84.82 |    1.75 | 0.0172 |     288 B |          NA |
|                                                      |            |            |           |           |       |         |        |           |             |
| HandWritten_PatternMatch                             | Sqlite     |  0.6815 ns | 0.0224 ns | 0.0199 ns |  1.00 |    0.04 |      - |         - |          NA |
| CSharpEssentials_DbErrorTranslation_SqliteTranslator | Sqlite     |  3.6440 ns | 0.0422 ns | 0.0374 ns |  5.35 |    0.16 |      - |         - |          NA |
