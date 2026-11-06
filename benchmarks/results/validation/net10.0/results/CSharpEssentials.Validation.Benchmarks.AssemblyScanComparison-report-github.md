```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method           | Mean      | Error     | StdDev    | Ratio | Gen0   | Gen1   | Allocated | Alloc Ratio |
|----------------- |----------:|----------:|----------:|------:|-------:|-------:|----------:|------------:|
| CSharpEssentials |  9.243 μs | 0.0226 μs | 0.0177 μs |  1.00 | 0.5188 |      - |   8.52 KB |        1.00 |
| FluentValidation | 19.603 μs | 0.0487 μs | 0.0432 μs |  2.12 | 1.9226 | 0.0305 |  31.88 KB |        3.74 |
