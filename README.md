# Hold To Command

[GitHub Repository](https://github.com/Vonny1412/Valheim.HoldToCommand)

A small quality-of-life mod that separates **petting** and **commanding** tamed creatures.

In vanilla Valheim, interacting with a commandable tameable will immediately issue a command instead of allowing you to pet it.

This mod changes that behavior to:

* **Tap** → Pet creature
* **Hold** → Command creature
* **Shift + Use** → Rename (unchanged vanilla behavior)

---

## Features

* Restores the ability to pet commandable creatures
* Adds intentional hold-to-command interaction
* Adjusted hover text to reflect new controls
* Fully localized (community language support)
* Uses external translation file (editable)
* Lightweight and fully standalone (no dependencies)

---

## Configuration

HoldToCommand provides the configurable option **HoldThreshold**  
to define how long the Use key must be held before a command is issued.  
Adjust the value to match your preferred hold duration.  

---

## Installation

1. Install **BepInEx** for Valheim
2. Place the contents of the zip into:

```
Valheim/
```

This will install:

```
BepInEx/plugins/HoldToCommand/HoldToCommand.dll
BepInEx/plugins/HoldToCommand/HoldToCommand.Translations.txt
```

3. Launch the game

---

## Translations

HoldToCommand uses an external file:

```
HoldToCommand.Translations.txt
```

You can edit or extend this file to customize translations.

Format:

```
Language|Hold $1|Command
```

Example:

```
English|Hold $1|Command
German|$1 halten|Befehlen
```

* `$1` will automatically be replaced with the Use key (e.g. E).
* If a language is missing, English is used as fallback.

Community translations are welcome.

---

## Compatibility

* Client-side only
* Works on vanilla servers
* Safe to install or remove at any time
* No impact on servers without the mod

---

## License

MIT License

---

*Created with ♥️ — and AI assistance*