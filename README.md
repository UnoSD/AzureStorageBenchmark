# Test details

All resources in the same region (West Europe)

Testing on Ubuntu 18.04 VM D15v2 with accelerated networking 25000 Mbps (per NIC) .NET Core 3.0.100

Premium blob storage with page blobs (StorageV2)

ADLS Gen2 with preview blob API (StorageV2 standard)

File size: 1GB total, 2048 blocks of 524KB (over 256KB to hit HTBB)

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

|                    Method | concurrency |        Mean |       Error |      StdDev |         Max |         Min | Rank |
|-------------------------- |------------ |------------:|------------:|------------:|------------:|------------:|-----:|
|       AdlsBlockBlobUpload |           1 |          NA |          NA |          NA |          NA |          NA |    ? |
|       AdlsBlockBlobUpload |          54 |          NA |          NA |          NA |          NA |          NA |    ? |
|  PremiumBlockBlobPutBlock |           ? |    847.7 ms |   136.25 ms |   156.90 ms |  1,367.9 ms |    706.0 ms |    1 |
|    PremiumBlockBlobUpload |          54 |  2,847.0 ms |    33.36 ms |    38.42 ms |  2,904.5 ms |  2,780.4 ms |    2 |
|    PremiumBlockBlobUpload |          11 |  2,847.6 ms |    92.05 ms |   106.00 ms |  3,037.1 ms |  2,600.2 ms |    2 |
|       AdlsBlockBlobUpload |          11 |  5,552.3 ms | 1,242.25 ms | 1,430.58 ms |  9,686.5 ms |  4,425.1 ms |    3 |
|              SmbFileWrite |           ? |  9,014.4 ms |   412.77 ms |   475.34 ms | 10,097.8 ms |  8,327.8 ms |    4 |
|   StandardBlockBlobUpload |          54 |  9,920.3 ms | 3,943.36 ms | 4,541.18 ms | 28,043.3 ms |  6,640.4 ms |    4 |
|     AdlsBlockBlobPutBlock |           ? | 10,521.4 ms | 2,912.01 ms | 3,353.47 ms | 15,859.1 ms |  4,218.8 ms |    4 |
|   StandardBlockBlobUpload |          11 | 11,179.1 ms | 2,569.15 ms | 2,958.64 ms | 19,721.5 ms |  7,886.1 ms |    4 |
| StandardBlockBlobPutBlock |           ? | 12,812.5 ms | 4,687.36 ms | 5,397.97 ms | 32,716.2 ms |  6,422.6 ms |    4 |
|    PremiumBlockBlobUpload |           1 | 18,006.4 ms |   616.58 ms |   407.83 ms | 18,864.7 ms | 17,480.8 ms |    5 |
|     PremiumPageBlobUpload |           1 | 23,248.4 ms |   968.12 ms |   640.35 ms | 25,035.3 ms | 22,867.3 ms |    6 |
|   StandardBlockBlobUpload |           1 | 29,117.4 ms | 3,191.81 ms | 3,675.69 ms | 42,567.2 ms | 25,427.9 ms |    7 |
|     PremiumPageBlobUpload |          54 | 29,405.8 ms |   711.11 ms |   818.92 ms | 31,537.0 ms | 28,300.6 ms |    7 |
|     PremiumPageBlobUpload |          11 | 31,246.2 ms | 2,260.72 ms | 2,603.45 ms | 33,924.2 ms | 21,759.5 ms |    8 |
