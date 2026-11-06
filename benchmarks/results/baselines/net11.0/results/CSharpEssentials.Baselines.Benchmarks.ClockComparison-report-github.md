```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                          | Categories     | Mean     | Error    | StdDev   | Ratio | Allocated | Alloc Ratio |
|-------------------------------- |--------------- |---------:|---------:|---------:|------:|----------:|------------:|
| CSharpEssentials_UtcNow         | UtcNow         | 27.67 ns | 0.016 ns | 0.015 ns |  1.00 |         - |          NA |
| CSharpEssentials_SealedUtcNow   | UtcNow         | 27.20 ns | 0.032 ns | 0.030 ns |  0.98 |         - |          NA |
| Bcl_TimeProviderGetUtcNow       | UtcNow         | 27.44 ns | 0.035 ns | 0.033 ns |  0.99 |         - |          NA |
| Bcl_DateTimeOffsetUtcNow        | UtcNow         | 27.16 ns | 0.024 ns | 0.022 ns |  0.98 |         - |          NA |
|                                 |                |          |          |          |       |           |             |
| CSharpEssentials_UtcNowDate     | UtcNowDate     | 29.68 ns | 0.044 ns | 0.039 ns |  1.00 |         - |          NA |
| Bcl_TimeProviderDateOnly        | UtcNowDate     | 29.89 ns | 0.027 ns | 0.024 ns |  1.01 |         - |          NA |
|                                 |                |          |          |          |       |           |             |
| CSharpEssentials_UtcNowDateTime | UtcNowDateTime | 28.23 ns | 0.008 ns | 0.007 ns |  1.00 |         - |          NA |
| Bcl_TimeProviderUtcDateTime     | UtcNowDateTime | 28.00 ns | 0.009 ns | 0.007 ns |  0.99 |         - |          NA |
| Bcl_DateTimeUtcNow              | UtcNowDateTime | 26.78 ns | 0.010 ns | 0.009 ns |  0.95 |         - |          NA |
|                                 |                |          |          |          |       |           |             |
| CSharpEssentials_UtcNowTime     | UtcNowTime     | 30.60 ns | 0.023 ns | 0.021 ns |  1.00 |         - |          NA |
| Bcl_TimeProviderTimeOnly        | UtcNowTime     | 30.87 ns | 0.023 ns | 0.020 ns |  1.01 |         - |          NA |
