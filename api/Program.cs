namespace Backwords.Api;

using System.Globalization;
using Backwords.Api.Models;
using CsvHelper;

public static class Program
{
    public static void Main(string[] args)
    {
        using var reader = new StreamReader(@"..\data\raw\etymology.csv");
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        var rows = csv.GetRecords<EtymologyDbRow>().ToList();

        Console.WriteLine($"Read {rows.Count} rows from CSV file.");

        // create a dictionary where the key is a tuple of (Term, Lang) and the value is a list of EtymologyDbRow objects, so we can lookup by
        // term and language and get all the related terms for that term in that language
        var etymologyDict = rows.GroupBy(r => (r.Term, r.Lang))
            .ToDictionary(g => g.Key, g => g.ToList());

        Console.WriteLine(
            $"Created dictionary with {etymologyDict.Count} unique (Term, Lang) pairs."
        );

        var currentKey = ("algorithm", "English");
        etymologyDict.TryGetValue(currentKey, out var currentRows);

        while (currentRows != null)
        {
            var hop = BunnyHopper.PickPrimaryHop(currentRows);

            if (hop is null)
            {
                Console.WriteLine(
                    $"{currentKey.Item1} ({currentKey.Item2}) is a root — no further hop."
                );
                break;
            }

            if (currentRows == null)
            {
                Console.WriteLine(
                    $"{currentKey.Item1} ({currentKey.Item2}) has no data in etymology-db — chain ends here (data gap, not necessarily a linguistic root)."
                );
                break;
            }

            Console.WriteLine(
                $"{currentKey.Item1} ({currentKey.Item2}) --[{hop.RelType}]--> {hop.RelatedTerm} ({hop.RelatedLang})"
            );

            currentKey = (hop.RelatedTerm, hop.RelatedLang);
            etymologyDict.TryGetValue(currentKey, out currentRows);
        }

        Console.WriteLine("Terminated.");
    }
}
