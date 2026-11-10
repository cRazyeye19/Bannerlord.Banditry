BANDITRY (v2.0.0, updated for Mount & Blade II: Bannerlord 1.4.x)
================================================================

Recruit bandits from infested hideouts and looters from friendly gang leaders in
towns, and fight a larger looter troop tree (Looter > Ransacker > Pillager /
Scrimmager > Manslayer / Vandal).

REQUIREMENTS
  - Bannerlord.MBOptionScreen (MCM) v5, for the Mod Options menu.

INSTALL
  Copy the "Banditry" folder into your game's Modules folder, then enable
  "Banditry" in the launcher. Load it after Native, SandBoxCore, Sandbox,
  StoryMode and MCM (the launcher does this automatically).

HOW TO USE
  - Hideouts: enter an infested hideout menu and choose "Recruit bandits".
  - Towns: open the town menu and choose "Recruit from gang leaders"
    (needs a gang leader whose relation with you is 0 or higher).
  Pick a troop type, then an amount. Cost is the normal recruitment cost.

OPTIONS (Mod Options > Banditry)
  - Hideout / Gang Leader Recruitable Multipliers.
  - Remove Modded Data from Save: tick it, load your save once, save again, then
    you can safely disable the mod for that save.

CHANGES FROM THE ORIGINAL (2020, v1.0.4)
  - Rebuilt for current game APIs and XML formats.
  - Recruiting now uses pop-up dialogs instead of a repurposed party screen.
  - ModLib/Harmony dependencies replaced by MCM; no runtime file renaming.
  - The "enable/disable troop trees / party templates" toggles were removed
    (they worked by renaming files inside the Modules folder).
  - Each troop now has its own name text (the original showed "Looter" for all).
