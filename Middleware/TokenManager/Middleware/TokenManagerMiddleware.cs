//using Ihelpers.Middleware.TokenManager.Interfaces;
//using Microsoft.AspNetCore.Http;
//using System.Net;

//namespace Ihelpers.Middleware.TokenManager.Middleware
//{
//    /// <summary>
//    /// This class implements middleware that manages tokens.
//    /// </summary>
//    public class TokenManagerMiddleware : IMiddleware
//    {
//        private readonly ITokenManager _tokenManager;

//        /// <summary>
//        /// Initializes a new instance of the `TokenManagerMiddleware` class.
//        /// </summary>
//        /// <param name="tokenManager">The token manager to use for managing tokens.</param>
//        public TokenManagerMiddleware(ITokenManager tokenManager)
//        {
//            _tokenManager = tokenManager;
//        }

//        /// <summary>
//        /// Invokes the middleware.
//        /// </summary>
//        /// <param name="context">The HTTP context.</param>
//        /// <param name="next">The next middleware in the pipeline.</param>
//        /// <returns>A task that represents the asynchronous operation.</returns>
//        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
//        {
//            // Check if the current token is active
//            bool isActiveToken = await _tokenManager.IsCurrentActiveToken();

//            // If the current token is active, pass the request to the next middleware in the pipeline
//            if (isActiveToken == true)
//            {
//                await next(context);
//                return;
//            }

//            // If the current token is not active, return a unauthorized status code
//            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
//        }
//    }
//}
