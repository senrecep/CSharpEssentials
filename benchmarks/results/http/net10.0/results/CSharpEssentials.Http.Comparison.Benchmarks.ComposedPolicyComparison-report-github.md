```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                        | Categories | Mean     | Error   | StdDev  | Ratio | Gen0   | Allocated | Alloc Ratio |
|------------------------------ |----------- |---------:|--------:|--------:|------:|-------:|----------:|------------:|
| CSharpEssentialsT_Fluent      | Generic    | 781.4 ns | 1.99 ns | 1.86 ns |  1.00 | 0.0048 |      80 B |        1.00 |
| PollyT_SingleBuilder          | Generic    | 579.5 ns | 0.45 ns | 0.40 ns |  0.74 |      - |         - |        0.00 |
|                               |            |          |         |         |       |        |           |             |
| CSharpEssentials_Options      | NonGeneric | 815.0 ns | 0.85 ns | 0.71 ns |  1.00 | 0.0076 |     128 B |        1.00 |
| CSharpEssentials_Fluent       | NonGeneric | 803.8 ns | 0.43 ns | 0.38 ns |  0.99 | 0.0076 |     128 B |        1.00 |
| CSharpEssentials_FromPipeline | NonGeneric | 684.1 ns | 0.85 ns | 0.80 ns |  0.84 | 0.0076 |     128 B |        1.00 |
| Polly_SingleBuilder           | NonGeneric | 633.8 ns | 1.09 ns | 0.91 ns |  0.78 | 0.0029 |      48 B |        0.38 |
