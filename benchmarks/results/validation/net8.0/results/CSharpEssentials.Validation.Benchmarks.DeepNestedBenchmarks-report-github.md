```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method         | Mean       | Error   | StdDev  | Gen0   | Allocated |
|--------------- |-----------:|--------:|--------:|-------:|----------:|
| CSE-DeepNested |   668.5 ns | 2.17 ns | 2.03 ns | 0.0696 |   1.15 KB |
| FV-DeepNested  | 1,338.4 ns | 3.73 ns | 3.49 ns | 0.1297 |   2.13 KB |
