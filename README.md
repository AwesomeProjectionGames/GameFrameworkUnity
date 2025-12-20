# Unity Game Framework Implementations
A collection of systems that implement unified game interface from [Game Framework](https://github.com/AwesomeProjectionGames/GameFramework) in Unity.

## Modules implemented
- **Spectating**: A spectate system implementation for GameFramework. Provides controllers and components for managing spectating functionality (UIs elements of a certain actor) and camera management.
- **Spawn Point**: An implementation of Spawn Points using the Game Framework interfaces.

## Installation
To install these modules, you can use the Unity Package Manager. 
To do so, open the Unity Package Manager and click on the `+` button in the top left corner. 
Then select `Add package from git URL...` and enter the following URLs (only the ones you need):

```
https://github.com/AwesomeProjectionGames/UnityGameFrameworkImplementations.git?path=/Spectating
https://github.com/AwesomeProjectionGames/UnityGameFrameworkImplementations.git?path=/SpawnPoint
```

Or you can manually add the following line to your `manifest.json` file located in your project's `Packages` directory.

```json
{
  "dependencies": {
    "com.awesomeprojection.gameframework.spectating": "https://github.com/AwesomeProjectionGames/UnityGameFrameworkImplementations.git?path=/Spectating",
    "com.awesomeprojection.gameframework.spawnpoint": "https://github.com/AwesomeProjectionGames/UnityGameFrameworkImplementations.git?path=/SpawnPoint",
  }
}
```