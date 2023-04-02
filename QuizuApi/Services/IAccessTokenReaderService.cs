namespace QuizuApi.Services
{
    public interface IAccessTokenReaderService
    {
        string ReadUserId(string jwtToken);
        string? RetrieveUserIdFromRequest(HttpRequest request);
    }
}
