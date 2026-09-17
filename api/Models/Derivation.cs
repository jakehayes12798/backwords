namespace Backwords.Api.Models;

public class Derivation
{
    public int Id { get; set; }
    public required int TermId { get; set; }
    public required Lexeme Term { get; set; }
    public int? RelatedTermId { get; set; }
    public Lexeme? RelatedTerm { get; set; }
    public required string RelationType { get; set; }
    public int? Position { get; set; }
}
