```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method         | Mean      | Error     | StdDev    | Ratio | Allocated | Alloc Ratio |
|--------------- |----------:|----------:|----------:|------:|----------:|------------:|
| CseTypeName    | 12.280 μs | 0.0154 μs | 0.0120 μs |  1.00 |         - |          NA |
| CseCustom      |  7.396 μs | 0.0092 μs | 0.0072 μs |  0.60 |         - |          NA |
| DirectWithName |  6.381 μs | 0.0088 μs | 0.0078 μs |  0.52 |         - |          NA |
