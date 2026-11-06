```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method             | Mean       | Error    | StdDev   | Gen0   | Allocated |
|------------------- |-----------:|---------:|---------:|-------:|----------:|
| CSE-Simple-Valid   |   327.9 ns |  2.56 ns |  2.40 ns | 0.0353 |     592 B |
| FV-Simple-Valid    |   424.1 ns |  1.48 ns |  1.31 ns | 0.0434 |     728 B |
| CSE-Simple-Invalid |   518.5 ns |  4.31 ns |  4.03 ns | 0.0868 |    1464 B |
| FV-Simple-Invalid  | 3,876.3 ns | 18.65 ns | 16.53 ns | 0.4349 |    7288 B |
