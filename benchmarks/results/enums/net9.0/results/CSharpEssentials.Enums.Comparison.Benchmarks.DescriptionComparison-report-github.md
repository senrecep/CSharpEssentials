```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method           | Mean      | Error     | StdDev    | Ratio | Allocated | Alloc Ratio |
|----------------- |----------:|----------:|----------:|------:|----------:|------------:|
| CSharpEssentials | 4.5524 ns | 0.0114 ns | 0.0089 ns |  1.00 |         - |          NA |
| EnumsNet         | 3.4942 ns | 0.0028 ns | 0.0023 ns |  0.77 |         - |          NA |
| NetEscapades     | 0.9030 ns | 0.0011 ns | 0.0011 ns |  0.20 |         - |          NA |
