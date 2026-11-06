```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method         | Categories  | Mean     | Error   | StdDev  | Ratio | Gen0      | Allocated | Alloc Ratio |
|--------------- |------------ |---------:|--------:|--------:|------:|----------:|----------:|------------:|
| CseNone        | Cse         | 749.4 ms | 3.91 ms | 3.47 ms |  1.00 |         - |  15.91 MB |        1.00 |
| CseTypeName    | Cse         | 761.0 ms | 3.81 ms | 3.38 ms |  1.02 | 1000.0000 |  16.95 MB |        1.07 |
| CseCustom      | Cse         | 761.8 ms | 5.50 ms | 5.14 ms |  1.02 | 1000.0000 |  16.14 MB |        1.01 |
|                |             |          |         |         |       |           |           |             |
| Direct         | HandWritten | 798.5 ms | 1.60 ms | 1.42 ms |  1.00 |         - |  15.05 MB |        1.00 |
| DirectWithName | HandWritten | 799.2 ms | 2.01 ms | 1.88 ms |  1.00 |         - |  15.26 MB |        1.01 |
