```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                          | Categories     | Mean     | Error    | StdDev   | Ratio | Allocated | Alloc Ratio |
|-------------------------------- |--------------- |---------:|---------:|---------:|------:|----------:|------------:|
| CSharpEssentials_UtcNow         | UtcNow         | 27.63 ns | 0.031 ns | 0.024 ns |  1.00 |         - |          NA |
| CSharpEssentials_SealedUtcNow   | UtcNow         | 27.44 ns | 0.032 ns | 0.030 ns |  0.99 |         - |          NA |
| Bcl_TimeProviderGetUtcNow       | UtcNow         | 27.44 ns | 0.019 ns | 0.017 ns |  0.99 |         - |          NA |
| Bcl_DateTimeOffsetUtcNow        | UtcNow         | 27.06 ns | 0.007 ns | 0.006 ns |  0.98 |         - |          NA |
|                                 |                |          |          |          |       |           |             |
| CSharpEssentials_UtcNowDate     | UtcNowDate     | 29.50 ns | 0.029 ns | 0.027 ns |  1.00 |         - |          NA |
| Bcl_TimeProviderDateOnly        | UtcNowDate     | 29.76 ns | 0.011 ns | 0.011 ns |  1.01 |         - |          NA |
|                                 |                |          |          |          |       |           |             |
| CSharpEssentials_UtcNowDateTime | UtcNowDateTime | 28.29 ns | 0.004 ns | 0.003 ns |  1.00 |         - |          NA |
| Bcl_TimeProviderUtcDateTime     | UtcNowDateTime | 28.24 ns | 0.020 ns | 0.018 ns |  1.00 |         - |          NA |
| Bcl_DateTimeUtcNow              | UtcNowDateTime | 26.84 ns | 0.017 ns | 0.016 ns |  0.95 |         - |          NA |
|                                 |                |          |          |          |       |           |             |
| CSharpEssentials_UtcNowTime     | UtcNowTime     | 30.52 ns | 0.041 ns | 0.038 ns |  1.00 |         - |          NA |
| Bcl_TimeProviderTimeOnly        | UtcNowTime     | 30.97 ns | 0.015 ns | 0.014 ns |  1.01 |         - |          NA |
