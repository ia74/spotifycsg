using System.Diagnostics;
using System.Net;
using SpotifyAPI.Web;

namespace SpotifyCSG;

public class SCSGHTTP
{
    public async void StartHTTP()
    {
        HttpListener listener = new HttpListener();
        listener.Prefixes.Add("http://127.0.0.1:5543/");
        listener.Start();
        var (verifier, challenge) = PKCEUtil.GenerateCodes(120);

        var loginRequest = new LoginRequest(
            new Uri("http://127.0.0.1:5543/"),
            "10e5c1cbbad546e39ef558e1aa4c03f4",
            LoginRequest.ResponseType.Code
        )
        {
            CodeChallengeMethod = "S256",
            CodeChallenge = challenge,
            Scope = new[] { Scopes.UserModifyPlaybackState }
        };
        var uri = loginRequest.ToUri();
            
        Console.WriteLine("url:");
        Process.Start(new ProcessStartInfo { FileName = uri.ToString(), UseShellExecute = true });
        Console.WriteLine(uri);
        
        while (true)
        {
            HttpListenerContext context = await listener.GetContextAsync();
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;
            
            if (request.HttpMethod == "GET" && request.QueryString["code"] != null)
            {
                string code = request.QueryString["code"]!;
                
                var initialResponse = await new OAuthClient().RequestToken(
                    new PKCETokenRequest("10e5c1cbbad546e39ef558e1aa4c03f4", code, new Uri("http://127.0.0.1:5543"), verifier)
                );
                var authenticator = new PKCEAuthenticator("10e5c1cbbad546e39ef558e1aa4c03f4", initialResponse);

                var config = SpotifyClientConfig.CreateDefault()
                    .WithAuthenticator(authenticator);
                Program.spotify = new SpotifyClient(config);
                Console.WriteLine("hi " + Program.spotify.UserProfile.Current().Result.DisplayName);
                
                // Send a response back to the browser
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes("Authentication successful! You can close this window.");
                response.ContentLength64 = buffer.Length;
                response.OutputStream.Write(buffer, 0, buffer.Length);
            }
            else
            {
                response.StatusCode = 400; // Bad Request
            }

            response.Close();
        }
    }
}