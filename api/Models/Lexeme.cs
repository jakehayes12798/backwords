namespace Backwords.Api.Models;

public class Lexeme
{
    public int Id { get; set; }
    public required string Term { get; set; }
    public required string Language { get; set; }
    public bool IsSeedWord { get; set; }
}
