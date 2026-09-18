namespace Backwords.Api.Models;

/// <summary>
/// Represents a node in the lineage tree of a term, including its term, language, relation to its ancestor, and any related terms.
/// </summary>
public class LineageNode
{
    /// <summary>
    /// The term represented by this lineage node.
    /// </summary>
    public required string Term { get; set; }

    /// <summary>
    /// The language of the term represented by this lineage node.
    /// </summary>
    public required string Language { get; set; }

    /// <summary>
    /// How this word relates to its ancestor (e.g. "inherited_from"), or null
    /// if this is the starting seed word with no ancestor.
    /// </summary>
    public string? RelationToAncestor { get; set; }

    /// <summary>
    /// The ancestor node in the lineage tree, representing the term from which this term is derived. This property is null for the root node of the lineage tree, which has no ancestor.
    /// </summary>
    public LineageNode? Ancestor { get; set; }

    /// <summary>
    /// Sideways relations — terms related to THIS word but not part of the
    /// primary chain (cognates, doublets, etc.). Not full nodes: just enough
    /// to render a "see related" callout and know whether it's clickable.
    /// </summary>
    public List<SidewaysRelation> Related { get; set; } = new();

    /// <summary>
    /// Represents whether a cycle was detected in the lineage tree. If true, it indicates that the current term has already been visited in the ancestry chain, preventing infinite recursion and indicating a circular relationship in the etymology data.
    /// </summary>
    public bool CycleDetected { get; set; } = false;

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
        return $"{Term} ({Language}){relatedStr}{ancestorStr} {(CycleDetected ? " [cycle detected]" : "")}";
    }
}

/// <summary>
/// Represents a sideways relation to a term that is related to the current term but not part of the primary lineage chain.
/// </summary>
/// <remarks>
/// This could be, for example, a cognate, doublet, or other related term that is not the primary ancestor of the current term.
/// </remarks>
public class SidewaysRelation
{
    /// <summary>
    /// The term that is related to the current term but not part of the primary lineage chain.
    /// </summary>
    public required string Term { get; set; }

    /// <summary>
    /// The language of the related term.
    /// </summary>
    public required string Language { get; set; }

    /// <summary>
    /// The type of relation between the current term and the related term (e.g., "cognate", "doublet", etc.).
    /// </summary>
    public required string RelationType { get; set; }

    /// <summary>
    /// Indicates whether the related term is part of the collection of terms being analyzed. This can be used to determine if the related term should be included in further analysis or processing.
    /// </summary>
    public bool InCollection { get; set; }
}
