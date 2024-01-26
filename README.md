# The Legend of Bum-bo: Windfall
The Legend of Bum-bo: Windfall is a mod for the Legend of Bum-bo that fixes bugs, adds new features, and adjusts game balance.

To see mod updates and change lists, visit the [Releases](https://github.com/Shpim/The-Legend-of-Bum-bo-Windfall/releases) page.

The Legend of Bum-bo does not have official modding support. This mod requires BepInEx, a modding framework that allows for patching games made in the Unity engine.

## Features
* A new spell, Plasma Ball
* An enhanced tooltip system that displays information of all enemies and collectibles
* A status indicator system, which passively displays temporary effects that are currently influencing Bum-bo
* A win streak counter
* The ability to rewatch cutscenes from the main menu
* Improvements to the save and continue system for in progress runs
* Various other quality of life improvements
* Many [balance changes](https://github.com/Shpim/The-Legend-of-Bum-bo-Windfall/wiki/Balance-Changes), and the option to disable them
* Fixes for over 100 bugs, including...
  * Achievements not unlocking
  * Unlock popups displaying the wrong text
  * Price Tag softlocking the game
  * The user interface breaking in a variety of situations

## Installation
If you already have BepInEx 5.4 installed for The Legend of Bum-bo, skip steps 1-3.

1. Download `BepInEx_x64_5.4.21.0.zip` from the [BepInEx 5.4.21 release page](https://github.com/BepInEx/BepInEx/releases/tag/v5.4.21).
2. Extract the contents of `BepInEx_x64_5.4.21.0.zip` into The Legend of Bum-bo root folder. You can find the root folder by clicking `Manage > Browse local files` in the options menu on the game's page in your Steam library.
3. Run the game once. This generates BepInEx configuration files.
4. Download `The.Legend.of.Bum-bo_Windfall.zip` from the [Releases](https://github.com/Shpim/The-Legend-of-Bum-bo-Windfall/releases) page. The latest version of the mod is [v1.2.1.0](https://github.com/JeffAdriaanse/The-Legend-of-Bum-bo-Windfall/releases/tag/v1.2.1.0).
5. Extract the contents of `The.Legend.of.Bum-bo_Windfall.zip` directly* into the `BepInEx/Plugins` directory in the game folder. Replace files if prompted.

*For the mod to function properly, the `BepInEx/Plugins` directory must contain `The Legend of Bum-bo_Windfall` folder. `The Legend of Bum-bo_Windfall` folder must contain two files: `The Legend of Bum-bo_Windfall.dll` and `windfall`.

If you are having trouble installing BepInEx, consult the BepInEx [installation guide](https://docs.bepinex.dev/articles/user_guide/installation/index.html).

To update the mod, repeat steps 4 and 5 with the new mod release.

To uninstall the mod, remove `The Legend of Bum-bo_Windfall` folder from `BepInEx/Plugins`.
To uninstall BepInEx, remove the `BepInEx` folder itself.

## Additional Information
The Legend of Bum-bo: Windfall is made for the Steam and GOG versions of the vanilla game, although it most likely works with the Epic Games version as well.

Text that is added or modified by The Legend of Bum-bo: Windfall is not translated and will only display in the English language.

Starting a new game without finishing your saved game will reset your win streak.

The Legend of Bum-bo: Windfall adds a 'Cutscenes' menu with functionality for rewatching cutscenes. This feature is part of an inaccessible debug menu in the vanilla game and is not original content created for The Legend of Bum-bo: Windfall. The mod only moves the 'Cutscenes' menu to the main menu and slightly modifies the game logic associated with it.

Credit to Jasper Flick for anti-aliasing post processing code used in the mod (catlikecoding.com).

Credit to Pixabay for sound effect assets used in the mod (pixabay.com).

Software used in mod development: BepInEx, HarmonyX, dnSpy, Visual Studio, Git, Unity, Gimp, Krita, Inkscape, Blender, Audacity.

## Bug Reports
If you encounter a bug, please report it by opening an issue on the [Issues](https://github.com/Shpim/The-Legend-of-Bum-bo-Windfall/issues) page or by contacting me (see below).
If possible, try to submit images and/or video demonstrating the bug.

## Contact
Have questions or comments? Send me an Email at jeff.adriaanse@gmail.com or contact me through Discord (username: jeffadriaanse).
