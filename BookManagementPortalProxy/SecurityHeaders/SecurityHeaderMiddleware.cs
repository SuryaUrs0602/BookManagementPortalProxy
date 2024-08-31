namespace BookManagementPortalProxy.SecurityHeaders
{
    public class SecurityHeaderMiddleware
    {
        private readonly RequestDelegate _requestDelegate;

        public SecurityHeaderMiddleware(RequestDelegate requestDelegate)
        {
            _requestDelegate = requestDelegate;
        }

        public async Task Invoke(HttpContext context)
        {
            context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
            context.Response.Headers.Add("X-Frame-Options", "DENY");
            context.Response.Headers.Add("Strict-Tansport-Security", "max-age=31536000; includeSubDomains");
            context.Response.Headers.Add("Content-Security-Policy", "default-src 'self'; script-src 'self' ; style-src 'self'; img-src 'self'");
            

            await _requestDelegate(context);
        }
    }
}

//          Context.Response.Headers.Add("Content-Security-Policy", "default-src 'self';" +
//                                       "script-src 'self' example.com *.example.com;" +
//                                       "img-src 'self' (image url)");
