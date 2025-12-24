namespace GreenfieldCoreApi.ApiModels;

public class LinkPatreonAccountRequest
{
    public required string RefreshToken { get; set; }
    public required string AccessToken { get; set; }
    public required string TokenType { get; set; }
    public required DateTime TokenExpiry { get; set; }
    public required string Scope { get; set; }
    public decimal? Pledge { get; set; }
}

