namespace ImgurPortable.Entities
{
    public enum StatusCode
    {
        Success = 200,
        MissingParameter = 400,
        AuthenticationRequired = 401,
        Forbidden = 403,
        MissingResource = 404,
        RateLimited = 429,
        ImgurIssue = 500
    }
}
