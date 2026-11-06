```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                        | Categories   | Mean      | Error     | StdDev    | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|------------------------------ |------------- |----------:|----------:|----------:|------:|--------:|-------:|-------:|----------:|------------:|
| CSharpEssentials_Plain        | Plain        |  5.307 ns | 0.0234 ns | 0.0207 ns |  1.00 |    0.01 |      - |      - |         - |          NA |
| ErrorOr_Plain                 | Plain        |  5.302 ns | 0.0325 ns | 0.0288 ns |  1.00 |    0.01 |      - |      - |         - |          NA |
| FluentResults_Plain           | Plain        | 38.534 ns | 0.2766 ns | 0.2452 ns |  7.26 |    0.05 | 0.0091 | 0.0001 |     152 B |          NA |
|                               |              |           |           |           |       |         |        |        |           |             |
| CSharpEssentials_WithMetadata | WithMetadata | 73.326 ns | 0.5885 ns | 0.5505 ns |  1.00 |    0.01 | 0.0129 |      - |     216 B |        1.00 |
| ErrorOr_WithMetadata          | WithMetadata | 80.612 ns | 0.6062 ns | 0.5670 ns |  1.10 |    0.01 | 0.0129 |      - |     216 B |        1.00 |
| FluentResults_WithMetadata    | WithMetadata | 88.239 ns | 1.1483 ns | 1.0741 ns |  1.20 |    0.02 | 0.0172 |      - |     288 B |        1.33 |
