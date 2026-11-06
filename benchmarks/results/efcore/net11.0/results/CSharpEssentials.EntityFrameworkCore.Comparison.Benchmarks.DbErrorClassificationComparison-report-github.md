```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                                               | Categories | Mean       | Error     | StdDev    | Ratio  | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------------------------------------------- |----------- |-----------:|----------:|----------:|-------:|--------:|-------:|----------:|------------:|
| HandWritten_SqlStateSwitch                           | SqlState   |  0.4166 ns | 0.0090 ns | 0.0085 ns |   1.00 |    0.03 |      - |         - |          NA |
| CSharpEssentials_SqlStateErrorTranslator             | SqlState   | 45.9486 ns | 0.4632 ns | 0.4332 ns | 110.34 |    2.38 | 0.0148 |     248 B |          NA |
|                                                      |            |            |           |           |        |         |        |           |             |
| HandWritten_PatternMatch                             | Sqlite     |  0.6730 ns | 0.0116 ns | 0.0102 ns |   1.00 |    0.02 |      - |         - |          NA |
| DbExceptionClassifier_IsUniqueConstraintError        | Sqlite     |  0.9571 ns | 0.0060 ns | 0.0056 ns |   1.42 |    0.02 |      - |         - |          NA |
| CSharpEssentials_DbErrorTranslation_SqliteTranslator | Sqlite     |  2.5418 ns | 0.0123 ns | 0.0109 ns |   3.78 |    0.06 |      - |         - |          NA |
