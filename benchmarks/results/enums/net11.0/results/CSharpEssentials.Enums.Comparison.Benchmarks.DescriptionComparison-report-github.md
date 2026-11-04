```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method           | Mean      | Error     | StdDev    | Ratio | Allocated | Alloc Ratio |
|----------------- |----------:|----------:|----------:|------:|----------:|------------:|
| CSharpEssentials | 3.4683 ns | 0.0139 ns | 0.0116 ns |  1.00 |         - |          NA |
| EnumsNet         | 2.7856 ns | 0.0542 ns | 0.0507 ns |  0.80 |         - |          NA |
| NetEscapades     | 0.9217 ns | 0.0211 ns | 0.0197 ns |  0.27 |         - |          NA |
