```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method         | Categories  | Mean     | Error   | StdDev  | Ratio | Gen0      | Allocated | Alloc Ratio |
|--------------- |------------ |---------:|--------:|--------:|------:|----------:|----------:|------------:|
| CseNone        | Cse         | 504.6 ms | 1.52 ms | 1.42 ms |  1.00 | 1000.0000 |  15.98 MB |        1.00 |
| CseTypeName    | Cse         | 523.0 ms | 3.75 ms | 3.32 ms |  1.04 | 1000.0000 |  17.26 MB |        1.08 |
| CseCustom      | Cse         | 507.6 ms | 1.28 ms | 1.13 ms |  1.01 | 1000.0000 |  16.34 MB |        1.02 |
|                |             |          |         |         |       |           |           |             |
| Direct         | HandWritten | 532.7 ms | 1.43 ms | 1.34 ms |  1.00 |         - |  15.24 MB |        1.00 |
| DirectWithName | HandWritten | 543.4 ms | 1.16 ms | 1.03 ms |  1.02 |         - |  15.54 MB |        1.02 |
