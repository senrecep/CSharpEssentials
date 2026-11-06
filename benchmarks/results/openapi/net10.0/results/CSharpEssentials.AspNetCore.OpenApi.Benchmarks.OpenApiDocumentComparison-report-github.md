```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                                   | Mean     | Error     | StdDev    | Ratio | RatioSD | Gen0     | Gen1     | Allocated | Alloc Ratio |
|----------------------------------------- |---------:|----------:|----------:|------:|--------:|---------:|---------:|----------:|------------:|
| Plain_AddOpenApi                         | 5.176 ms | 0.0828 ms | 0.0775 ms |  1.00 |    0.02 | 187.5000 |  62.5000 |   3.11 MB |        1.00 |
| Plain_AddOpenApi_StringEnums             | 5.768 ms | 0.0697 ms | 0.0652 ms |  1.11 |    0.02 | 218.7500 |  62.5000 |   3.76 MB |        1.21 |
| CSharpEssentials_EnumConventions         | 6.341 ms | 0.0685 ms | 0.0607 ms |  1.23 |    0.02 | 218.7500 |  62.5000 |   3.99 MB |        1.28 |
| CSharpEssentials_IdempotencyConventions  | 5.621 ms | 0.0522 ms | 0.0488 ms |  1.09 |    0.02 | 187.5000 |  62.5000 |   3.52 MB |        1.13 |
| CSharpEssentials_OptionalRouteParameters | 5.391 ms | 0.0968 ms | 0.0906 ms |  1.04 |    0.02 | 187.5000 |  62.5000 |   3.43 MB |        1.10 |
| CSharpEssentials_AllConventions          | 7.715 ms | 0.1529 ms | 0.3053 ms |  1.49 |    0.06 | 250.0000 | 125.0000 |   4.72 MB |        1.52 |
