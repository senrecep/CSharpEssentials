```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method           | Mean      | Error     | StdDev    | Ratio | Gen0   | Gen1   | Allocated | Alloc Ratio |
|----------------- |----------:|----------:|----------:|------:|-------:|-------:|----------:|------------:|
| CSharpEssentials |  8.641 μs | 0.0382 μs | 0.0358 μs |  1.00 | 0.4883 |      - |   8.22 KB |        1.00 |
| FluentValidation | 20.176 μs | 0.0779 μs | 0.0729 μs |  2.33 | 1.9531 | 0.0305 |  31.94 KB |        3.88 |
