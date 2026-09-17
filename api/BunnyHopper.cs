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
        return rowsForWord
            // remove any rows that don't have a related term, since those can't be hops
            .Where(row => !string.IsNullOrWhiteSpace(row.RelatedTerm))
            // pick the first row that has a chain-type relation, if any
            .FirstOrDefault(row => ChainRelationTypes.Contains(row.RelType));
    }

    /// <summary>
    /// Given all rows for one (term, lang) pair, picks the rows that represent
    /// "sideways hops" — i.e. relations that are not part of the primary chain.
    /// </summary>
    /// <param name="rowsForWord"></param>
    /// <returns></returns>
    public static List<EtymologyDbRow> PickSidewaysHops(List<EtymologyDbRow> rowsForWord)
    {
        return rowsForWord
            // remove any rows that don't have a related term, since those can't be hops
            .Where(row => !string.IsNullOrWhiteSpace(row.RelatedTerm))
            // keep every row whose relation type is NOT one of the chain-following types —
            // these are the "see related" callouts, not the primary lineage spine
            .Where(row => !ChainRelationTypes.Contains(row.RelType))
            .DistinctBy(row => (row.RelatedTerm, row.RelatedLang))
            .ToList();
    }

    public static List<EtymologyDbRow> BuildHopChain(
        Dictionary<(string Term, string Lang), List<EtymologyDbRow>> etymologyDict,
        List<EtymologyDbRow> rowsForWord
    )
    {
        var chain = new List<EtymologyDbRow>();
        var currentRows = rowsForWord;

        while (currentRows != null && currentRows.Count > 0)
        {
            var hop = PickPrimaryHop(currentRows);
            if (hop == null)
            {
                break;
            }

            chain.Add(hop);

            var nextKey = (hop.RelatedTerm, hop.RelatedLang);
            etymologyDict.TryGetValue(nextKey, out currentRows);
        }

        return chain;
    }
}
