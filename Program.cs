using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Mathematics;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using static StorageBenchmark.Program;

namespace StorageBenchmark
{
    static class AdlsG2Config
    {
        public const string ConnectionString =
            "DefaultEndpointsProtocol=https;" +
            "AccountName=<ADLS Gen2 account name here>;" +
            "AccountKey=<Account key here>;" +
            "EndpointSuffix=core.windows.net";
    }

    static class PremiumBlobConfig
    {
        public const string ConnectionString =
            "DefaultEndpointsProtocol=https;" +
            "AccountName=<Premium blob account name here>;" +
            "AccountKey=<Account key here>;" +
            "EndpointSuffix=core.windows.net";
    }

    static class StandardBlobConfig
    {
        public const string ConnectionString =
            "DefaultEndpointsProtocol=https;" +
            "AccountName=<Standard blob account name here>;" +
            "AccountKey=<Account key here>;" +
            "EndpointSuffix=core.windows.net";
    }

    static class PremiumBlockBlobConfig
    {
        public const string ConnectionString =
            "DefaultEndpointsProtocol=https;" +
            "AccountName=<Premium block blob account name here>;" +
            "AccountKey=<Account key here>;" +
            "EndpointSuffix=core.windows.net";
    }

    static class SmbConfig
    {
        public const string FileLocation = @"/<Local mount point of SMB share here>/";
    }

    static class Config
    {
        public const string ContainerName = "perftest";
        public const int RandomDataSizeBytes = 1_073_741_824;
        public const int BlockCount = 128;
        public const int BenchmarkProcessRunCount = 2;
        public const int BenchmarkTestRunCount = -1;
        public const int BenchmarkWarmupCount = 1;
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class GbpsColumnsAttribute : ColumnConfigBaseAttribute
    {
        public GbpsColumnsAttribute(int sizeInBytes, bool showSizeInBytes) : base(CreateColumn(sizeInBytes, showSizeInBytes)) { }

        private static IColumn[] CreateColumn(int sizeInBytes, bool showSizeInBytes) =>
            new IColumn[] {
            new CalculatedColumn(statistics => $"{GetGbps(sizeInBytes, statistics.Mean, showSizeInBytes):F4} G{(showSizeInBytes ? "B" : "b")}ps", "Mean speed"),
            new CalculatedColumn(statistics => $"{GetGbps(sizeInBytes, statistics.Median, showSizeInBytes):F4} G{(showSizeInBytes ? "B" : "b")}ps", "Median speed"),
            new CalculatedColumn(statistics => $"{GetGbps(sizeInBytes, statistics.Min, showSizeInBytes):F4} G{(showSizeInBytes ? "B" : "b")}ps", "Min speed"),
            new CalculatedColumn(statistics => $"{GetGbps(sizeInBytes, statistics.Max, showSizeInBytes):F4} G{(showSizeInBytes ? "B" : "b")}ps", "Max speed")
        };

        static double GetGbps(int sizeInBytes, double nanosecondsTime, bool showSizeInBytes) =>
            sizeInBytes *
            (showSizeInBytes ? 1L : 8L /*GB in gb*/) /
            nanosecondsTime /*ns to s (* 1_000_000_000) bytes to GB (/ 1_000_000_000) so (* 1) */;
    }

    public class CalculatedColumn : IColumn
    {
        readonly Func<Statistics, string> _getValue;

        public string Id => ColumnName;
        public string ColumnName { get; }

        public CalculatedColumn(Func<Statistics, string> getValue, string name)
        {
            ColumnName = name;
            _getValue = getValue;
        }

        public bool IsDefault(Summary summary, BenchmarkCase benchmarkCase) =>
            false;

        public string GetValue(Summary summary, BenchmarkCase benchmarkCase) =>
            _getValue(summary.Reports
                             .Single(r => r.BenchmarkCase == benchmarkCase)
                             .ResultStatistics);

        public bool IsAvailable(Summary summary) => true;

        public bool AlwaysShow => true;

        public ColumnCategory Category => ColumnCategory.Custom;

        public int PriorityInCategory => 0;

        public bool IsNumeric => false;

        public UnitType UnitType => UnitType.Dimensionless;

        public string Legend => "Data per second column";

        public string GetValue(Summary summary, BenchmarkCase benchmarkCase, SummaryStyle style) =>
            GetValue(summary, benchmarkCase);

        public override string ToString() => ColumnName;
    }

    [RankColumn, GbpsColumns(Config.RandomDataSizeBytes, true)]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [SimpleJob(RunStrategy.Monitoring,
               launchCount: Config.BenchmarkProcessRunCount,
               warmupCount: Config.BenchmarkWarmupCount,
               targetCount: Config.BenchmarkTestRunCount)]
    public class StorageWriteTest
    {
        internal static readonly byte[] RandomData = new byte[Config.RandomDataSizeBytes];

        static Lazy<CloudBlockBlob> GetBlockBlobReference(string connectionString) =>
            GetBlobReference<CloudBlockBlob>(connectionString, c => c.GetBlockBlobReference);

        static Lazy<T> GetBlobReference<T>(string connectionString,
                                           Func<CloudBlobContainer, Func<string, T>> getBlobRef)
            where T : ICloudBlob =>
            new Lazy<T>(() =>
        {
            var containerRef =
                CloudStorageAccount.Parse(connectionString)
                                   .CreateCloudBlobClient()
                                   .GetContainerReference(Config.ContainerName);

            containerRef.CreateIfNotExists();

            return getBlobRef(containerRef)(NewGuid());
        });

