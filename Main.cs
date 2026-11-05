using System;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

[assembly: AssemblyVersion("0.0.0.0")]
[assembly: AssemblyFileVersion("1.0.5.0")]

namespace DSPCompleteReplicatorQueue
{
    [BepInPlugin(Guid, "DSPCompleteReplicatorQueue", "1.0.5")]
    [BepInDependency("Appun.DSP.plugin.DSPReplicatorPlus", BepInDependency.DependencyFlags.SoftDependency)]
    public sealed class Main : BaseUnityPlugin
    {
        public const string Guid = "Appun.DSP.plugin.DSPCompleteReplicatorQueue";
        internal static Main Instance;
        private Harmony harmony;
        private QueueCompletion completion;
        private ConfigEntry<KeyboardShortcut> hotkey;
        internal bool Busy { get { return completion != null && completion.Active; } }

        private void Awake()
        {
            Instance = this;
            hotkey = Config.Bind("General", "Hotkey", new KeyboardShortcut(KeyCode.Y, KeyCode.LeftShift), "Completes the replication queue, also while the Replicator window is closed.");
            harmony = new Harmony(Guid);
            harmony.PatchAll(typeof(WindowUI));
        }

        internal void Request(MechaForge forge)
        {
            if (Busy) return;
            completion = new QueueCompletion(forge);
            if (!completion.Active && completion.StopReason != null) ReportStop(completion.StopReason);
        }

        private void Update()
        {
            // The main menu runs a demo game with its own player, so isRunning alone is not enough.
            if (hotkey.Value.IsDown() && !VFInput.inputing && GameMain.isRunning && !GameMain.isLoading && !GameMain.isPaused &&
                !DSPGame.IsMenuDemo && GameMain.mainPlayer != null)
                Request(GameMain.mainPlayer.mecha.forge);
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
                if (current.StopReason != null) ReportStop(current.StopReason);
            }
        }

        private void ReportStop(string reason)
        {
            Logger.LogWarning(reason);
            UIRealtimeTip.Popup("Complete Queue: " + reason);
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
