namespace Backwords.Api;

using Backwords.Api.Models;

public static class LineageBuilder
{
    public static LineageNode BuildLineage(
        EtymologyDictionary etymologyDict,
        string term,
        string language,
        HashSet<(string Term, string Language)>? visited = null
    )
    {
        visited ??= [];
        var key = (term, language);

        var currentRows = etymologyDict.GetRows(term, language);
        var primaryHop = BunnyHopper.PickPrimaryHop(currentRows);
        var sidewaysHops = BunnyHopper.PickSidewaysHops(currentRows);

        var node = new LineageNode
        {
            Term = term,
            Language = language,
            RelationToAncestor = primaryHop?.RelType,
            Related = sidewaysHops
                .Select(row => new SidewaysRelation
                {
                    Term = row.RelatedTerm,
                    Language = row.RelatedLang,
                    RelationType = row.RelType,
                })
                .ToList(),
        };

        if (primaryHop != null)
        {
            var nextKey = (primaryHop.RelatedTerm, primaryHop.RelatedLang);

            if (visited.Contains(nextKey))
            {
                node.CycleDetected = true;
            }
            else
            {
                visited.Add(key);
                node.Ancestor = BuildLineage(
                    etymologyDict,
                    primaryHop.RelatedTerm,
                    primaryHop.RelatedLang,
                    visited
                );
            }
        }

        return node;
    }
}
