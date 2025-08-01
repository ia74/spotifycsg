using System.ComponentModel;
using Config.Net;

namespace SpotifyCSG;

public interface ISCSGSettings
{
    [DefaultValue(75)]
    int BuyPhaseVolume { get; }
    [DefaultValue(25)]
    int InGamePhaseVolume { get; }
    
    [DefaultValue(false)]
    bool SetVolumeWhenDead { get; }
    [DefaultValue(50)]
    int DeadVolume { get; }
    
    [DefaultValue(false)]
    bool SetVolumeWhenRoundEnds { get; }
    [DefaultValue(50)]
    int RoundEndVolume { get; }
    
    [DefaultValue(false)]
    bool SetVolumeWhenBombPlanted { get; }
    [DefaultValue(10)]
    int BombPlantVolume { get; }
}