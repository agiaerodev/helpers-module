using System.IdentityModel.Tokens.Jwt;

namespace Ihelpers.Helpers
{
    /// <summary>
    /// Class that helps with Json Web Token strings (bearer) common operations.
    /// </summary>
    public static class JWTHelper
    {
        /// <summary>
        /// Gets the value of a specific claim in a JSON Web Token (JWT).
        /// </summary>
        /// <param name="token">The JWT to extract the claim from.</param>
        /// <param name="claimName">The name of the claim to extract.</param>
        /// <returns>The value of the specified claim, or `null` if the claim doesn't exist or an exception was thrown.</returns>
        public static async Task<string> getJWTTokenClaimAsync(string token, string claimName)
        {
            // Remove the "Bearer " prefix from the token if it exists
            token = token.Replace("Bearer ", string.Empty);

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var securityToken = (JwtSecurityToken)tokenHandler.ReadToken(token);
                var claimValue = securityToken.Claims.FirstOrDefault(c => c.Type == claimName)?.Value;
                return claimValue;
            }
            catch (Exception ex)
            {
                //TODO: Logger.Error
                return null;
            }
        }

        /// <summary>
        /// Gets the value of a specific claim in a JSON Web Token (JWT).
        /// </summary>
        /// <param name="token">The JWT to extract the claim from.</param>
        /// <param name="claimName">The name of the claim to extract.</param>
        /// <returns>The value of the specified claim, or `null` if the claim doesn't exist or an exception was thrown.</returns>
        public static string getJWTTokenClaim(string token, string claimName)
        {
            try
            {
                if (string.IsNullOrEmpty(token)) { return null; }

                // Remove the "Bearer " prefix from the token if it exists
                token = token.Replace("Bearer ", string.Empty);

                var tokenHandler = new JwtSecurityTokenHandler();
                var securityToken = (JwtSecurityToken)tokenHandler.ReadToken(token);
                var claimValue = securityToken.Claims.FirstOrDefault(c => c.Type == claimName)?.Value;
                return claimValue;
            }
            catch (Exception ex)
            {
                //TODO: Logger.Error
                return null;
            }
        }

    }

}
