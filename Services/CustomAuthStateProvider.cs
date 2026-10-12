using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Security.Claims;

namespace AeroSync.Services
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {

        public CustomAuthStateProvider()
        {
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                // Blazor will attempt this. If JS isn't ready (prerendering), it throws an error.
                var gettoken = await SecureStorage.GetAsync("paseto_token");

                if (string.IsNullOrWhiteSpace(gettoken))
                {
                    return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
                }

                var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "Authority") }, "paseto");
                var user = new ClaimsPrincipal(identity);

                return new AuthenticationState(user);
            }
            catch
            {
                // Failsafe for Prerendering: Return an empty, anonymous user state so the app doesn't crash.
                // Once the app fully loads in the browser, Blazor will automatically call this method again.
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
        }

        public void NotifyUserAuthentication(string token,string UserRole)
        {
            var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, UserRole) }, "paseto");
            var authenticatedUser = new ClaimsPrincipal(identity);
            var authState = Task.FromResult(new AuthenticationState(authenticatedUser));

            // This triggers the UI to re-render and grant access
            NotifyAuthenticationStateChanged(authState);
        }

        public void NotifyUserLogout()
        {
            var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
            var authState = Task.FromResult(new AuthenticationState(anonymousUser));
            NotifyAuthenticationStateChanged(authState);
        }
    }
}
