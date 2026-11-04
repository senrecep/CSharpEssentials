```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method           | Mean      | Error     | StdDev    | Ratio | Allocated | Alloc Ratio |
|----------------- |----------:|----------:|----------:|------:|----------:|------------:|
| CSharpEssentials | 4.4799 ns | 0.0043 ns | 0.0036 ns |  1.00 |         - |          NA |
| EnumsNet         | 3.7137 ns | 0.0034 ns | 0.0031 ns |  0.83 |         - |          NA |
| NetEscapades     | 0.5407 ns | 0.0004 ns | 0.0003 ns |  0.12 |         - |          NA |
