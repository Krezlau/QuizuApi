using Microsoft.Extensions.Primitives;
using System.IdentityModel.Tokens.Jwt;

namespace QuizuApi.Services
{
    public class AccessTokenReaderService : IAccessTokenReaderService
    {
        public string ReadUserId(string jwtToken)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.ReadJwtToken(jwtToken);
            var userIdClaim = token.Claims.FirstOrDefault(c => c.Type == "nameid");

            if (userIdClaim is null)
            {
                throw new Exception("Invalid access token.");
            }
            return userIdClaim.Value;
        }

        public string? RetrieveUserIdFromRequest(HttpRequest request)
        {
            if (request.Headers.ContainsKey("Authorization"))
            {
                if (request.Headers.TryGetValue("Authorization", out StringValues values))
                {
                    var jwt = values.ToString();

                    if (jwt.Contains("Bearer"))
                    {
                        jwt = jwt.Replace("Bearer", "").Trim();
                    }

                    try
                    {
                        return ReadUserId(jwt);
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                }
            }
            return null;
        }
    }
}
