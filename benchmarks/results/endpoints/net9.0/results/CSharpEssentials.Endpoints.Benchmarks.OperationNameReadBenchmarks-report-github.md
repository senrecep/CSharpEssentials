```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method         | Mean      | Error     | StdDev    | Ratio | Allocated | Alloc Ratio |
|--------------- |----------:|----------:|----------:|------:|----------:|------------:|
| CseTypeName    | 13.464 μs | 0.0130 μs | 0.0101 μs |  1.00 |         - |          NA |
| CseCustom      |  9.804 μs | 0.1914 μs | 0.2048 μs |  0.73 |         - |          NA |
| DirectWithName |  8.006 μs | 0.0473 μs | 0.0419 μs |  0.59 |         - |          NA |
