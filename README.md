# The Legend of Bum-bo: Windfall
The Legend of Bum-bo: Windfall is a mod for the Legend of Bum-bo that fixes bugs, adds new features, and adjusts game balance.

To see mod updates and change lists, visit the [Releases](https://github.com/Shpim/The-Legend-of-Bum-bo-Windfall/releases) page.

The Legend of Bum-bo does not have official modding support. This mod requires BepInEx, a modding framework that allows for patching games made in the Unity engine.

## Features
* Win streak counter
* Ability to rewatch cutscenes from the main menu
* Improvements to the save and continue system for in progress runs
* Many balance changes, and the option to disable them
* Fixes for over 100 bugs, including...
  * Achievements not unlocking
  * Unlock popups displaying the wrong text
  * Price Tag (and other spells) softlocking the game
  * The user interface breaking in a variety of situations
  * And many other bugs!

## Installation
If you already have BepInEx 5.4 installed for The Legend of Bum-bo, skip steps 1-3.

1. Download `BepInEx_x64_5.4.21.0.zip` from the [BepInEx 5.4.21 release page](https://github.com/BepInEx/BepInEx/releases/tag/v5.4.21).
2. Extract the contents of `BepInEx_x64_5.4.21.0.zip` into The Legend of Bum-bo root folder. You can find the root folder by clicking `Manage > Browse local files` in the options menu on the game's page in your Steam library.
3. Run the game once. This generates BepInEx configuration files.
4. Download `The.Legend.of.Bum-bo_Windfall.zip` from the [Releases](https://github.com/Shpim/The-Legend-of-Bum-bo-Windfall/releases) page.
5. Extract the contents of `The.Legend.of.Bum-bo_Windfall.zip` into the `Bepinex/Plugins` directory in the game folder. Replace files if prompted.

To update the mod, repeat steps 4 and 5 with the new mod release.

To uninstall the mod, remove `The Legend of Bum-bo_Windfall` folder from `Bepinex/Plugins`.

## Disclaimers
The Legend of Bum-bo: Windfall is made for the Steam version of the vanilla game. It has not been tested with the GOG or Epic Games versions, and might not work properly with them.

Text that is added or modified by The Legend of Bum-bo: Windfall is not translated and will only display in the English language.

Credit to Jasper Flick for anti-aliasing post processing assets used in the mod (catlikecoding.com).

## Bug Reports
If you encounter a bug, please report it by opening an issue on the [Issues](https://github.com/Shpim/The-Legend-of-Bum-bo-Windfall/issues) page or by contacting me (see below).
If possible, try to submit images and/or video demonstrating the bug.

You can also view the console log while the game is running by editing the text file entitled `Bepinex` in the `Bepinex/config` directory and changing `Enabled` to `true` under `[Logging.Console]`. If an error occurs, it will show up in red text in the console.
## Contact
Have questions or comments? Send me an Email at jeff.adriaanse@gmail.com or contact me through Discord at Shpim#0573.
