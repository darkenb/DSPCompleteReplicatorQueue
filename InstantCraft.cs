using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

namespace DSPCompleteReplicatorQueue
{
    public static class InstantCraft
    {
        private static readonly FieldInfo _productIdsField = typeof(ForgeTask)
            .GetField("productIds", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo _productCountsField = typeof(ForgeTask)
            .GetField("productCounts", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo _countField = typeof(ForgeTask)
            .GetField("count", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        // Player.ThrowTrash(int itemId, int count, int inc, Vector3 pos)
        private static readonly MethodInfo _throwTrashMethod = typeof(Player)
            .GetMethod("ThrowTrash", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        public static void CompleteAllTasks(MechaForge forge)
        {
            if (forge == null || forge.tasks == null || forge.tasks.Count == 0)
                return;

            List<ForgeTask> snapshot = new List<ForgeTask>(forge.tasks);
            int totalTaskCount = snapshot.Count;
            int totalItemCount = 0;

            foreach (ForgeTask task in snapshot)
            {
                if (task == null) continue;

                int remaining       = (int)_countField.GetValue(task);
                int[] productIds    = (int[])_productIdsField.GetValue(task);
                int[] productCounts = (int[])_productCountsField.GetValue(task);

                if (remaining <= 0 || productIds == null || productCounts == null) continue;

                for (int i = 0; i < productIds.Length; i++)
                {
                    if (productIds[i] <= 0) continue;

                    int amount = remaining * productCounts[i];
                    int added  = GameMain.mainPlayer.TryAddItemToPackage(productIds[i], amount, 0, true, 0);
                    totalItemCount += added;

                    // Envantere sığmayan kısmı yere bırak — vanilla davranışı
                    int overflow = amount - added;
                    if (overflow > 0 && _throwTrashMethod != null)
                    {
                        _throwTrashMethod.Invoke(GameMain.mainPlayer,
                            new object[] { productIds[i], overflow, 0, GameMain.mainPlayer.position });
                        Main.LogManager.Logger.LogInfo(
                            $"DSPCompleteReplicatorQueue: Inventory full — dropped {overflow}x item[{productIds[i]}] to ground.");
                    }
                }
            }

            forge.tasks.Clear();
            forge.CalculateExtra();

            Main.LogManager.Logger.LogInfo(
                $"DSPCompleteReplicatorQueue: Completed {totalTaskCount} task(s), added {totalItemCount} item(s) to inventory.");
        }
    }
}
