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
  Job-FMLRNZ : .NET Core 3.0.0 (CoreCLR 4.700.19.46205, CoreFX 4.700.19.46214), 64bit RyuJIT

LaunchCount=2  RunStrategy=Monitoring  WarmupCount=1  

```
|                    Method | Concurrency  |        Mean | Rank |  Mean speed | Median speed |   Max speed |   Min speed |
|-------------------------- |------------- |------------:|-----:|------------ |------------- |------------ |------------ |
|  PremiumBlockBlobPutBlock | Max (to 128) |    481.9 ms |    1 | 2.2280 GBps |  2.2412 GBps | 2.3826 GBps | 2.0765 GBps |
| StandardBlockBlobPutBlock | Max (to 128) |    699.2 ms |    2 | 1.5358 GBps |  1.6166 GBps | 1.9539 GBps | 0.7487 GBps |
|     AdlsBlockBlobPutBlock | Max (to 128) |  1,476.3 ms |    3 | 0.7273 GBps |  1.7910 GBps | 1.9583 GBps | 0.1052 GBps |
|    PremiumBlockBlobUpload |          11  |  2,550.9 ms |    4 | 0.4209 GBps |  0.4220 GBps | 0.4320 GBps | 0.4065 GBps |
|    PremiumBlockBlobUpload |          54  |  2,609.5 ms |    5 | 0.4115 GBps |  0.4123 GBps | 0.4192 GBps | 0.3995 GBps |
|       AdlsBlockBlobUpload |          11  |  4,251.5 ms |    6 | 0.2526 GBps |  0.2560 GBps | 0.3020 GBps | 0.1744 GBps |
|       AdlsBlockBlobUpload |          54  |  4,254.8 ms |    7 | 0.2524 GBps |  0.2786 GBps | 0.3688 GBps | 0.1029 GBps |
|   StandardBlockBlobUpload |          54  | 11,412.8 ms |    8 | 0.0941 GBps |  0.1102 GBps | 0.1372 GBps | 0.0403 GBps |
|   StandardBlockBlobUpload |          11  | 12,597.5 ms |    9 | 0.0852 GBps |  0.0866 GBps | 0.1114 GBps | 0.0577 GBps |
|    PremiumBlockBlobUpload |           1  | 17,360.9 ms |   10 | 0.0618 GBps |  0.0617 GBps | 0.0632 GBps | 0.0605 GBps |
|       AdlsBlockBlobUpload |           1  | 22,298.5 ms |   11 | 0.0482 GBps |  0.0482 GBps | 0.0529 GBps | 0.0446 GBps |
|     PremiumPageBlobUpload |           1  | 22,982.2 ms |   11 | 0.0467 GBps |  0.0470 GBps | 0.0475 GBps | 0.0453 GBps |
|   StandardBlockBlobUpload |           1  | 24,574.5 ms |   12 | 0.0437 GBps |  0.0436 GBps | 0.0459 GBps | 0.0419 GBps |
|     PremiumPageBlobUpload |          54  | 30,683.4 ms |   13 | 0.0350 GBps |  0.0348 GBps | 0.0377 GBps | 0.0314 GBps |
|     PremiumPageBlobUpload |          11  | 31,350.7 ms |   13 | 0.0342 GBps |  0.0352 GBps | 0.0353 GBps | 0.0281 GBps |
