```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                                               | Categories | Mean       | Error     | StdDev    | Ratio  | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------------------------------------------- |----------- |-----------:|----------:|----------:|-------:|--------:|-------:|----------:|------------:|
| HandWritten_SqlStateSwitch                           | SqlState   |  0.4459 ns | 0.0079 ns | 0.0070 ns |   1.00 |    0.02 |      - |         - |          NA |
| CSharpEssentials_SqlStateErrorTranslator             | SqlState   | 49.3231 ns | 0.3491 ns | 0.3266 ns | 110.64 |    1.81 | 0.0148 |     248 B |          NA |
|                                                      |            |            |           |           |        |         |        |           |             |
| HandWritten_PatternMatch                             | Sqlite     |  0.7026 ns | 0.0151 ns | 0.0141 ns |   1.00 |    0.03 |      - |         - |          NA |
| DbExceptionClassifier_IsUniqueConstraintError        | Sqlite     |  0.9238 ns | 0.0212 ns | 0.0198 ns |   1.32 |    0.04 |      - |         - |          NA |
| CSharpEssentials_DbErrorTranslation_SqliteTranslator | Sqlite     |  2.5700 ns | 0.0340 ns | 0.0318 ns |   3.66 |    0.08 |      - |         - |          NA |
