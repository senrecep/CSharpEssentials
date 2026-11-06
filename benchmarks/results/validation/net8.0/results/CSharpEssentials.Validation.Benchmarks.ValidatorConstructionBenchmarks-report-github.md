```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method   | Mean        | Error     | StdDev    | Gen0   | Allocated |
|--------- |------------:|----------:|----------:|-------:|----------:|
| CSE-Ctor |    14.32 ns |  0.400 ns |  1.180 ns | 0.0014 |      24 B |
| FV-Ctor  | 5,022.91 ns | 44.407 ns | 39.366 ns | 0.5493 |    9192 B |
