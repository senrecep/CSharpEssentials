```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method  | Variant              | Mean      | Error     | StdDev    | Exceptions | Gen0   | Allocated |
|-------- |--------------------- |----------:|----------:|----------:|-----------:|-------:|----------:|
| **GetById** | **Plain**                | **10.880 μs** | **0.2152 μs** | **0.2113 μs** |          **-** | **0.4883** |   **9.11 KB** |
| Delete  | Plain                |  8.105 μs | 0.1055 μs | 0.0987 μs |          - | 0.4883 |   7.96 KB |
| **GetById** | **CseGenerated**         | **10.710 μs** | **0.2087 μs** | **0.2050 μs** |          **-** | **0.4883** |   **9.11 KB** |
| Delete  | CseGenerated         |  7.882 μs | 0.1524 μs | 0.1565 μs |          - | 0.4883 |   7.96 KB |
| **GetById** | **CseReflection**        | **10.699 μs** | **0.2075 μs** | **0.3523 μs** |          **-** | **0.4883** |   **9.11 KB** |
| Delete  | CseReflection        |  8.126 μs | 0.0984 μs | 0.0821 μs |          - | 0.4883 |   7.96 KB |
| **GetById** | **FastEndpoints**        | **13.470 μs** | **0.2623 μs** | **0.4005 μs** |          **-** | **0.6104** |  **10.48 KB** |
| Delete  | FastEndpoints        | 10.438 μs | 0.0866 μs | 0.0768 μs |          - | 0.5493 |   9.22 KB |
| **GetById** | **FastE(...)ceGen [22]** | **13.390 μs** | **0.2537 μs** | **0.3116 μs** |          **-** | **0.6104** |  **10.48 KB** |
| Delete  | FastE(...)ceGen [22] |  9.982 μs | 0.1873 μs | 0.1564 μs |          - | 0.5493 |   9.22 KB |
