# DSP Complete Replicator Queue 1.0.3

Adds a **Complete Queue** button to the Replicator window in Dyson Sphere Program.

The analysis and proposed fixes in this release were developed with assistance from the GPT 6.1 Sol model. English documentation accompanies the code to explain the changes and available validation results.

## Features

- Complete the tasks present when the button is pressed without waiting for normal crafting time.
- Preserve native recipe inputs, intermediate products, inventory/delivery handling, overflow and production events.
- Process large queues over multiple frames and stop safely if tasks change or production cannot proceed.

## Requirements

- Dyson Sphere Program with BepInEx installed.
- DSPReplicatorPlus is optional.

## Install manually

1. Close the game.
2. Extract this ZIP into a temporary folder.
3. Copy the `DSPCompleteReplicatorQueue` folder, which contains the DLL, into the game's or active mod profile's `BepInEx/plugins/` directory.
4. The final DLL path should be `BepInEx/plugins/DSPCompleteReplicatorQueue/DSPCompleteReplicatorQueue.dll`.

## Update an existing installation

1. Close the game and back up your current mod DLL.
2. Replace the existing `DSPCompleteReplicatorQueue.dll` with the DLL from this ZIP, using the folder where the previous DLL is already installed.
3. Keep a single installed DLL for this mod. If your manager placed it inside a package folder, replace it there rather than creating a second copy elsewhere.
4. Start the game, open the Replicator, queue a recipe, and check the **Complete Queue** button.

For a manager-owned profile, use the manager's local import/update feature when supported so its package records stay consistent. The manifest in this ZIP declares version 1.0.3.

## Changes in 1.0.3

- Use native production instead of granting products directly and clearing the queue.
- Preserve intermediate items, delivery/overflow handling, statistics and events.
- Avoid duplicate overflow transfers and repeated activation.
- Stop safely on changed queues, missing inputs or invalid tasks.
- Limit accelerated work per frame and improve button lifecycle cleanup.


## Build from source

Use a current .NET SDK and the .NET Framework 4.7.2 Developer Pack. Game, Unity and BepInEx assemblies are external references and are not included in the source download.

The project defaults to `D:\Games\Dyson Sphere Program` and the Gale Default profile's BepInEx core. Override `DSPGamePath`, `BepInExCorePath` or `GameAssemblyPath` for your installation, for example:

```text
dotnet build DSPCompleteReplicatorQueue.csproj -c Release "-p:DSPGamePath=D:\Games\Dyson Sphere Program" "-p:BepInExCorePath=C:\path\to\BepInEx\core"
```

Output: `bin/Release/DSPCompleteReplicatorQueue.dll`.

`build_and_install.bat` builds only when run without arguments. After closing the game, an explicit plugin-directory argument enables installation; it builds first, backs up an existing DLL, and does not delete plugin directories.

The project compiles `Main.cs`, `InstantCraft.cs`, `UI.cs` and `QueueCompletion.cs`. Upload all source files from the source ZIP to the repository root so the new queue implementation is included.
