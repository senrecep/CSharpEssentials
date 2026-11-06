```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method         | Mean      | Error     | StdDev    | Ratio | Allocated | Alloc Ratio |
|--------------- |----------:|----------:|----------:|------:|----------:|------------:|
| CseTypeName    | 10.515 μs | 0.0283 μs | 0.0265 μs |  1.00 |         - |          NA |
| CseCustom      |  6.879 μs | 0.0123 μs | 0.0109 μs |  0.65 |         - |          NA |
| DirectWithName |  5.782 μs | 0.0074 μs | 0.0069 μs |  0.55 |         - |          NA |
