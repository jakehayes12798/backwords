using CsvHelper.Configuration.Attributes;

namespace Backwords.Api.Models;

/// <summary>
/// Represents a row in the etymology CSV file.
/// </summary>
public class EtymologyDbRow
{
    [Name("term_id")]
    public string TermId { get; set; }

    [Name("lang")]
    public string Lang { get; set; }

    [Name("term")]
    public string Term { get; set; }

    [Name("reltype")]
    public string RelType { get; set; }

    [Name("related_term_id")]
    public string RelatedTermId { get; set; }

    [Name("related_lang")]
    public string RelatedLang { get; set; }

    [Name("related_term")]
    public string RelatedTerm { get; set; }

    [Name("position")]
    public int? Position { get; set; }

    [Name("group_tag")]
    public string GroupTag { get; set; }

    [Name("parent_tag")]
    public string ParentTag { get; set; }

    [Name("parent_position")]
    public int? ParentPosition { get; set; }
}
