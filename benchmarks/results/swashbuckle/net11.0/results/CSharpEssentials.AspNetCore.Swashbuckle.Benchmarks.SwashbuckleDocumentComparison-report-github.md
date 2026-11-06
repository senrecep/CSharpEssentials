```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                                   | Mean     | Error     | StdDev    | Median   | Ratio | RatioSD | Gen0     | Gen1     | Gen2    | Allocated | Alloc Ratio |
|----------------------------------------- |---------:|----------:|----------:|---------:|------:|--------:|---------:|---------:|--------:|----------:|------------:|
| Plain_AddSwaggerGen                      | 3.433 ms | 0.0665 ms | 0.0841 ms | 3.427 ms |  1.00 |    0.03 | 156.2500 |  46.8750 |       - |   2.61 MB |        1.00 |
| Plain_AddSwaggerGen_StringEnums          | 3.304 ms | 0.0582 ms | 0.1289 ms | 3.253 ms |  0.96 |    0.04 | 156.2500 |  46.8750 |       - |   2.61 MB |        1.00 |
| CSharpEssentials_EnumConventions         | 4.074 ms | 0.0504 ms | 0.0897 ms | 4.044 ms |  1.19 |    0.04 | 156.2500 |  62.5000 |       - |   3.09 MB |        1.19 |
| CSharpEssentials_IdempotencyConventions  | 3.979 ms | 0.0548 ms | 0.0513 ms | 3.978 ms |  1.16 |    0.03 | 203.1250 |  93.7500 | 31.2500 |   3.14 MB |        1.20 |
| CSharpEssentials_OptionalRouteParameters | 3.777 ms | 0.0741 ms | 0.0761 ms | 3.746 ms |  1.10 |    0.03 | 171.8750 |  62.5000 |       - |   3.09 MB |        1.18 |
| CSharpEssentials_AllConventions          | 5.242 ms | 0.1033 ms | 0.1726 ms | 5.201 ms |  1.53 |    0.06 | 250.0000 | 125.0000 | 31.2500 |    4.1 MB |        1.57 |
| CSharpEssentials_AddSwagger              | 6.955 ms | 0.1343 ms | 0.1493 ms | 6.915 ms |  2.03 |    0.06 | 531.2500 | 250.0000 | 62.5000 |   8.25 MB |        3.16 |
