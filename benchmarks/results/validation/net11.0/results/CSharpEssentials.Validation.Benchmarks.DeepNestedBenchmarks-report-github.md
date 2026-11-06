```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method         | Mean       | Error   | StdDev  | Gen0   | Allocated |
|--------------- |-----------:|--------:|--------:|-------:|----------:|
| CSE-DeepNested |   454.2 ns | 0.88 ns | 0.82 ns | 0.0472 |     792 B |
| FV-DeepNested  | 1,146.0 ns | 4.87 ns | 4.56 ns | 0.1240 |    2088 B |
