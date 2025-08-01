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
        private static readonly SpotifyController Spotify = new();
        private static ISettingsInterface? _settingsInterface;
        
        static void Main(string[] args)
        {
            _settingsInterface = new ConfigurationBuilder<ISettingsInterface>()
                .UseJsonFile("settings.json")
                .Build();
            
            Console.WriteLine($"Loaded settings:\n" +
                              $"- Buy Phase Volume: {_settingsInterface.BuyPhaseVolume}\n" +
                              $"- In-Game Phase Volume: {_settingsInterface.InGamePhaseVolume}\n" +
                              $"- Set Volume When Dead: {_settingsInterface.SetVolumeWhenDead}\n" +
                              $"    - Dead Volume: {_settingsInterface.DeadVolume}\n" +
                              $"- Set Volume When Round Ends: {_settingsInterface.SetVolumeWhenRoundEnds}\n" +
                              $"    - Round End Volume: {_settingsInterface.RoundEndVolume}\n" +
                              $"- Set Volume When Bomb Planted: {_settingsInterface.SetVolumeWhenBombPlanted}\n" +
                              $"    - Bomb Plant Volume: {_settingsInterface.BombPlantVolume}\n\n" +
                              $"Restart SpotifyCSG to apply new changes to the config file.\n");

            _ = Spotify.CreateSpotify();
            
            Console.WriteLine("Listening to CS:GO GameState events. Press Escape to exit.");
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
        
        private static void OnNewGameState(GameState gameState)
        {
            var vol = -1;
            if (_settingsInterface!.SetVolumeWhenDead && gameState.Player.State.Health == 0)
            {
                vol = _settingsInterface.DeadVolume;
            } else if (gameState.Round.Phase is Phase.Freezetime)
            {
                vol = _settingsInterface.BuyPhaseVolume;
            } else if (gameState.Round.Phase is Phase.Live)
            {
                vol = _settingsInterface.InGamePhaseVolume;
            }
            
            if (_settingsInterface.SetVolumeWhenRoundEnds && gameState.Round.Phase is Phase.Over)
            {
                vol = _settingsInterface.RoundEndVolume;
            }
            if (_settingsInterface.SetVolumeWhenBombPlanted && gameState.Bomb.State is BombState.Planted)
            {
                vol = _settingsInterface.BombPlantVolume;
            }

            if (vol == -1) return;
            _ = Spotify.TrySetVolume(vol);
        }
    }
}
