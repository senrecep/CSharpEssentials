```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                                   | Mean     | Error     | StdDev    | Ratio | RatioSD | Gen0     | Gen1     | Gen2    | Allocated | Alloc Ratio |
|----------------------------------------- |---------:|----------:|----------:|------:|--------:|---------:|---------:|--------:|----------:|------------:|
| Plain_AddSwaggerGen                      | 3.979 ms | 0.0759 ms | 0.0813 ms |  1.00 |    0.03 | 140.6250 |  46.8750 |       - |    2.5 MB |        1.00 |
| Plain_AddSwaggerGen_StringEnums          | 3.998 ms | 0.0387 ms | 0.0362 ms |  1.01 |    0.02 | 140.6250 |  46.8750 |       - |    2.5 MB |        1.00 |
| CSharpEssentials_EnumConventions         | 5.034 ms | 0.0972 ms | 0.1394 ms |  1.27 |    0.04 | 156.2500 |  31.2500 |       - |   3.04 MB |        1.21 |
| CSharpEssentials_IdempotencyConventions  | 4.815 ms | 0.0840 ms | 0.1536 ms |  1.21 |    0.04 | 187.5000 |  93.7500 | 31.2500 |   3.04 MB |        1.21 |
| CSharpEssentials_OptionalRouteParameters | 4.759 ms | 0.0575 ms | 0.0480 ms |  1.20 |    0.03 | 171.8750 |  62.5000 |       - |   2.99 MB |        1.19 |
| CSharpEssentials_AllConventions          | 6.680 ms | 0.1253 ms | 0.1715 ms |  1.68 |    0.05 | 281.2500 | 156.2500 | 62.5000 |   4.31 MB |        1.72 |
| CSharpEssentials_AddSwagger              | 8.906 ms | 0.1758 ms | 0.2521 ms |  2.24 |    0.08 | 531.2500 | 250.0000 | 62.5000 |   8.43 MB |        3.37 |
