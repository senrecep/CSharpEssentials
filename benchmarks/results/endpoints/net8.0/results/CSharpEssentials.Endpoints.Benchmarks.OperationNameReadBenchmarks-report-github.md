```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method         | Mean      | Error     | StdDev    | Ratio | Allocated | Alloc Ratio |
|--------------- |----------:|----------:|----------:|------:|----------:|------------:|
| CseTypeName    | 14.531 μs | 0.0229 μs | 0.0203 μs |  1.00 |         - |          NA |
| CseCustom      | 10.247 μs | 0.0509 μs | 0.0476 μs |  0.71 |         - |          NA |
| DirectWithName |  8.723 μs | 0.0590 μs | 0.0552 μs |  0.60 |         - |          NA |
