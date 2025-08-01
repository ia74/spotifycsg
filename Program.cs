using Config.Net;
using CounterStrike2GSI;
using CounterStrike2GSI.EventMessages;
using CounterStrike2GSI.Nodes;
using SpotifyAPI.Web;
using SpotifyCSG;
using GameState = CounterStrike2GSI.GameState;

namespace SpotifyCSG
{
    class Program
    {
        static GameStateListener? _gsl;
        public static SpotifyClient spotify;
        public static ISCSGSettings settings;
        
        static void Main(string[] args)
        {
            Console.WriteLine("loading settings yo");
            settings = new ConfigurationBuilder<ISCSGSettings>()
                .UseJsonFile("settings.json")
                .Build();
            // Generates a secure random verifier of length 120 and its challenge
            Console.WriteLine("authenticatin spotify");
            SCSGHTTP scsgHttp = new SCSGHTTP();
            scsgHttp.StartHTTP();
            
            Console.WriteLine("listening to csgo events");
            _gsl = new GameStateListener(5758);
            if (!_gsl.GenerateGSIConfigFile("SpotifyCSG"))
            {
                Console.WriteLine("Could not generate GSI configuration file.");
            }
            
            _gsl.NewGameState += OnNewGameState;
            if (!_gsl.Start())
            {
                Console.WriteLine("GameStateListener could not start. Try running this program as Administrator. Exiting.");
                Console.ReadLine();
                Environment.Exit(0);
            }

            do
            {
                while (!Console.KeyAvailable)
                {
                    Thread.Sleep(1000);
                }
            } while (Console.ReadKey(true).Key != ConsoleKey.Escape);
            
        }

        public static async Task TrySetVolume(int vol)
        {
            try
            {
                await spotify.Player.SetVolume(new PlayerVolumeRequest(vol));
            }
            catch (APIException ignore)
            {
                Console.WriteLine(ignore);
                Console.WriteLine(ignore.Message);
            }
            catch (Exception ignored)
            {
                Console.WriteLine(ignored.Message);
            }
        }
        
        private static void OnNewGameState(GameState gamestate)
        {
            // Guaranteed to fire before CS2GameEvent events.
            int vol = -1;
            if (settings.SetVolumeWhenDead && gamestate.Player.State.Health == 0)
            {
                vol = settings.DeadVolume;
            } else if (gamestate.Round.Phase is Phase.Freezetime)
            {
                vol = settings.BuyPhaseVolume;
            } else if (gamestate.Round.Phase is Phase.Live)
            {
                vol = settings.InGamePhaseVolume;
            }
            
            if (settings.SetVolumeWhenRoundEnds && gamestate.Round.Phase is Phase.Over)
            {
                vol = settings.RoundEndVolume;
            }
            if (settings.SetVolumeWhenBombPlanted && gamestate.Bomb.State is BombState.Planted)
            {
                vol = settings.BombPlantVolume;
            }

            if (vol == -1) return;
            _ = TrySetVolume(vol);
        }
    }
}