        static readonly Lazy<CloudBlockBlob> StandardBlockBlob =
            GetBlockBlobReference(StandardBlobConfig.ConnectionString);

        static readonly Lazy<CloudPageBlob> PremiumPageBlob =
            GetBlobReference<CloudPageBlob>(PremiumBlobConfig.ConnectionString, c => c.GetPageBlobReference);

        static readonly Lazy<CloudBlockBlob> PremiumBlockBlob =
            GetBlockBlobReference(PremiumBlockBlobConfig.ConnectionString);

        static readonly Lazy<CloudBlockBlob> AdlsG2BlobApi =
            GetBlockBlobReference(AdlsG2Config.ConnectionString);

        static readonly string SmbFilePath =
            Path.Combine(SmbConfig.FileLocation, NewGuid());

        [Benchmark]
        public Task SmbFileWrite() =>
            File.WriteAllBytesAsync(SmbFilePath, RandomData);

        public static int[] IOConcurrency { get; } = { 1, 11, 54 };

        [Benchmark]
        [ArgumentsSource(nameof(IOConcurrency))]
        public Task StandardBlockBlobUpload(int concurrency) =>
            StandardBlockBlob.Value.WriteRandomDataUploadAsync(concurrency);

        [Benchmark]
        public Task StandardBlockBlobPutBlock() =>
            StandardBlockBlob.Value.WriteRandomDataPutBlockAsync();

        [Benchmark]
        [ArgumentsSource(nameof(IOConcurrency))]
        public Task PremiumPageBlobUpload(int concurrency) =>
            PremiumPageBlob.Value.WriteRandomDataUploadAsync(concurrency);

        [Benchmark]
        public Task PremiumBlockBlobPutBlock() =>
            PremiumBlockBlob.Value.WriteRandomDataPutBlockAsync();

        [Benchmark]
        [ArgumentsSource(nameof(IOConcurrency))]
        public Task PremiumBlockBlobUpload(int concurrency) =>
            PremiumBlockBlob.Value.WriteRandomDataUploadAsync(concurrency);

        [Benchmark]
        public Task AdlsBlockBlobPutBlock() =>
            AdlsG2BlobApi.Value.WriteRandomDataPutBlockAsync();

        [Benchmark]
        [ArgumentsSource(nameof(IOConcurrency))]
        public Task AdlsBlockBlobUpload(int concurrency) =>
            AdlsG2BlobApi.Value.WriteRandomDataUploadAsync(concurrency);

        static readonly List<Func<Task>> _cleanUpTasks = new List<Func<Task>>
        {
            () => Task.Run(() => File.Delete(SmbFilePath)),
            () => StandardBlockBlob.Value.DeleteIfExistsAsync(),
            () => PremiumBlockBlob.Value.DeleteIfExistsAsync(),
            () => PremiumPageBlob.Value.DeleteIfExistsAsync(),
            () => AdlsG2BlobApi.Value.DeleteIfExistsAsync()
        };

        [GlobalSetup]
        public void GlobalSetup()
        {
            new Random(unchecked((int)DateTime.UtcNow.Ticks)).NextBytes(RandomData);
            PremiumPageBlob.Value.DeleteIfExists();
            PremiumBlockBlob.Value.DeleteIfExists();
            StandardBlockBlob.Value.DeleteIfExists();
            AdlsG2BlobApi.Value.DeleteIfExists();
            // Gen1 API not supported by Gen2, try using REST API as no SDK is available for Gen2
            //var _ = AdlsG1Api.Value;
        }

        [GlobalCleanup]
        public void GlobalCleanup() =>
            _cleanUpTasks.Select(t => t())
                         .WhenAll()
                         .GetAwaiter()
                         .GetResult();

    }

    static class Program
    {
        internal static void Main() =>
            BenchmarkRunner.Run<StorageWriteTest>();

        internal static string NewGuid() =>
            Guid.NewGuid().ToString("N");

        internal static Task WhenAll(this IEnumerable<Task> tasks) =>
            Task.WhenAll(tasks);

        internal static Task WriteRandomDataUploadAsync(this ICloudBlob blob, int concurrency) =>
            blob.UploadFromStreamAsync(new MemoryStream(StorageWriteTest.RandomData),
                                       default,
                                       new BlobRequestOptions
                                       {
                                           ParallelOperationThreadCount = concurrency,
                                           DisableContentMD5Validation = true,
                                           RequireEncryption = false,
                                           StoreBlobContentMD5 = false,
                                           UseTransactionalMD5 = false
                                       },
                                       default);

        static readonly List<string> _blockIds =
            Enumerable.Range(0, Config.BlockCount)
                      .Select(i => Convert.ToBase64String(Encoding.UTF8.GetBytes(i.ToString("d6"))))
                      .ToList();

        static readonly byte[] _dataBlock =
            StorageWriteTest.RandomData[0..(Config.RandomDataSizeBytes / Config.BlockCount)];

        internal static async Task WriteRandomDataPutBlockAsync(this CloudBlockBlob blob)
        {
            await _blockIds.Select(block => Task.Run(() => blob.PutBlockAsync(block,
                                                                              new MemoryStream(_dataBlock),
                                                                              null)))
                           .WhenAll();

            await blob.PutBlockListAsync(_blockIds);
        }
    }
}