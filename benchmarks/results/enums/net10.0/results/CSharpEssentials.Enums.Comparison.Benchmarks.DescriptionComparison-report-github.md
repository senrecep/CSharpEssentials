```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method           | Mean      | Error     | StdDev    | Ratio | Allocated | Alloc Ratio |
|----------------- |----------:|----------:|----------:|------:|----------:|------------:|
| CSharpEssentials | 3.6495 ns | 0.0145 ns | 0.0136 ns |  1.00 |         - |          NA |
| EnumsNet         | 2.6886 ns | 0.0079 ns | 0.0074 ns |  0.74 |         - |          NA |
| NetEscapades     | 0.9027 ns | 0.0013 ns | 0.0012 ns |  0.25 |         - |          NA |
