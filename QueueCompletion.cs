using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DSPCompleteReplicatorQueue
{
    // Only advance the tasks present when the button was pressed. Native forge owns
    // consumption, parent feeding, delivery, statistics, events and queue removal.
    public sealed class QueueCompletion
    {
        private readonly Dictionary<ForgeTask, int> observed = new Dictionary<ForgeTask, int>();
        private bool processing;
        public MechaForge Forge { get; private set; }
        public bool Active { get; private set; }
        public int CompletedCycles { get; private set; }
        public string StopReason { get; private set; }

        public QueueCompletion(MechaForge forge)
        {
            Forge = forge;
            if (forge == null || forge.tasks == null || forge.tasks.Count == 0) return;
            for (int i = 0; i < forge.tasks.Count; i++)
            {
                ForgeTask task = forge.tasks[i];
                if (!Valid(task) || observed.ContainsKey(task) ||
                    (task.parentTaskIndex >= 0 &&
                     (task.parentTaskIndex <= i || task.parentTaskIndex >= forge.tasks.Count)))
                {
                    Stop("Invalid task state; queue left unchanged.");
                    return;
                }
                observed.Add(task, task.count);
            }
            Active = true;
        }

        private static bool Valid(ForgeTask task)
        {
            if (task == null || task.count <= 0 || task.tickSpend <= 0 ||
                task.itemIds == null || task.itemCounts == null || task.served == null ||
                task.productIds == null || task.productCounts == null || task.produced == null ||
                task.itemIds.Length != task.itemCounts.Length || task.served.Length != task.itemCounts.Length ||
                task.productIds.Length != task.productCounts.Length || task.produced.Length != task.productCounts.Length)
                return false;
            for (int i = 0; i < task.itemCounts.Length; i++)
                if (task.itemIds[i] <= 0 || task.itemCounts[i] <= 0 || task.served[i] < 0) return false;
            for (int i = 0; i < task.productCounts.Length; i++)
                if (task.productIds[i] <= 0 || task.productCounts[i] <= 0 || task.produced[i] != 0) return false;
            return true;
        }

        public void Stop(string reason)
        {
            Active = false;
            StopReason = reason;
        }

        public int ProcessBatch(long gameTick, int maxCycles = 128, double maxMilliseconds = 4.0)
        {
            if (!Active || processing || maxCycles <= 0 || maxMilliseconds <= 0) return 0;
            int completed = 0;
            var watch = Stopwatch.StartNew();
            processing = true;
            try
            {
                while (Active && completed < maxCycles && watch.Elapsed.TotalMilliseconds < maxMilliseconds)
                {
                    if (Forge.tasks == null || Forge.player == null || Forge.mecha == null || !Forge.player.isAlive)
                    {
                        Stop("Forge or player is no longer available.");
                        break;
                    }
                    if (Forge.tasks.Count == 0) { Stop(null); break; }
                    ForgeTask task = Forge.tasks[0];
                    int previousCount;
                    if (task == null || !observed.TryGetValue(task, out previousCount) || task.count > previousCount)
                    {
                        Stop("Queue changed; press Complete Queue again to include new tasks.");
                        break;
                    }
                    if (!Valid(task) || !task.itemEnough ||
                        (task.parentTaskIndex >= 0 &&
                         (task.parentTaskIndex == 0 || task.parentTaskIndex >= Forge.tasks.Count ||
                          !observed.ContainsKey(Forge.tasks[task.parentTaskIndex]))))
                    {
                        Stop("Missing inputs or invalid task state; remaining queue preserved.");
                        break;
                    }
                    int before = task.count;
                    int oldTick = task.tick;
                    // Do not add time to an already-ready task: native Produce
                    // subtracts exactly tickSpend, preserving partial progress.
                    task.tick = Math.Max(task.tick, task.tickSpend);
                    try
                    {
                        Forge.GameTick(gameTick, 0f);
                    }
                    catch
                    {
                        // Native callbacks can fail after Produce has committed.
                        // Never roll back count/items or repeat product delivery.
                        if (task.count == before) task.tick = oldTick;
                        Stop("Native forge raised an error; accelerated completion stopped.");
                        throw;
                    }
                    if (task.count != before - 1)
                    {
                        if (task.count == before) task.tick = oldTick;
                        Stop("Native forge made no expected progress; remaining queue preserved.");
                        break;
                    }
                    observed[task] = task.count;
                    completed++;
                    CompletedCycles++;
                    if (Forge.tasks == null || Forge.tasks.Count == 0) Stop(null);
                }
            }
            finally { processing = false; }
            return completed;
        }
    }
}
