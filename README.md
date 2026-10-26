# StarCraft II Campaign Ultrawide

Play StarCraft II single-player campaigns in ultrawide with an adjustable HUD, farther camera zoom, and wider hub views.

[Download the latest release](https://github.com/felixfoertsch/StarCraft-2-Campaign-Ultrawide/releases/latest)

Fork of [jrgray933/StarCraft-2-Campaign-Ultrawide](https://github.com/jrgray933/StarCraft-2-Campaign-Ultrawide).

## Features

- Automatic ultrawide setup when you enter a campaign mission or hub.
- Resolution selection from your display's available modes.
- Centered bottom HUD with **50–100% scaling** in **5% increments**. Objectives and dialogs keep their normal size.
- Aligned minimap, unit panel, command card, menu buttons, cargo slots, and production queues. The bottom corners show the battlefield.
- **Max zoom out:** Default or **+10% to +100%** additional camera distance, in **10% increments**.
- **Zoom steps:** **5 to 20** evenly spaced positions between the closest view and your selected maximum.
- Shadow distance adjusts automatically with the selected maximum zoom.
- **Hub Scale:** Off, Fit, or Expanded. Fit shows more scenery without cropping the normal framing; Expanded adds another 20 degrees of horizontal field of view. Mission framing is controlled separately.
- Light and dark themes, saved settings, and a transparent app icon.
- Detects an already-running game and includes **Recheck game**.

## Getting started

1. Download the **windows-x64.zip** release and extract it to a writable folder.
2. Run **Campaign Ultrawide.exe**.
3. Choose your resolution, HUD size, and camera settings, then click **Enable**.
4. Launch StarCraft II through Battle.net and load a campaign mission. If the game is already running, setup starts automatically.
5. Return to the game for a few seconds to let the changes apply.

Use fullscreen mode. The helper does not launch Battle.net or the game, and you do not need to change the game's resolution manually.

You can adjust HUD size, zoom, zoom steps, and hub framing while playing. Close the game before choosing a different resolution. **Default** restores the normal maximum zoom distance; **Off** restores normal hub framing.

## Controls

- **Recheck game** restarts setup if the game is not detected or changes have stopped applying.
- **Stop automatic setup** stops monitoring and restores normal camera settings. The current resolution and HUD remain until you restore them or close the game.
- **More options** provides HUD and resolution restore controls and the activity log.
- Closing the window while monitoring keeps the helper in the notification area. Use its **Exit** command to quit.

## Requirements

- 64-bit Windows 10 or 11, .NET Framework 4.8.
- The 64-bit version of StarCraft II, in fullscreen mode.
- A widescreen resolution supported by your display.

**Single-player campaigns only.** Signing into Battle.net is fine; multiplayer is not supported. This tool modifies game memory, so account enforcement cannot be ruled out. Use at your own risk. It is not affiliated with or endorsed by Blizzard Entertainment.

Game updates may require an updated helper. StarCraft II can be installed in any folder.

## Experimental Wine / Proton support

Run the helper and StarCraft II in the same 64-bit Wine prefix and with the same Wine / Proton runtime. Use a separate copy of your working prefix for testing.

Extract the helper inside the prefix's `C:` drive and launch it using its Windows path. Launching from an external Unix path can break Wine Mono's configuration lookup and JSON serializer. Under umu, launch the helper with `PROTON_VERB=run`.

Run StarCraft II / Battle.net inside gamescope at your display's native resolution, for example:

```sh
gamescope -W 3440 -H 1440 -w 3440 -h 1440 -r 120 -f -- umu-run Battle.net.exe
```

Without gamescope, Proton restores the 16:9 fullscreen mode on alt-tab.

Wine Mono is sufficient; no separate scripting runtime or runtime compilation is needed. Run `Campaign Ultrawide.exe --self-test` first. Verify that `self-test-results.txt` contains native test PASS lines, not only an exit code. Optional `--smoke-test` checks launcher layout and writes `layout-test-results.txt`.

Live campaign behavior under Wine remains unverified. All campaign-only restrictions and account-risk warnings still apply.

## Building

On Linux, install [mise](https://mise.jdx.dev/), then run:

```fish
mise install
mise exec -- dotnet restore --locked-mode
mise exec -- dotnet build -c Release --no-restore
```

Output: `build/Campaign Ultrawide.exe`, targeting x64 .NET Framework 4.8. Build downloads only reference assemblies; deployment remains one executable. Windows can also use `./Build.ps1` with the Framework64 compiler. `./Release.ps1 -SkipUi` builds, runs native self-tests, and packages releases.

## Updating

Close the game and helper before replacing the application with a newer release. Your settings are saved under `%LOCALAPPDATA%\SC2CampaignUltrawide`. The helper does not change your saves or game installation files. Closing the game removes its session changes.

## License

[MIT](LICENSE). StarCraft II and related names are trademarks of their respective owners.
