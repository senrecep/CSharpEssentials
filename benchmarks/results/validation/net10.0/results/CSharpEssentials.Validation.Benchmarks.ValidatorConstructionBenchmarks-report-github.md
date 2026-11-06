```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method   | Mean         | Error      | StdDev     | Gen0   | Gen1   | Allocated |
|--------- |-------------:|-----------:|-----------:|-------:|-------:|----------:|
| CSE-Ctor |     9.444 ns |  0.3289 ns |  0.9699 ns | 0.0014 |      - |      24 B |
| FV-Ctor  | 3,828.194 ns | 21.4652 ns | 20.0786 ns | 0.5341 | 0.0076 |    8968 B |
