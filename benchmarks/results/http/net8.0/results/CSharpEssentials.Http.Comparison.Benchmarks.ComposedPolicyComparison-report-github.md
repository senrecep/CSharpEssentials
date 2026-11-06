```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                        | Categories | Mean       | Error   | StdDev  | Ratio | Gen0   | Allocated | Alloc Ratio |
|------------------------------ |----------- |-----------:|--------:|--------:|------:|-------:|----------:|------------:|
| CSharpEssentialsT_Fluent      | Generic    |   915.2 ns | 1.42 ns | 1.33 ns |  1.00 | 0.0048 |      80 B |        1.00 |
| PollyT_SingleBuilder          | Generic    |   681.5 ns | 0.38 ns | 0.31 ns |  0.74 |      - |         - |        0.00 |
|                               |            |            |         |         |       |        |           |             |
| CSharpEssentials_Options      | NonGeneric | 1,192.3 ns | 3.69 ns | 3.27 ns |  1.00 | 0.0076 |     128 B |        1.00 |
| CSharpEssentials_Fluent       | NonGeneric | 1,205.1 ns | 1.03 ns | 0.92 ns |  1.01 | 0.0076 |     128 B |        1.00 |
| CSharpEssentials_FromPipeline | NonGeneric |   939.5 ns | 3.52 ns | 3.29 ns |  0.79 | 0.0076 |     128 B |        1.00 |
| Polly_SingleBuilder           | NonGeneric |   917.2 ns | 1.26 ns | 1.11 ns |  0.77 | 0.0029 |      48 B |        0.38 |
