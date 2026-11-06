```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                        | Categories | Mean     | Error   | StdDev  | Ratio | Gen0   | Allocated | Alloc Ratio |
|------------------------------ |----------- |---------:|--------:|--------:|------:|-------:|----------:|------------:|
| CSharpEssentialsT_Fluent      | Generic    | 825.9 ns | 1.70 ns | 1.50 ns |  1.00 | 0.0048 |      80 B |        1.00 |
| PollyT_SingleBuilder          | Generic    | 620.7 ns | 5.88 ns | 5.50 ns |  0.75 |      - |         - |        0.00 |
|                               |            |          |         |         |       |        |           |             |
| CSharpEssentials_Options      | NonGeneric | 940.0 ns | 7.88 ns | 7.37 ns |  1.00 | 0.0076 |     128 B |        1.00 |
| CSharpEssentials_Fluent       | NonGeneric | 928.4 ns | 2.54 ns | 2.12 ns |  0.99 | 0.0076 |     128 B |        1.00 |
| CSharpEssentials_FromPipeline | NonGeneric | 744.9 ns | 1.66 ns | 1.47 ns |  0.79 | 0.0076 |     128 B |        1.00 |
| Polly_SingleBuilder           | NonGeneric | 679.8 ns | 4.12 ns | 3.65 ns |  0.72 | 0.0029 |      48 B |        0.38 |
