using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text.Encodings.Web;

namespace final_project_back_end_akostryba
{
    public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public BasicAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
            {
                Console.WriteLine($"Missing Authorization header. Request Path: {Request.Path}, Method: {Request.Method}");
                return AuthenticateResult.Fail("Missing Authorization Header");
            }

            string username = String.Empty;
            string password = String.Empty;
            string base64Credentials;
            string hashCreds = String.Empty;
            try
            {
                var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
                var credentialBytes = Convert.FromBase64String(authHeader.Parameter);
                base64Credentials = credentialBytes.ToString();
                var credentials = System.Text.Encoding.UTF8.GetString(credentialBytes).Split(':');
                username = credentials[0];
                password = credentials[1];
            }
            catch
            {
                return AuthenticateResult.Fail("Error decrypting login");
            }

            using (var db = new ChirperContext())
            {
                var login = db.Users.FirstOrDefault(l => l.username == username);
                if (login == null)
                {
                    return await Task.FromResult(AuthenticateResult.Fail("Invalid username"));
                }
                if (login.password != password)
                {
                    return await Task.FromResult(AuthenticateResult.Fail("Invalid password"));
                }
                // if(login.base64Credentials != base64Credentials){
                //     return await Task.FromResult(AuthenticateResult.Fail("Invalid credentials"));
                // }

                var claims = new System.Security.Claims.Claim[]{
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, username)
                };
                var identity = new System.Security.Claims.ClaimsIdentity(claims, Scheme.Name);
                var principal = new System.Security.Claims.ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, Scheme.Name);
                return AuthenticateResult.Success(ticket);
            }
        }
    }
}