```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method           | Mean      | Error     | StdDev    | Ratio | Gen0   | Gen1   | Allocated | Alloc Ratio |
|----------------- |----------:|----------:|----------:|------:|-------:|-------:|----------:|------------:|
| CSharpEssentials |  8.069 μs | 0.0263 μs | 0.0233 μs |  1.00 | 0.5188 |      - |   8.52 KB |        1.00 |
| FluentValidation | 19.030 μs | 0.0466 μs | 0.0413 μs |  2.36 | 1.8921 | 0.0305 |  30.97 KB |        3.64 |
