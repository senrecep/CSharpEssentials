```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method         | Categories  | Mean     | Error   | StdDev  | Ratio | Gen0      | Allocated | Alloc Ratio |
|--------------- |------------ |---------:|--------:|--------:|------:|----------:|----------:|------------:|
| CseNone        | Cse         | 641.3 ms | 5.53 ms | 4.90 ms |  1.00 | 1000.0000 |  15.94 MB |        1.00 |
| CseTypeName    | Cse         | 648.8 ms | 2.88 ms | 2.69 ms |  1.01 | 1000.0000 |  17.09 MB |        1.07 |
| CseCustom      | Cse         | 645.3 ms | 5.84 ms | 4.88 ms |  1.01 | 1000.0000 |  16.25 MB |        1.02 |
|                |             |          |         |         |       |           |           |             |
| Direct         | HandWritten | 676.7 ms | 2.50 ms | 2.09 ms |  1.00 |         - |  15.15 MB |        1.00 |
| DirectWithName | HandWritten | 682.1 ms | 5.29 ms | 4.95 ms |  1.01 |         - |  15.54 MB |        1.03 |
