```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method           | Mean      | Error     | StdDev    | Ratio | Allocated | Alloc Ratio |
|----------------- |----------:|----------:|----------:|------:|----------:|------------:|
| CSharpEssentials | 3.1203 ns | 0.0024 ns | 0.0021 ns |  1.00 |         - |          NA |
| EnumsNet         | 2.7039 ns | 0.0091 ns | 0.0085 ns |  0.87 |         - |          NA |
| NetEscapades     | 0.9022 ns | 0.0010 ns | 0.0009 ns |  0.29 |         - |          NA |
