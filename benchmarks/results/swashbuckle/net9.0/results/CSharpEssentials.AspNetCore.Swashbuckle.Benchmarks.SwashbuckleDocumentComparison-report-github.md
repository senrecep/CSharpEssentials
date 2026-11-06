```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                                   | Mean     | Error     | StdDev    | Ratio | RatioSD | Gen0     | Gen1     | Gen2    | Allocated | Alloc Ratio |
|----------------------------------------- |---------:|----------:|----------:|------:|--------:|---------:|---------:|--------:|----------:|------------:|
| Plain_AddSwaggerGen                      | 3.790 ms | 0.0630 ms | 0.0526 ms |  1.00 |    0.02 | 156.2500 |  62.5000 |       - |   2.66 MB |        1.00 |
| Plain_AddSwaggerGen_StringEnums          | 3.771 ms | 0.0249 ms | 0.0221 ms |  1.00 |    0.01 | 156.2500 |  62.5000 |       - |   2.66 MB |        1.00 |
| CSharpEssentials_EnumConventions         | 4.836 ms | 0.0430 ms | 0.0381 ms |  1.28 |    0.02 | 203.1250 |  78.1250 | 15.6250 |   3.19 MB |        1.20 |
| CSharpEssentials_IdempotencyConventions  | 4.508 ms | 0.0721 ms | 0.0639 ms |  1.19 |    0.02 | 203.1250 |  93.7500 | 31.2500 |   3.19 MB |        1.20 |
| CSharpEssentials_OptionalRouteParameters | 4.535 ms | 0.0876 ms | 0.0820 ms |  1.20 |    0.03 | 187.5000 |  62.5000 |       - |   3.15 MB |        1.18 |
| CSharpEssentials_AllConventions          | 6.375 ms | 0.1224 ms | 0.1755 ms |  1.68 |    0.05 | 281.2500 | 125.0000 | 62.5000 |   4.45 MB |        1.67 |
| CSharpEssentials_AddSwagger              | 8.073 ms | 0.1097 ms | 0.0916 ms |  2.13 |    0.04 | 531.2500 | 250.0000 | 62.5000 |   8.57 MB |        3.22 |
