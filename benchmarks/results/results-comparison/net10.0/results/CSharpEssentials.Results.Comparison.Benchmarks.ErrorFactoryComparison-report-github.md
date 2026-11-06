```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                        | Categories   | Mean      | Error     | StdDev    | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|------------------------------ |------------- |----------:|----------:|----------:|------:|--------:|-------:|-------:|----------:|------------:|
| CSharpEssentials_Plain        | Plain        |  5.771 ns | 0.0048 ns | 0.0037 ns |  1.00 |    0.00 |      - |      - |         - |          NA |
| ErrorOr_Plain                 | Plain        |  5.280 ns | 0.0022 ns | 0.0020 ns |  0.91 |    0.00 |      - |      - |         - |          NA |
| FluentResults_Plain           | Plain        | 28.055 ns | 0.2861 ns | 0.2676 ns |  4.86 |    0.05 | 0.0091 | 0.0001 |     152 B |          NA |
|                               |              |           |           |           |       |         |        |        |           |             |
| CSharpEssentials_WithMetadata | WithMetadata | 64.585 ns | 0.3249 ns | 0.3039 ns |  1.00 |    0.01 | 0.0129 |      - |     216 B |        1.00 |
| ErrorOr_WithMetadata          | WithMetadata | 60.802 ns | 0.5427 ns | 0.5077 ns |  0.94 |    0.01 | 0.0129 |      - |     216 B |        1.00 |
| FluentResults_WithMetadata    | WithMetadata | 68.929 ns | 0.6433 ns | 0.6017 ns |  1.07 |    0.01 | 0.0172 |      - |     288 B |        1.33 |
