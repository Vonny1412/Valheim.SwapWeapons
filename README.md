# Swap Weapons

[GitHub Repository](https://github.com/Vonny1412/Valheim.SwapWeapons)

A small quality-of-life mod that lets you quickly swap between your **current and previously equipped weapons** with a single key press.

Instead of manually switching back and forth between weapons or tools, Swap Weapons remembers your previous equipment and allows you to restore it instantly.

---

## Features

* Swap between current and previously equipped weapons with a single key press
* Supports one-handed weapons, two-handed weapons, bows, shields, torches, and tools
* Correctly handles left- and right-hand equipment
* Automatically restores weapons after swimming (configurable)
* Integrates with Valheim's normal weapon sheathing behavior
* Lightweight and fully standalone (no dependencies)

---

## Configuration

Swap Weapons provides the following configurable options:

* **SwapWeaponsKey** — Key used to swap between your current and previously equipped weapons
* **AutoEquipOnLeavingWater** — Automatically restores weapons that were unequipped when entering water
* **EnableDebugLogging** — Enables detailed logging for troubleshooting

The default key for swapping weapons is **Q**.

---

## Installation

1. Install **BepInEx** for Valheim
2. Place the contents of the zip into:

```
Valheim/
```

This will install:

```
BepInEx/plugins/SwapWeapons/SwapWeapons.dll
```

3. Launch the game

---

## Compatibility

* Client-side only
* Works on vanilla servers
* Safe to install or remove at any time
* No impact on servers without the mod

---

## Credits

Swap Weapons is based on the original **Last Used Weapons** mod by **bakaspaceman**.

The original mod provided the foundation and core concept for switching between previously equipped weapons. Swap Weapons is an extensively reworked and updated continuation, adapted for current versions of Valheim with fixes and changes to the original implementation.

**Original mod:**

* Plugin ID: `bakaspaceman.LastUsedWeapons`
* [Nexus Mods](https://www.nexusmods.com/valheim/mods/259)
* [Thunderstore](https://thunderstore.io/c/valheim/p/bakaspaceman/Last_Used_Weapons/)
* [GitHub Repository](https://github.com/bakaspaceman/ValheimMods)
* Original license: MIT

---

## License

MIT License

---

*Created with ♥️ — and AI assistance*
