```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method           | Mean      | Error     | StdDev    | Ratio | Allocated | Alloc Ratio |
|----------------- |----------:|----------:|----------:|------:|----------:|------------:|
| CSharpEssentials | 3.7150 ns | 0.0064 ns | 0.0056 ns |  1.00 |         - |          NA |
| EnumsNet         | 2.6899 ns | 0.0128 ns | 0.0107 ns |  0.72 |         - |          NA |
| NetEscapades     | 0.9021 ns | 0.0006 ns | 0.0005 ns |  0.24 |         - |          NA |
