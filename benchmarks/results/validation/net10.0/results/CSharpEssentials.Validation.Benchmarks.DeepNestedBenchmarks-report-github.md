```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method         | Mean       | Error   | StdDev  | Gen0   | Allocated |
|--------------- |-----------:|--------:|--------:|-------:|----------:|
| CSE-DeepNested |   455.8 ns | 1.53 ns | 1.43 ns | 0.0472 |     792 B |
| FV-DeepNested  | 1,117.8 ns | 4.76 ns | 4.46 ns | 0.1240 |    2088 B |
