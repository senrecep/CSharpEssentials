```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method      | Variant              | Mean      | Error     | StdDev    | Exceptions | Gen0   | Allocated |
|------------ |--------------------- |----------:|----------:|----------:|-----------:|-------:|----------:|
| **GetById**     | **Plain**                | **12.136 μs** | **0.1309 μs** | **0.1160 μs** |          **-** | **0.6104** |  **10.11 KB** |
| PostValid   | Plain                | 17.740 μs | 0.3434 μs | 0.3044 μs |          - | 0.7324 |  13.51 KB |
| PostInvalid | Plain                | 24.749 μs | 0.4491 μs | 0.9473 μs |          - | 0.9766 |  16.99 KB |
| Delete      | Plain                |  9.688 μs | 0.1844 μs | 0.2265 μs |          - | 0.5493 |   9.29 KB |
| **GetById**     | **CseGenerated**         | **12.164 μs** | **0.2126 μs** | **0.1989 μs** |          **-** | **0.6104** |  **10.11 KB** |
| PostValid   | CseGenerated         | 16.946 μs | 0.3338 μs | 0.5667 μs |          - | 0.7324 |  13.51 KB |
| PostInvalid | CseGenerated         | 27.745 μs | 0.4393 μs | 0.6968 μs |          - | 0.9766 |  16.99 KB |
| Delete      | CseGenerated         |  9.544 μs | 0.1878 μs | 0.3837 μs |          - | 0.5493 |   9.33 KB |
| **GetById**     | **CseReflection**        | **11.758 μs** | **0.1920 μs** | **0.1702 μs** |          **-** | **0.6104** |  **10.11 KB** |
| PostValid   | CseReflection        | 16.448 μs | 0.3195 μs | 0.4880 μs |          - | 0.7324 |   13.5 KB |
| PostInvalid | CseReflection        | 24.654 μs | 0.3217 μs | 0.2687 μs |          - | 0.9766 |  16.99 KB |
| Delete      | CseReflection        |  9.088 μs | 0.1760 μs | 0.2350 μs |          - | 0.5493 |   9.28 KB |
| **GetById**     | **Carter**               | **11.848 μs** | **0.2103 μs** | **0.1864 μs** |          **-** | **0.6104** |  **10.11 KB** |
| PostValid   | Carter               | 16.250 μs | 0.3207 μs | 0.4390 μs |          - | 0.7324 |   12.7 KB |
| PostInvalid | Carter               | 28.183 μs | 0.5418 μs | 0.7942 μs |          - | 1.2207 |  22.68 KB |
| Delete      | Carter               | 10.296 μs | 0.2060 μs | 0.2115 μs |          - | 0.5493 |   9.33 KB |
| **GetById**     | **FastEndpoints**        | **12.541 μs** | **0.1595 μs** | **0.1332 μs** |          **-** | **0.6104** |  **11.48 KB** |
| PostValid   | FastEndpoints        | 20.872 μs | 0.4136 μs | 0.6561 μs |          - | 0.7324 |  14.08 KB |
| PostInvalid | FastEndpoints        | 43.274 μs | 0.8585 μs | 1.0220 μs |     2.0000 | 1.4648 |   24.6 KB |
| Delete      | FastEndpoints        | 12.176 μs | 0.2366 μs | 0.2724 μs |          - | 0.6104 |   10.5 KB |
| **GetById**     | **FastE(...)ceGen [22]** | **12.126 μs** | **0.1726 μs** | **0.1442 μs** |          **-** | **0.6104** |  **11.48 KB** |
| PostValid   | FastE(...)ceGen [22] | 20.356 μs | 0.4387 μs | 1.1324 μs |          - | 0.7324 |  14.07 KB |
| PostInvalid | FastE(...)ceGen [22] | 44.565 μs | 0.8866 μs | 0.8707 μs |     2.0000 | 1.4648 |   24.6 KB |
| Delete      | FastE(...)ceGen [22] | 11.765 μs | 0.2162 μs | 0.2023 μs |          - | 0.6104 |  10.48 KB |
