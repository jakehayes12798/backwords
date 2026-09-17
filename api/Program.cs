using System.Globalization;
using Backwords.Api.Models;
using CsvHelper;

using var reader = new StreamReader(@"..\data\raw\etymology.csv");
using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
var rows = csv.GetRecords<EtymologyDbRow>().ToList();

Console.WriteLine($"Read {rows.Count} rows from CSV file.");

// create a dictionary where the key is a tuple of (Term, Lang) and the value is a list of EtymologyDbRow objects, so we can lookup by
// term and language and get all the related terms for that term in that language
var etymologyDict = rows.GroupBy(r => (r.Term, r.Lang)).ToDictionary(g => g.Key, g => g.ToList());

Console.WriteLine($"Created dictionary with {etymologyDict.Count} unique (Term, Lang) pairs.");

foreach (var row in etymologyDict)
{
    var (term, lang) = row.Key;

    if (!term.Equals("algorithm", StringComparison.OrdinalIgnoreCase))
    {
        continue;
    }

    var relatedTerms = row.Value;

    Console.WriteLine($"Term: {term}, Lang: {lang}");
    foreach (var relatedTerm in relatedTerms)
    {
        Console.WriteLine(
            $"  Related Term: {relatedTerm.RelatedTerm}, Related Lang: {relatedTerm.RelatedLang}, Relation Type: {relatedTerm.RelType}"
        );
    }
}

Console.WriteLine("Terminated.");
