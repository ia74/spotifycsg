# SpotifyCSG

Dynamically adjust your Spotify volume based on the state of your CS:GO active game.

This uses the Spotify Web API and the CS:GO GameState Integration.

## Features

- Configuration file included (`spotifycsg_settings.json`) to adjust the volume levels for different game states.
- You can set the volume for:
  - Buy Phase
    - This is when the game is frozen and players can buy weapons and equipment.
  - In Game
    - This is while the round is fully active
  - Death
    - This is when you are dead and waiting for the next round to start.
  - Round End
    - This is where the announcer states "(Side) wins", and the game is transitioning to the next round.

## Usage

Download the latest release from the [Releases](https://github.com/ia74/SpotifyCSG/releases) page.

Unzip the downloaded file and run `SpotifyCSG.exe`.