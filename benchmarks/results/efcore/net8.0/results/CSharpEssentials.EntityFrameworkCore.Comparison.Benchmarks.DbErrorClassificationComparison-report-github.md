```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                                               | Categories | Mean        | Error     | StdDev    | Ratio  | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------------------------------------------- |----------- |------------:|----------:|----------:|-------:|--------:|-------:|----------:|------------:|
| HandWritten_SqlStateSwitch                           | SqlState   |   0.9317 ns | 0.0082 ns | 0.0077 ns |   1.00 |    0.01 |      - |         - |          NA |
| CSharpEssentials_SqlStateErrorTranslator             | SqlState   | 116.8037 ns | 0.7936 ns | 0.7423 ns | 125.38 |    1.27 | 0.0172 |     288 B |          NA |
|                                                      |            |             |           |           |        |         |        |           |             |
| HandWritten_PatternMatch                             | Sqlite     |   1.3259 ns | 0.0043 ns | 0.0040 ns |   1.00 |    0.00 |      - |         - |          NA |
| CSharpEssentials_DbErrorTranslation_SqliteTranslator | Sqlite     |  12.1964 ns | 0.0053 ns | 0.0047 ns |   9.20 |    0.03 |      - |         - |          NA |
