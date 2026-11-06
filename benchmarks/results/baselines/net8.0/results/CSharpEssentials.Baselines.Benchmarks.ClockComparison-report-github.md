```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                          | Categories     | Mean     | Error    | StdDev   | Ratio | Allocated | Alloc Ratio |
|-------------------------------- |--------------- |---------:|---------:|---------:|------:|----------:|------------:|
| CSharpEssentials_UtcNow         | UtcNow         | 27.97 ns | 0.021 ns | 0.020 ns |  1.00 |         - |          NA |
| CSharpEssentials_SealedUtcNow   | UtcNow         | 27.87 ns | 0.045 ns | 0.040 ns |  1.00 |         - |          NA |
| Bcl_TimeProviderGetUtcNow       | UtcNow         | 28.22 ns | 0.008 ns | 0.007 ns |  1.01 |         - |          NA |
| Bcl_DateTimeOffsetUtcNow        | UtcNow         | 27.61 ns | 0.012 ns | 0.012 ns |  0.99 |         - |          NA |
|                                 |                |          |          |          |       |           |             |
| CSharpEssentials_UtcNowDateTime | UtcNowDateTime | 28.08 ns | 0.030 ns | 0.028 ns |  1.00 |         - |          NA |
| Bcl_TimeProviderUtcDateTime     | UtcNowDateTime | 28.18 ns | 0.031 ns | 0.027 ns |  1.00 |         - |          NA |
| Bcl_DateTimeUtcNow              | UtcNowDateTime | 26.55 ns | 0.005 ns | 0.005 ns |  0.95 |         - |          NA |
