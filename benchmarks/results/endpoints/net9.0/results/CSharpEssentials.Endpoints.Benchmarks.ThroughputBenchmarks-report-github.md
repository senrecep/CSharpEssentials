```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method  | Variant              | Mean      | Error     | StdDev    | Exceptions | Gen0   | Allocated |
|-------- |--------------------- |----------:|----------:|----------:|-----------:|-------:|----------:|
| **GetById** | **Plain**                |  **8.917 μs** | **0.1729 μs** | **0.1922 μs** |          **-** | **0.5493** |   **9.03 KB** |
| Delete  | Plain                |  6.803 μs | 0.0929 μs | 0.0823 μs |          - | 0.4883 |   7.95 KB |
| **GetById** | **CseGenerated**         |  **8.877 μs** | **0.0908 μs** | **0.0805 μs** |          **-** | **0.5493** |   **9.03 KB** |
| Delete  | CseGenerated         |  6.936 μs | 0.0194 μs | 0.0162 μs |          - | 0.4883 |   7.95 KB |
| **GetById** | **CseReflection**        |  **9.059 μs** | **0.1077 μs** | **0.0900 μs** |          **-** | **0.5493** |   **9.03 KB** |
| Delete  | CseReflection        |  6.909 μs | 0.0715 μs | 0.0597 μs |          - | 0.4883 |   7.95 KB |
| **GetById** | **FastEndpoints**        | **11.337 μs** | **0.2059 μs** | **0.2451 μs** |          **-** | **0.6104** |  **10.41 KB** |
| Delete  | FastEndpoints        |  8.536 μs | 0.1144 μs | 0.1014 μs |          - | 0.5493 |    9.2 KB |
| **GetById** | **FastE(...)ceGen [22]** | **11.174 μs** | **0.2062 μs** | **0.1929 μs** |          **-** | **0.6104** |  **10.41 KB** |
| Delete  | FastE(...)ceGen [22] |  8.594 μs | 0.1219 μs | 0.1140 μs |          - | 0.5493 |    9.2 KB |
