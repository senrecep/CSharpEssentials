```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                                   | Mean     | Error     | StdDev    | Ratio | RatioSD | Gen0     | Gen1     | Gen2    | Allocated | Alloc Ratio |
|----------------------------------------- |---------:|----------:|----------:|------:|--------:|---------:|---------:|--------:|----------:|------------:|
| Plain_AddSwaggerGen                      | 3.478 ms | 0.0442 ms | 0.0392 ms |  1.00 |    0.02 | 156.2500 |  46.8750 |       - |   2.62 MB |        1.00 |
| Plain_AddSwaggerGen_StringEnums          | 3.480 ms | 0.0535 ms | 0.0474 ms |  1.00 |    0.02 | 156.2500 |  46.8750 |       - |   2.62 MB |        1.00 |
| CSharpEssentials_EnumConventions         | 4.340 ms | 0.0864 ms | 0.1182 ms |  1.25 |    0.04 | 156.2500 |  31.2500 |       - |   3.11 MB |        1.19 |
| CSharpEssentials_IdempotencyConventions  | 4.108 ms | 0.0347 ms | 0.0308 ms |  1.18 |    0.02 | 203.1250 |  93.7500 | 31.2500 |   3.16 MB |        1.21 |
| CSharpEssentials_OptionalRouteParameters | 3.994 ms | 0.0354 ms | 0.0314 ms |  1.15 |    0.02 | 171.8750 |  46.8750 |       - |    3.1 MB |        1.18 |
| CSharpEssentials_AllConventions          | 5.405 ms | 0.0474 ms | 0.0420 ms |  1.55 |    0.02 | 250.0000 | 125.0000 | 31.2500 |   4.12 MB |        1.57 |
| CSharpEssentials_AddSwagger              | 7.308 ms | 0.0392 ms | 0.0348 ms |  2.10 |    0.02 | 531.2500 | 250.0000 | 62.5000 |   8.27 MB |        3.16 |
