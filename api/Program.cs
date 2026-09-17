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

        foreach (var currentKey in SeedWords.All)
        {
            etymologyDict.TryGetValue(currentKey, out var currentRows);

            var hops = BunnyHopper.BuildHopChain(
                etymologyDict,
                currentRows ?? new List<EtymologyDbRow>()
            );

            foreach (var hop in hops)
            {
                Console.WriteLine(
                    $"Hop: {hop.Term} ({hop.Lang}) -> {hop.RelatedTerm} ({hop.RelatedLang})"
                );
            }
        }

        Console.WriteLine("Terminated.");
    }
}
