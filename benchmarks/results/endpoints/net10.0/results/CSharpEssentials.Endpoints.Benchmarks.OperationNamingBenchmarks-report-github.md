```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method         | Categories  | Mean     | Error   | StdDev  | Ratio | Gen0      | Allocated | Alloc Ratio |
|--------------- |------------ |---------:|--------:|--------:|------:|----------:|----------:|------------:|
| CseNone        | Cse         | 703.9 ms | 1.70 ms | 1.51 ms |  1.00 |         - |  15.62 MB |        1.00 |
| CseTypeName    | Cse         | 714.4 ms | 2.53 ms | 2.25 ms |  1.01 | 1000.0000 |  16.83 MB |        1.08 |
| CseCustom      | Cse         | 711.4 ms | 1.11 ms | 0.98 ms |  1.01 | 1000.0000 |  15.94 MB |        1.02 |
|                |             |          |         |         |       |           |           |             |
| Direct         | HandWritten | 747.7 ms | 1.59 ms | 1.41 ms |  1.00 |         - |  15.01 MB |        1.00 |
| DirectWithName | HandWritten | 753.5 ms | 2.76 ms | 2.45 ms |  1.01 |         - |  15.23 MB |        1.01 |
