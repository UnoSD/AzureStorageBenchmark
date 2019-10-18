# Test details

All resources in the same region (West Europe)

Testing on Ubuntu 18.04 VM D15v2 with accelerated networking 25000 Mbps (per NIC) .NET Core 3.0.100

Premium blob storage with page blobs (StorageV2)

ADLS Gen2 with preview blob API (StorageV2 standard)

File size: 1GB total, 128 blocks of 8388608 byes (8MB to be over 4MB to hit HTBB)

SMB machine:
	DS4 v2
		Network 6000 Mbps (per NIC)
	2xP20 striped
		150 MiB/sec each
		157 MB/sec each
		2,300 IOPS each

# Run the test

Replace tokens with connections data in Program.cs, install dotnet-sdk, copy the files to the VM and run `sudo dotnet run -c Release`.

# Results

``` ini

BenchmarkDotNet=v0.11.5, OS=ubuntu 18.04
Intel Xeon CPU E5-2673 v3 2.40GHz, 2 CPU, 20 logical and 20 physical cores
.NET Core SDK=3.0.100
  [Host]     : .NET Core 3.0.0 (CoreCLR 4.700.19.46205, CoreFX 4.700.19.46214), 64bit RyuJIT
  Job-AZBPPI : .NET Core 3.0.0 (CoreCLR 4.700.19.46205, CoreFX 4.700.19.46214), 64bit RyuJIT

IterationCount=1  LaunchCount=1  RunStrategy=Monitoring  
WarmupCount=0  

```
|                    Method | Concurrency |        Mean | Error | Rank |  Mean speed | Median speed |   Min speed |   Max speed |
|-------------------------- |------------ |------------:|------:|-----:|------------ |------------- |------------ |------------ |
|  PremiumBlockBlobPutBlock |           1 |    478.8 ms |    NA |    1 | 2.2424 GBps |  2.2424 GBps | 2.2424 GBps | 2.2424 GBps |
|     AdlsBlockBlobPutBlock |           1 |    554.0 ms |    NA |    2 | 1.9380 GBps |  1.9380 GBps | 1.9380 GBps | 1.9380 GBps |
| StandardBlockBlobPutBlock |           1 |  1,149.8 ms |    NA |    3 | 0.9338 GBps |  0.9338 GBps | 0.9338 GBps | 0.9338 GBps |
|    PremiumBlockBlobUpload |          54 |  2,500.1 ms |    NA |    4 | 0.4295 GBps |  0.4295 GBps | 0.4295 GBps | 0.4295 GBps |
|    PremiumBlockBlobUpload |          11 |  2,576.8 ms |    NA |    5 | 0.4167 GBps |  0.4167 GBps | 0.4167 GBps | 0.4167 GBps |
|       AdlsBlockBlobUpload |          54 |  2,924.8 ms |    NA |    6 | 0.3671 GBps |  0.3671 GBps | 0.3671 GBps | 0.3671 GBps |
|       AdlsBlockBlobUpload |          11 |  3,693.3 ms |    NA |    7 | 0.2907 GBps |  0.2907 GBps | 0.2907 GBps | 0.2907 GBps |
|   StandardBlockBlobUpload |          54 |  8,529.4 ms |    NA |    8 | 0.1259 GBps |  0.1259 GBps | 0.1259 GBps | 0.1259 GBps |
|   StandardBlockBlobUpload |          11 |  8,587.1 ms |    NA |    8 | 0.1250 GBps |  0.1250 GBps | 0.1250 GBps | 0.1250 GBps |
|    PremiumBlockBlobUpload |           1 | 17,335.2 ms |    NA |    9 | 0.0619 GBps |  0.0619 GBps | 0.0619 GBps | 0.0619 GBps |
|       AdlsBlockBlobUpload |           1 | 21,475.0 ms |    NA |   10 | 0.0500 GBps |  0.0500 GBps | 0.0500 GBps | 0.0500 GBps |
|     PremiumPageBlobUpload |           1 | 23,787.8 ms |    NA |   11 | 0.0451 GBps |  0.0451 GBps | 0.0451 GBps | 0.0451 GBps |
|   StandardBlockBlobUpload |           1 | 24,282.3 ms |    NA |   12 | 0.0442 GBps |  0.0442 GBps | 0.0442 GBps | 0.0442 GBps |
|     PremiumPageBlobUpload |          54 | 28,484.4 ms |    NA |   13 | 0.0377 GBps |  0.0377 GBps | 0.0377 GBps | 0.0377 GBps |
|     PremiumPageBlobUpload |          11 | 30,414.6 ms |    NA |   14 | 0.0353 GBps |  0.0353 GBps | 0.0353 GBps | 0.0353 GBps |
