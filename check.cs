using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using DSPCompleteReplicatorQueue;

// Runs QueueCompletion against the game's real MechaForge.GameTick: intermediates must feed the parent, not reach the inventory (GitHub issue #1).
public static class Check
{
    const int Ore = 1001, Gear = 1201, Plate = 1101, Miner = 2301;
    static int failed;

    static void Expect(string name, bool ok, string detail = "")
    {
        failed += ok ? 0 : 1;
        Console.WriteLine((ok ? "PASS " : "FAIL ") + name + " " + detail);
    }

    static T Blank<T>() => (T)FormatterServices.GetUninitializedObject(typeof(T));

    static void Set(object o, string property, object value) =>
        o.GetType().GetProperty(property, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetSetMethod(true).Invoke(o, new[] { value });

    static MechaForge Forge()
    {
        StorageComponent.staticLoaded = true;
        StorageComponent.itemStackCount = new int[12000];
        for (int i = 0; i < 12000; i++) StorageComponent.itemStackCount[i] = 1000;
        var player = Blank<Player>();
        player.isAlive = true;
        Set(player, "package", new StorageComponent(40));
        var data = Blank<GameData>();
        data.factories = new PlanetFactory[0];
        data.history = Blank<GameHistoryData>();
        data.history.featureValues = new Dictionary<int, int>();
        var controller = Blank<PlayerController>();
        controller.gameData = data;
        Set(player, "controller", controller);
        var forge = new MechaForge { tasks = new List<ForgeTask>(), extraItems = new ItemBundle(), bottleneckItems = new HashSet<int>() };
        Set(forge, "player", player);
        Set(forge, "mecha", Blank<Mecha>());
        return forge;
    }

    static ForgeTask Task(int count, int parent, int[] items, int[] itemCounts, int[] served, int product, int productCount) => new ForgeTask
    {
        recipeId = 1, count = count, tickSpend = 100000, parentTaskIndex = parent,
        itemIds = items, itemCounts = itemCounts, served = served,
        productIds = new[] { product }, productCounts = new[] { productCount }, produced = new int[1],
    };

    static int Inv(MechaForge f, int item) => f.player.package.GetItemCount(item);

    static QueueCompletion Run(MechaForge f)
    {
        var c = new QueueCompletion(f);
        c.ProcessBatch(1, 100000, 60000);
        return c;
    }

    public static int Main()
    {
        // Issue #1's case: a miner queued without its gears. Gears are made first and must all go into the miner.
        var f = Forge();
        f.tasks.Add(Task(4, 1, new[] { Ore }, new[] { 1 }, new[] { 4 }, Gear, 1));
        f.tasks.Add(Task(1, -1, new[] { Gear, Plate }, new[] { 4, 2 }, new[] { 0, 2 }, Miner, 1));
        var c = Run(f);
        Expect("issue-1 miner delivered, no intermediate", Inv(f, Miner) == 1 && Inv(f, Gear) == 0 && f.tasks.Count == 0 && c.StopReason == null && c.CompletedCycles == 5,
            $"miner={Inv(f, Miner)} gear={Inv(f, Gear)} tasks={f.tasks.Count} cycles={c.CompletedCycles} stop={c.StopReason}");

        // A recipe making more than the parent needs: only the real surplus reaches the inventory.
        f = Forge();
        f.tasks.Add(Task(2, 1, new[] { Ore }, new[] { 1 }, new[] { 2 }, Gear, 2));
        f.tasks.Add(Task(1, -1, new[] { Gear }, new[] { 3 }, new[] { 0 }, Miner, 1));
        Run(f);
        Expect("surplus delivered once", Inv(f, Miner) == 1 && Inv(f, Gear) == 1, $"miner={Inv(f, Miner)} gear={Inv(f, Gear)}");

        // Unserved inputs: the queue stays untouched and the reason is available for the player.
        f = Forge();
        f.tasks.Add(Task(1, -1, new[] { Ore }, new[] { 1 }, new[] { 0 }, Miner, 1));
        c = Run(f);
        Expect("missing inputs stop", f.tasks.Count == 1 && f.tasks[0].count == 1 && Inv(f, Miner) == 0 && !c.Active && c.StopReason != null, c.StopReason);

        // A task added after the button press is not completed.
        f = Forge();
        f.tasks.Add(Task(1, -1, new[] { Ore }, new[] { 1 }, new[] { 1 }, Gear, 1));
        c = new QueueCompletion(f);
        f.tasks.Add(Task(1, -1, new[] { Ore }, new[] { 1 }, new[] { 1 }, Miner, 1));
        c.ProcessBatch(1, 100000, 60000);
        Expect("new task left alone", Inv(f, Gear) == 1 && Inv(f, Miner) == 0 && f.tasks.Count == 1 && c.StopReason != null, c.StopReason);

        // Partial progress survives: the ready task spends exactly tickSpend.
        f = Forge();
        var t = Task(2, -1, new[] { Ore }, new[] { 1 }, new[] { 2 }, Gear, 1);
        t.tick = 150000;
        f.tasks.Add(t);
        c = new QueueCompletion(f);
        c.ProcessBatch(1, 1, 60000);
        Expect("partial progress kept", t.count == 1 && t.tick == 50000, $"count={t.count} tick={t.tick}");
        return failed;
    }
}
