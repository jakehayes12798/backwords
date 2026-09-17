namespace Backwords.Api;

using Backwords.Api.Models;

public static class BunnyHopper
{
    private static readonly HashSet<string> ChainRelationTypes = new(
        StringComparer.OrdinalIgnoreCase
    )
    {
        "inherited_from",
        "borrowed_from",
        "derived_from",
        "root",
    };

    /// <summary>
    /// Given all rows for one (term, lang) pair, picks the single row that represents
    /// the primary next hop backward in the etymology chain — or null if this word
    /// has no chain-type relation (i.e. it's a root with nothing further to trace).
    /// </summary>
    public static EtymologyDbRow? PickPrimaryHop(List<EtymologyDbRow> rowsForWord)
    {
        return rowsForWord.FirstOrDefault(row => ChainRelationTypes.Contains(row.RelType));
    }
}
