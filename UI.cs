using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace DSPCompleteReplicatorQueue
{
    [HarmonyPatch]
    internal static class WindowUI
    {
        private static UIReplicatorWindow owner;
        private static Button button;
        private static Text label;
        private static bool createFailed;
        private static readonly FieldInfo ForgeField = typeof(UIReplicatorWindow).GetField(
            "mechaForge", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        private static MechaForge GetForge(UIReplicatorWindow window)
        {
            return window != null && ForgeField != null ? ForgeField.GetValue(window) as MechaForge : null;
        }

        internal static void Remove()
        {
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.gameObject.SetActive(false);
                UnityEngine.Object.Destroy(button.gameObject);
            }
            button = null;
            label = null;
            owner = null;
            createFailed = false;
        }

        private static void Ensure(UIReplicatorWindow window)
        {
            if (button != null && owner == window) return;
            if (createFailed && owner == window) return;
            Remove();
            owner = window;
            GameObject root = null;
            try
            {
                Transform panel = window.transform.Find("panel-bg");
                if (panel == null || window.minusButton == null) return;
                var template = window.minusButton.GetComponent<Image>();
                root = new GameObject("DSPCompleteReplicatorQueueButton", typeof(RectTransform), typeof(Image), typeof(Button));
                root.SetActive(false);
                root.transform.SetParent(panel, false);
                var rect = root.GetComponent<RectTransform>();
                var box = panel.Find("btn-box") as RectTransform;
                rect.anchorMin = box != null ? box.anchorMin : new Vector2(1f, 1f);
                rect.anchorMax = box != null ? box.anchorMax : new Vector2(1f, 1f);
                rect.pivot = new Vector2(1f, 0.5f);
                rect.sizeDelta = new Vector2(235f, 30f);
                rect.anchoredPosition = new Vector2(-28f, -65f);
                var image = root.GetComponent<Image>();
                if (template != null) { image.sprite = template.sprite; image.type = template.type; image.material = template.material; }
                image.color = Color.white;
                button = root.GetComponent<Button>();
                button.targetGraphic = image;
                ColorBlock colors = button.colors;
                colors.normalColor = new Color(0.05f, 0.09f, 0.09f, 0.25f);
                colors.highlightedColor = new Color(0.16f, 0.18f, 0.18f, 0.7f);
                colors.pressedColor = new Color(0.18f, 0.27f, 0.27f, 0.7f);
                colors.disabledColor = new Color(0.05f, 0.09f, 0.09f, 0.05f);
                button.colors = colors;
                var textObject = new GameObject("text", typeof(RectTransform), typeof(Text));
                textObject.transform.SetParent(root.transform, false);
                var textRect = textObject.GetComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero; textRect.anchorMax = Vector2.one;
                textRect.offsetMin = Vector2.zero; textRect.offsetMax = Vector2.zero;
                label = textObject.GetComponent<Text>();
                label.font = window.multiValueText.font;
                label.fontSize = 16;
                label.alignment = TextAnchor.MiddleCenter;
                label.color = new Color(0.95f, 0.95f, 0.95f, 1f);
                label.raycastTarget = false;
                label.text = "Complete Replication Queue";
                button.onClick.AddListener(delegate { InstantCraft.CompleteAllTasks(GetForge(owner)); });
                root.SetActive(true);
            }
            catch (Exception ex)
            {
                if (root != null) UnityEngine.Object.Destroy(root);
                button = null; label = null; createFailed = true;
                if (Main.Instance != null) Main.Instance.Log(ex);
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(UIReplicatorWindow), "_OnCreate")]
        private static void Created(UIReplicatorWindow __instance) { Ensure(__instance); }

        [HarmonyPostfix, HarmonyPatch(typeof(UIReplicatorWindow), "_OnOpen")]
        private static void Opened(UIReplicatorWindow __instance) { createFailed = false; Ensure(__instance); }

        [HarmonyPostfix, HarmonyPriority(Priority.Last), HarmonyPatch(typeof(UIReplicatorWindow), "_OnUpdate")]
        private static void Updated(UIReplicatorWindow __instance)
        {
            Ensure(__instance);
            if (button == null) return;
            bool busy = Main.Instance != null && Main.Instance.Busy;
            MechaForge forge = GetForge(__instance);
            button.interactable = !busy && forge != null && forge.tasks != null && forge.tasks.Count > 0;
            label.text = busy ? "Completing Queue..." : "Complete Replication Queue";
        }

        [HarmonyPrefix, HarmonyPatch(typeof(UIReplicatorWindow), "_OnFree")]
        private static void Freed(UIReplicatorWindow __instance)
        {
            if (owner == __instance && Main.Instance != null) Main.Instance.Cancel();
        }

        [HarmonyPrefix, HarmonyPatch(typeof(UIReplicatorWindow), "_OnDestroy")]
        private static void Destroyed(UIReplicatorWindow __instance)
        {
            if (owner != __instance) return;
            if (Main.Instance != null) Main.Instance.Cancel();
            Remove();
        }
    }
}
