using System.Diagnostics;
using System.Net;
using SpotifyAPI.Web;

namespace SpotifyCSG;

public class SpotifyController
{
    private SpotifyClient? _spotifyClient;
    private readonly string _clientId = "10e5c1cbbad546e39ef558e1aa4c03f4";
    private readonly Uri _redirectUri = new Uri("http://127.0.0.1:5543/");

    public async Task TrySetVolume(int vol)
    {
        try
        {
            await _spotifyClient?.Player.SetVolume(new PlayerVolumeRequest(vol))!;
        }
        catch (Exception ignored)
        {
            Console.WriteLine(ignored.Message);
        }
    }

    public async Task CreateSpotify()
    {
        HttpListener listener = new HttpListener();
        listener.Prefixes.Add(_redirectUri.ToString());
        listener.Start();
        var (verifier, challenge) = PKCEUtil.GenerateCodes(120);
        var loginRequest = new LoginRequest(_redirectUri, _clientId, LoginRequest.ResponseType.Code)
        {
            CodeChallengeMethod = "S256", CodeChallenge = challenge, Scope = [Scopes.UserModifyPlaybackState]
        };
        Console.WriteLine($"Authorization for Spotify needed! If your browser doesn't open, manually open this link to sign in.\n{loginRequest.ToUri().ToString()}");
        Process.Start(new ProcessStartInfo { FileName = loginRequest.ToUri().ToString(), UseShellExecute = true });
        while (true)
        {
            var context = await listener.GetContextAsync();
            var request = context.Request;
            var response = context.Response;
            if (request.HttpMethod == "GET" && request.QueryString["code"] != null)
            {
                var code = request.QueryString["code"]!;
                var initialResponse = await new OAuthClient().RequestToken(
                    new PKCETokenRequest(_clientId, code, _redirectUri, verifier));
                var authenticator = new PKCEAuthenticator(_clientId, initialResponse);
                var config = SpotifyClientConfig.CreateDefault().WithAuthenticator(authenticator);
                _spotifyClient = new SpotifyClient(config);
                Console.WriteLine($"Hello {_spotifyClient.UserProfile.Current().Result.DisplayName}.");

                // Send a response back to the browser
                var buffer = "Authentication successful! You can close this window."u8.ToArray();
                response.ContentLength64 = buffer.Length;
                await response.OutputStream.WriteAsync(buffer);
                listener.Close();
                break;
            }

            response.StatusCode = 400; // Bad Request
            response.Close();
        }
    }
}