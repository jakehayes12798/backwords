namespace Backwords.Api.Models;

/// <summary>
/// Represents a dictionary of etymology rows, organized by term and language.
/// </summary>
/// <param name="allRows">The list of etymology rows to initialize the dictionary with.</param>
public class EtymologyDictionary(List<EtymologyDbRow> allRows)
{
    private readonly Dictionary<
        (string Term, string Language),
        List<EtymologyDbRow>
    > _rowsByTermAndLanguage = allRows
        .GroupBy(r => (r.Term, r.Lang))
        .ToDictionary(g => g.Key, g => g.ToList());

    public List<EtymologyDbRow> GetRows(string term, string language) =>
        _rowsByTermAndLanguage.TryGetValue((term, language), out var rows)
            ? rows
            : new List<EtymologyDbRow>();

    public bool Contains(string term, string language) =>
        _rowsByTermAndLanguage.ContainsKey((term, language));
}
