# Changelog

## 1.10.0

- Run without PowerShell: the engine is compiled into the executable. Wine / Proton needs only Wine Mono.
- Build on Linux with `mise exec -- dotnet build -c Release`.
- Run the helper in the same Wine / Proton prefix as StarCraft II. Under umu, launch it with `PROTON_VERB=run`.
- Run StarCraft II / Battle.net inside gamescope at native resolution, for example `gamescope -W 3440 -H 1440 -w 3440 -h 1440 -r 120 -f -- umu-run Battle.net.exe`. Otherwise Proton restores the 16:9 fullscreen mode on alt-tab.

## 1.9.1

- Fix Fit and Expanded hub views in the Zerg campaign.

Close StarCraft II and the helper before updating.

Download **Starcraft-2-Campaign-Ultrawide-1.9.1-windows-x64.zip**, extract it, and run **Campaign Ultrawide.exe**.

## 1.9.0

- Restore support for the latest StarCraft II update.
- Automatically locate game addresses after compatible updates.
- Fix extra zoom and zoom-step settings in missions such as Enemy Within.
- Keep campaign objectives, hero panels, and encounter displays at their normal size and position.

Download **Starcraft-2-Campaign-Ultrawide-1.9.0-windows-x64.zip**, extract it, and run **Campaign Ultrawide.exe**.

## 1.8.0

- Add farther mission zoom, from +10% to +100% in 10% increments.
- Add 5–20 zoom positions, evenly spaced across the selected range.
- Extend shadow distance automatically to keep distant shadows visible when zoomed out.
- Add Off, Fit, and Expanded hub framing. Keep the selected framing when changing rooms, opening the mission computer, or returning from a mission.
- Add light and dark themes and a transparent app icon.
- Limit bottom HUD size to 50–100% in 5% increments.
- Fix disappearing hotkeys and inconsistent unit-panel text when changing HUD size during a mission.
- Improve setup when opening the helper after loading a mission from the archives.

Download **Starcraft-2-Campaign-Ultrawide-1.8.0-windows-x64.zip**, extract it, and run **Campaign Ultrawide.exe**. Close the game and helper before updating.
