using System;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

[assembly: AssemblyVersion("0.0.0.0")]
[assembly: AssemblyFileVersion("1.0.3.0")]

namespace DSPCompleteReplicatorQueue
{
    [BepInPlugin(Guid, "DSPCompleteReplicatorQueue", "1.0.3")]
    [BepInDependency("Appun.DSP.plugin.DSPReplicatorPlus", BepInDependency.DependencyFlags.SoftDependency)]
    public sealed class Main : BaseUnityPlugin
    {
        public const string Guid = "Appun.DSP.plugin.DSPCompleteReplicatorQueue";
        internal static Main Instance;
        private Harmony harmony;
        private QueueCompletion completion;
        internal bool Busy { get { return completion != null && completion.Active; } }

        private void Awake()
        {
            Instance = this;
            harmony = new Harmony(Guid);
            harmony.PatchAll(typeof(WindowUI));
        }

        internal void Request(MechaForge forge)
        {
            if (Busy) return;
            completion = new QueueCompletion(forge);
            if (!completion.Active && completion.StopReason != null) Logger.LogWarning(completion.StopReason);
        }

        private void Update()
        {
            if (!Busy) return;
            if (!GameMain.isRunning || GameMain.isLoading || GameMain.mainPlayer == null ||
                !ReferenceEquals(completion.Forge.player, GameMain.mainPlayer))
            {
                Cancel();
                return;
            }
            if (GameMain.isPaused) return;
            QueueCompletion current = completion;
            try { current.ProcessBatch(GameMain.gameTick); }
            catch (Exception ex) { Logger.LogError(ex); }
            if (ReferenceEquals(current, completion) && !current.Active)
            {
                Logger.LogInfo("Complete Queue: " + current.CompletedCycles + " native production cycles completed.");
                if (current.StopReason != null) Logger.LogWarning(current.StopReason);
            }
        }

        internal void Cancel()
        {
            if (completion != null) completion.Stop("Completion cancelled; remaining queue preserved.");
            completion = null;
        }

        private void OnDestroy()
        {
            Cancel();
            WindowUI.Remove();
            if (harmony != null) harmony.UnpatchSelf();
            if (ReferenceEquals(Instance, this)) Instance = null;
        }

        internal void Log(Exception ex) { Logger.LogError(ex); }
    }
}
