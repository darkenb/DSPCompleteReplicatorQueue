# DSP Complete Replicator Queue

Adds a **Complete Queue** button to the Replicator window in Dyson Sphere Program.

The analysis and proposed fixes in release 1.0.3 were developed with assistance from the GPT 6.1 Sol model. English documentation accompanies the code to explain the changes and the available validation results.

## Features

- Press **Shift+Y** to complete the queue, even while the Replicator window is closed. Change the key with the `Hotkey` setting in the mod's config.
- Complete the tasks present when the button is pressed without waiting for the normal crafting time.
- Preserve native recipe inputs, intermediate products, inventory/delivery handling, overflow and production events.
- Process large queues over multiple frames and stop safely if tasks change or production cannot proceed. An in-game message tells you why it stopped.

## Requirements

- Dyson Sphere Program with BepInEx.
- DSPReplicatorPlus is optional.

## Installation

Install with a mod manager such as Gale or r2modman.

To install by hand, copy `DSPCompleteReplicatorQueue.dll` into `BepInEx/plugins/DSPCompleteReplicatorQueue/`. Keep a single copy of the DLL.

## Build from source

Use a current .NET SDK and the .NET Framework 4.7.2 Developer Pack. Game, Unity and BepInEx assemblies are external references and are not included in the source download.

The project defaults to `D:\Games\Dyson Sphere Program` and the Gale Default profile's BepInEx core. Override `DSPGamePath`, `BepInExCorePath` or `GameAssemblyPath` for your installation, for example:

```text
dotnet build DSPCompleteReplicatorQueue.csproj -c Release "-p:DSPGamePath=D:\Games\Dyson Sphere Program" "-p:BepInExCorePath=C:\path\to\BepInEx\core"
```

Output: `bin/Release/DSPCompleteReplicatorQueue.dll`.

`build_and_install.bat` builds only when run without arguments. After closing the game, an explicit plugin-directory argument enables installation; it builds first, backs up an existing DLL, and does not delete plugin directories.

## License

MIT, see [LICENSE](LICENSE).
