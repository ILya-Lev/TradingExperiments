using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace Udemy.Fin.Stat;

public static class DataLoader
{
    public static async IAsyncEnumerable<T> LoadCsv<T>
    (
        string path,
        [EnumeratorCancellation] CancellationToken ct = default
    )
        where T : struct
    {
        await using var fileStream = File.Open(path, new FileStreamOptions()
        {
            Access = FileAccess.Read,
            BufferSize = 8096,
            Mode = FileMode.Open,
            Options = FileOptions.Asynchronous | FileOptions.SequentialScan,
            Share = FileShare.Read,
        });

        using var textReader = new StreamReader(fileStream, Encoding.UTF8);
        var csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            Delimiter = ",",
            BufferSize = 8096,
            //ReadingExceptionOccurred = (ReadingExceptionOccurredArgs args) =>
            //{
            //    var record = args.Exception.Context?.Parser?.Record;
            //    args.Record = default(T);
            //    return false;
            //}
        };
        using var csv = new CsvReader(textReader, csvConfiguration, leaveOpen: false);

        await foreach(var record in csv.GetRecordsAsync<T>(ct))
            yield return record;
    }

    public static Task DumpCsv<T>(IEnumerable<T> data, string path)
    {
        throw new NotImplementedException();
    }

    public static IAsyncEnumerable<T> LoadParquet<T>
    (
        string path,
        CancellationToken ct = default
    )
    {
        throw new NotImplementedException();
    }

    public static Task DumpParquet<T>(IEnumerable<T> data, string path)
    {
        throw new NotImplementedException();
    }
}

