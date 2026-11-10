# Banditry (updated for Bannerlord 1.4.x)

A Mount & Blade II: Bannerlord mod that lets you recruit bandits from infested hideouts and looters from friendly town gang leaders, and adds a larger looter troop tree (Looter > Ransacker > Pillager / Scrimmager > Manslayer / Vandal).

This is an update/rewrite of the original **Banditry** mod (v1.0.4, 2020), which stopped working with current game versions. The original author's work is the basis for the troop data and the mod concept; all credit for the idea goes to them. See `Module/README.txt` for what changed.

## Requirements
- Bannerlord (tested against the 1.4.x module versions only at build time, not yet in-game)
- [MCM / Bannerlord.MBOptionScreen](https://www.nexusmods.com/mountandblade2bannerlord/mods/612) v5

## Install
Build (below), then copy the `Module` folder into your game's `Modules` directory renamed to `Banditry`.

## Build
Requires the .NET SDK. The project references the game's DLLs directly:

```
dotnet build Banditry.csproj -c Release -p:GameFolder="C:\path\to\Mount & Blade II Bannerlord"
```

Output goes to `Module/bin/Win64_Shipping_Client/Banditry.dll`.

## Status
Compiles against the game libraries. Not yet verified in-game; bug reports welcome.
