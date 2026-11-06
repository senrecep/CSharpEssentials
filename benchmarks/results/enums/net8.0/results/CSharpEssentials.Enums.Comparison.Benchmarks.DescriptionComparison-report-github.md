```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method           | Mean      | Error     | StdDev    | Ratio | Allocated | Alloc Ratio |
|----------------- |----------:|----------:|----------:|------:|----------:|------------:|
| CSharpEssentials | 4.4773 ns | 0.0032 ns | 0.0029 ns |  1.00 |         - |          NA |
| EnumsNet         | 4.2669 ns | 0.0040 ns | 0.0036 ns |  0.95 |         - |          NA |
| NetEscapades     | 0.5411 ns | 0.0006 ns | 0.0005 ns |  0.12 |         - |          NA |
