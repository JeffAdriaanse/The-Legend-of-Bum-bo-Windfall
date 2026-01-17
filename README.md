<p align="center">
 <picture>
  <img src="/assets/Windfall Logo.png" alt="Windfall logo" width="600">
 </picture>
</p>
Windfall is a mod for the Legend of Bum-bo that fixes bugs, adds new features, and adjusts game balance.

To see mod updates and change lists, visit the [Releases](https://github.com/Shpim/The-Legend-of-Bum-bo-Windfall/releases) page.

The Legend of Bum-bo does not have official modding support. This mod uses BepInEx, a modding framework that allows for patching games made in the Unity engine.

## Features
* A new character, Bum-bo the Wise
* 2 new spells
* 2 new trinkets
* Enhanced tooltips that display information of all enemies and collectibles
* Status indicators, which display temporary effects that are currently influencing Bum-bo
* Spell slot indicators, which show what upgrades a spell has received and other information
* Spell upgrade previews, so Bum-bo can see how his spell will be upgraded before he commits to it
* Ramappable hotkeys
* A win streak counter
* The ability to rewatch cutscenes from the main menu
* Improvements to the save and continue system for in progress runs
* Various other quality of life improvements
* Many [balance changes](https://github.com/Shpim/The-Legend-of-Bum-bo-Windfall/wiki/Balance-Changes)
* And lots of bug fixes!

## Easy Installation
[Click here to download an installer for the latest version (1.4.0) of Windfall!](https://github.com/JeffAdriaanse/The-Legend-of-Bum-bo-Windfall/releases/download/v1.4.0/Windfall_Installer.msi)

Run the installer. Follow the steps and the installer will automatically add Windfall to The Legend of Bum-bo for you.

If you already have an older version of Windfall installed, the installer will update Windfall to the new version.

To uninstall the mod, run the installer again and select 'Remove'.

## Manual Installation
Easy installation is recommended as it provides an easy-to-use installer. If you would still like to install manually, proceed below.

If you already have BepInEx 5.4 installed for The Legend of Bum-bo, skip steps 1 and 2.

1. [Click here to download BepInEx.](https://github.com/BepInEx/BepInEx/releases/download/v5.4.21/BepInEx_x64_5.4.21.0.zip)
2. Extract the contents of `BepInEx_x64_5.4.21.0.zip` into The Legend of Bum-bo root folder. You can find the root folder by clicking `Manage > Browse local files` in the options menu on the game's page in your Steam library.
3. [Click here to download the latest version of Windfall.](https://github.com/JeffAdriaanse/The-Legend-of-Bum-bo-Windfall/releases/download/v1.4.0/The.Legend.of.Bum-bo_Windfall.zip)
4. Extract the contents of `The.Legend.of.Bum-bo_Windfall.zip` into the `BepInEx/plugins` directory in the game folder. Create the `plugins` folder if it is not there already. Replace files if prompted.

The directory structure should end up looking like this:
```
The Legend of Bum-Bo/
└─ BepInEx/
   ├─ core/
   │  └─ ...
   └─ plugins/
      └─ The Legend of Bum-bo_Windfall/
         ├─ windfall
         └─ The Legend of Bum-bo_Windfall.dll
```
If you are having trouble installing BepInEx, consult the BepInEx [installation guide](https://docs.bepinex.dev/articles/user_guide/installation/index.html).

To update the mod, repeat steps 3 and 4 with the new mod release.

To uninstall the mod, remove `The Legend of Bum-bo_Windfall` folder from `BepInEx/plugins`.
To uninstall BepInEx, remove the `BepInEx` folder itself.

## Windfall on Linux With Proton
It is possible to get Windfall working on Linux systems that are running The Legend of Bum-bo through the Proton compatibility layer. However, an extra step is needed to get BepInEx working properly with Proton.

Follow the instructions below:

1. Follow the [Manual Installation](https://github.com/JeffAdriaanse/The-Legend-of-Bum-bo-Windfall?tab=readme-ov-file#manual-installation) steps as normal.
2. In Steam, go to The Legend of Bum-bo. Select `Properties → Launch Options` and write `WINEDLLOVERRIDES="winhttp=n,b" %command%` in the launch options.
3. Run the game through Steam.

## Vanilla Save Data
Some players have reported in steam discussions([1](https://steamcommunity.com/app/1148650/discussions/0/3845556684657609646/) [2](https://steamcommunity.com/app/1148650/discussions/0/3802777561340047478/)) that the vanilla game might not properly save progression to Steam Cloud, even if cloud saves are enabled.

Unfortunately it does not seem possible to investigate or fix cloud save issues with mods. It is recommended that players who are concerned about losing save progress use other methods to back up their local save data.

When playing through Steam, The Legend of Bum-bo stores local save data in the `%USERPROFILE%\Documents\My Games\The Legend of Bum-bo (Steam)` directory. Data is saved to three files:
- `progression.sav` contains your long-term game progression and unlocks.
- `setting.sav` contains your menu settings such as music and SFX volume.
- `state.sav` contains the state of your in-progress run, allowing you to resume where you left off.

## Windfall Save Data
When playing with Windfall installed, the mod stores its own save data. Windfall save data is stored separately from vanilla save data and is not saved to Steam Cloud.

Windfall stores its local save data in the same place the mod is installed to, the `BepInEx/Plugins/The Legend of Bum-bo_Windfall` directory in the game root folder. Data is saved to two files:
- `windfall.sav` contains your Windfall menu settings, as well as your progression and unlocks for content added by Windfall.
- `windfallstate.sav` contains some extra data regarding the state of your in-progress run, improving the save and continue feature of the vanilla game.

## Additional Information
The Legend of Bum-bo: Windfall is made for the Steam and GOG versions of the vanilla game, although it most likely works with the Epic Games version as well.

Text that is added or modified by The Legend of Bum-bo: Windfall is not translated to all languages and will only display in English and Chinese.

Starting a new game without finishing your saved game will reset your win streak.

The ability to rewatch cutscenes exists in a debug menu in the vanilla game, Windfall just brings the feature to the main menu.

Credit to [YazawaAkiOS](https://github.com/YazawaAkio) for translating Windfall into Chinese.

Credit to Jasper Flick for anti-aliasing post processing code used in the mod (catlikecoding.com).

Credit to Pixabay for the Plasma Ball sound effect asset used in the mod (pixabay.com).

Software used in mod development: BepInEx, HarmonyX, dnSpy, Visual Studio, Git, Unity, Gimp, Krita, Inkscape, Blender, Audacity, DaVinci Resolve.

## Bug Reports
If you encounter a bug, please report it by opening an issue on the [Issues](https://github.com/Shpim/The-Legend-of-Bum-bo-Windfall/issues) page or by contacting me (see below).
If possible, try to submit images and/or video demonstrating the bug.

## Contact
Have questions or comments? Send me an Email at jeff.adriaanse@gmail.com.
