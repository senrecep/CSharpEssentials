```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                          | Categories     | Mean     | Error    | StdDev   | Ratio | Allocated | Alloc Ratio |
|-------------------------------- |--------------- |---------:|---------:|---------:|------:|----------:|------------:|
| CSharpEssentials_UtcNow         | UtcNow         | 27.96 ns | 0.018 ns | 0.017 ns |  1.00 |         - |          NA |
| CSharpEssentials_SealedUtcNow   | UtcNow         | 27.95 ns | 0.029 ns | 0.027 ns |  1.00 |         - |          NA |
| Bcl_TimeProviderGetUtcNow       | UtcNow         | 28.10 ns | 0.043 ns | 0.040 ns |  1.00 |         - |          NA |
| Bcl_DateTimeOffsetUtcNow        | UtcNow         | 27.73 ns | 0.013 ns | 0.011 ns |  0.99 |         - |          NA |
|                                 |                |          |          |          |       |           |             |
| CSharpEssentials_UtcNowDate     | UtcNowDate     | 29.87 ns | 0.028 ns | 0.026 ns |  1.00 |         - |          NA |
| Bcl_TimeProviderDateOnly        | UtcNowDate     | 29.97 ns | 0.013 ns | 0.012 ns |  1.00 |         - |          NA |
|                                 |                |          |          |          |       |           |             |
| CSharpEssentials_UtcNowDateTime | UtcNowDateTime | 27.95 ns | 0.009 ns | 0.009 ns |  1.00 |         - |          NA |
| Bcl_TimeProviderUtcDateTime     | UtcNowDateTime | 28.04 ns | 0.161 ns | 0.150 ns |  1.00 |         - |          NA |
| Bcl_DateTimeUtcNow              | UtcNowDateTime | 26.84 ns | 0.021 ns | 0.020 ns |  0.96 |         - |          NA |
|                                 |                |          |          |          |       |           |             |
| CSharpEssentials_UtcNowTime     | UtcNowTime     | 31.92 ns | 0.009 ns | 0.008 ns |  1.00 |         - |          NA |
| Bcl_TimeProviderTimeOnly        | UtcNowTime     | 32.01 ns | 0.018 ns | 0.016 ns |  1.00 |         - |          NA |
