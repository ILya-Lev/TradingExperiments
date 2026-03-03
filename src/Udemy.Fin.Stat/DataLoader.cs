using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Parquet;
using Parquet.Serialization;

namespace Udemy.Fin.Stat;

public static class DataLoader
{
    private const int BufferSize = 8096;

    private static readonly FileStreamOptions _readingFileOptions = new FileStreamOptions()
    {
        Access = FileAccess.Read,
        BufferSize = BufferSize,
        Mode = FileMode.Open,
        Options = FileOptions.Asynchronous | FileOptions.SequentialScan,
        Share = FileShare.Read,
    };

    private static readonly FileStreamOptions _writingFileOptions = new FileStreamOptions()
    {
        Access = FileAccess.Write,
        BufferSize = BufferSize,
        Mode = FileMode.Create,
        Options = FileOptions.Asynchronous | FileOptions.SequentialScan,
        Share = FileShare.Read,
    };

    private static readonly ParquetSerializerOptions _parquetOptions = new ()
    {
        CompressionLevel = CompressionLevel.SmallestSize,
        Append = false,
        CompressionMethod = CompressionMethod.Zstd,
        PropertyNameCaseInsensitive = true,
        RowGroupSize = 10_000,
        ParquetOptions = new ParquetOptions()
        {
            UseDateOnlyTypeForDates = true,
            UseBigDecimal = false,
        }
    };

    private static readonly CsvConfiguration _csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture)
    {
        HasHeaderRecord = true,
        Delimiter = ",",
        BufferSize = BufferSize,
        //ReadingExceptionOccurred = (ReadingExceptionOccurredArgs args) =>
        //{
        //    var record = args.Exception.Context?.Parser?.Record;
        //    args.Record = default(T);
        //    return false;
        //}
    };

    public static async IAsyncEnumerable<T> LoadCsv<T>(string path, [EnumeratorCancellation] CancellationToken ct = default)
    {
        await using var fileStream = File.Open(path, _readingFileOptions);
        using var textReader = new StreamReader(fileStream, Encoding.UTF8);
        using var csv = new CsvReader(textReader, _csvConfiguration, leaveOpen: false);

        await foreach(var record in csv.GetRecordsAsync<T>(ct))
            yield return record;
    }

    public static async Task DumpCsv<T>(IEnumerable<T> data, string path, CancellationToken ct = default)
    {
        await using var fileStream = File.Open(path, _writingFileOptions);
        await using var textWriter = new StreamWriter(fileStream, Encoding.UTF8);
        await using var csv = new CsvWriter(textWriter, _csvConfiguration, leaveOpen: false);
        csv.WriteHeader<T>();
        await csv.WriteRecordsAsync(data, ct);
    }

    public static async IAsyncEnumerable<T> LoadParquet<T>(string path, [EnumeratorCancellation] CancellationToken ct = default) 
        where T : class, new()
    {
        await using var fileStream = File.Open(path, _readingFileOptions);

        await foreach (var record in ParquetSerializer.DeserializeAllAsync<T>(fileStream, _parquetOptions, ct))
            yield return record;
    }

    public static async Task DumpParquet<T>(IEnumerable<T> data, string path, CancellationToken ct = default)
    {
        await using var fileStream = File.Open(path, _writingFileOptions);
        var schema = await ParquetSerializer.SerializeAsync(data, fileStream, _parquetOptions, ct);
    }
}

