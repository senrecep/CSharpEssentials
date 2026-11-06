```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                        | Categories | Mean     | Error   | StdDev  | Ratio | Gen0   | Allocated | Alloc Ratio |
|------------------------------ |----------- |---------:|--------:|--------:|------:|-------:|----------:|------------:|
| CSharpEssentialsT_Fluent      | Generic    | 791.7 ns | 0.92 ns | 0.86 ns |  1.00 | 0.0048 |      80 B |        1.00 |
| PollyT_SingleBuilder          | Generic    | 567.3 ns | 1.16 ns | 1.08 ns |  0.72 |      - |         - |        0.00 |
|                               |            |          |         |         |       |        |           |             |
| CSharpEssentials_Options      | NonGeneric | 777.1 ns | 1.69 ns | 1.58 ns |  1.00 | 0.0076 |     128 B |        1.00 |
| CSharpEssentials_Fluent       | NonGeneric | 775.1 ns | 4.47 ns | 4.18 ns |  1.00 | 0.0076 |     128 B |        1.00 |
| CSharpEssentials_FromPipeline | NonGeneric | 651.5 ns | 0.34 ns | 0.27 ns |  0.84 | 0.0076 |     128 B |        1.00 |
| Polly_SingleBuilder           | NonGeneric | 568.4 ns | 0.37 ns | 0.31 ns |  0.73 | 0.0029 |      48 B |        0.38 |
