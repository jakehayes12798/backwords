namespace Backwords.Api.Models;

public class LineageNode
{
    public required string Term { get; set; }
    public required string Language { get; set; }

    /// <summary>
    /// How this word relates to its ancestor (e.g. "inherited_from"), or null
    /// if this is the starting seed word with no ancestor.
    /// </summary>
    public string? RelationToAncestor { get; set; }

    public LineageNode? Ancestor { get; set; }

    /// <summary>
    /// Sideways relations — terms related to THIS word but not part of the
    /// primary chain (cognates, doublets, etc.). Not full nodes: just enough
    /// to render a "see related" callout and know whether it's clickable.
    /// </summary>
    public List<SidewaysRelation> Related { get; set; } = new();

    /// <summary>
    /// Returns a string representation of the lineage node, including its term, language,
    /// relation to ancestor, and any related terms. This is useful for debugging and logging.
    /// </summary>
    /// <returns>The string representation of the lineage node.</returns>
    public override string ToString()
    {
        var relatedStr =
            Related.Count > 0
                ? $" [see related: {string.Join(", ", Related.Select(r => $"{r.Term} ({r.Language})"))}]"
                : "";
        var ancestorStr = Ancestor != null ? $" -> {Ancestor}" : "";
        return $"{Term} ({Language}){relatedStr}{ancestorStr}";
    }
}

public class SidewaysRelation
{
    public required string Term { get; set; }
    public required string Language { get; set; }
    public required string RelationType { get; set; }
    public bool InCollection { get; set; }
}
