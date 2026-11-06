```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                         | PageNumber | Mean      | Error    | StdDev   | Ratio | Gen0   | Allocated | Alloc Ratio |
|------------------------------- |----------- |----------:|---------:|---------:|------:|-------:|----------:|------------:|
| **Ef_CountSkipTake**               | **1**          |  **84.24 μs** | **0.236 μs** | **0.209 μs** |  **1.00** | **0.9766** |  **23.49 KB** |        **1.00** |
| CSharpEssentials_PaginateAsync | 1          |  86.80 μs | 0.147 μs | 0.123 μs |  1.03 | 0.9766 |  23.53 KB |        1.00 |
| XPagedList_ToPagedListAsync    | 1          |  87.10 μs | 0.201 μs | 0.168 μs |  1.03 | 0.9766 |  23.63 KB |        1.01 |
|                                |            |           |          |          |       |        |           |             |
| **Ef_CountSkipTake**               | **250**        | **126.32 μs** | **0.248 μs** | **0.207 μs** |  **1.00** | **0.9766** |  **23.49 KB** |        **1.00** |
| CSharpEssentials_PaginateAsync | 250        | 126.63 μs | 0.524 μs | 0.465 μs |  1.00 | 0.9766 |  23.53 KB |        1.00 |
| XPagedList_ToPagedListAsync    | 250        | 128.59 μs | 0.388 μs | 0.324 μs |  1.02 | 0.9766 |  23.76 KB |        1.01 |
