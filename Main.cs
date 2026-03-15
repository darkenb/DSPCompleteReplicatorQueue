using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using System.Reflection;
using System.Security;
using System.Security.Permissions;

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace DSPCompleteReplicatorQueue
{
    [BepInPlugin("Appun.DSP.plugin.DSPCompleteReplicatorQueue", "DSPCompleteReplicatorQueue", "0.0.1")]
    [BepInDependency("Appun.DSP.plugin.DSPReplicatorPlus", BepInDependency.DependencyFlags.SoftDependency)]
    [HarmonyPatch]
    public class Main : BaseUnityPlugin
    {
        public void Start()
        {
            LogManager.Logger = Logger;
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());

            // UI MonoBehaviour — Update() için
            GameObject uiObject = new GameObject("DSPCompleteReplicatorQueueUI");
            uiObject.AddComponent<UI>();
            DontDestroyOnLoad(uiObject);

            // DSPReplicatorPlus ile aynı yöntem: Start()'tan direkt çağır
            UI.Create();
        }

        public class LogManager
        {
            public static ManualLogSource Logger;
        }
    }
}
