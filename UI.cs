using UnityEngine;
using UnityEngine.UI;
using System.Reflection;
using HarmonyLib;

namespace DSPCompleteReplicatorQueue
{
    internal class UI : MonoBehaviour
    {
        public static GameObject instantButton = null;
        private static Image _bgImage = null;

        private static readonly Color ColorNormal   = new Color(0.05f, 0.09f, 0.09f, 0.05f);
        private static readonly Color ColorHover    = new Color(0.16f, 0.18f, 0.18f, 0.50f);
        private static readonly Color ColorPressed  = new Color(0.18f, 0.27f, 0.27f, 0.50f);
        private static readonly Color ColorDisabled = new Color(0.05f, 0.09f, 0.09f, 0.05f);

        private static readonly FieldInfo _mechaForgeField = typeof(UIReplicatorWindow)
            .GetField("mechaForge", BindingFlags.NonPublic | BindingFlags.Instance);

        private static MechaForge GetMechaForge()
            => (MechaForge)_mechaForgeField.GetValue(UIRoot.instance.uiGame.replicator);

        public static void Create()
        {
            Main.LogManager.Logger.LogInfo("DSPCompleteReplicatorQueue: Create() called");
            try
            {
                UIReplicatorWindow replicator = UIRoot.instance.uiGame.replicator;
                GameObject minusButton = replicator.minusButton.gameObject;

                Transform panelBg = replicator.transform.Find("panel-bg");
                Transform btnBox  = panelBg.Find("btn-box");
                RectTransform btnBoxRT = btnBox.GetComponent<RectTransform>();

                // Orijinal minusButton'un Image bilgilerini al — klonlamadan önce
                Image srcImage = minusButton.GetComponent<Image>();
                Sprite srcSprite   = srcImage != null ? srcImage.sprite   : null;
                Image.Type srcType = srcImage != null ? srcImage.type     : Image.Type.Sliced;
                Material srcMat    = srcImage != null ? srcImage.material : null;

                instantButton = UnityEngine.Object.Instantiate(minusButton, panelBg);
                instantButton.name = "DSPCompleteReplicatorQueueButton";

                // UIButton'u hemen yok et
                foreach (var comp in instantButton.GetComponents<MonoBehaviour>())
                {
                    if (comp.GetType().Name == "UIButton")
                    {
                        UnityEngine.Object.DestroyImmediate(comp);
                        Main.LogManager.Logger.LogInfo("DSPCompleteReplicatorQueue: UIButton destroyed");
                        break;
                    }
                }

                // Image'ı orijinalden kopyala + renk set et
                _bgImage = instantButton.GetComponent<Image>();
                if (_bgImage != null)
                {
                    _bgImage.sprite   = srcSprite;
                    _bgImage.type     = srcType;
                    _bgImage.material = srcMat;
                    _bgImage.color    = ColorNormal;
                }

                RectTransform instRT = instantButton.GetComponent<RectTransform>();
                instRT.anchorMin        = btnBoxRT.anchorMin;
                instRT.anchorMax        = btnBoxRT.anchorMax;
                instRT.pivot            = new Vector2(1f, 0.5f);
                instRT.sizeDelta        = new Vector2(200f, 30f);
                instRT.anchoredPosition = new Vector2(-28f, -65f);

                // Yazı
                Text label = instantButton.transform.Find("text").GetComponent<Text>();
				label.font     = replicator.multiValueText.font;  // DSP standart font
                label.text     = "Complete Replication Queue";
                label.fontSize = 16;
                label.color    = new Color(0.95f, 0.95f, 0.95f, 1.00f);

                Button btn = instantButton.GetComponent<Button>();
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(OnClickInstantAll);

                // ColorBlock tamamen beyaz — rengi biz yönetiyoruz
                ColorBlock cb = btn.colors;
                cb.normalColor      = Color.white;
                cb.highlightedColor = Color.white;
                cb.pressedColor     = Color.white;
                cb.disabledColor    = Color.white;
                cb.colorMultiplier  = 1f;
                btn.colors = cb;

                instantButton.SetActive(true);

                Main.LogManager.Logger.LogInfo("DSPCompleteReplicatorQueue: Button created OK");
            }
            catch (System.Exception e)
            {
                Main.LogManager.Logger.LogError("DSPCompleteReplicatorQueue: Button creation failed: " + e.Message);
            }
        }

        public void Update()
        {
            if (instantButton != null && _bgImage != null)
            {
                Button btn = instantButton.GetComponent<Button>();
                bool interactable = btn != null && btn.interactable;

                if (!interactable)
                {
                    _bgImage.color = ColorDisabled;
                }
                else
                {
                    RectTransform rt = instantButton.GetComponent<RectTransform>();
                    Canvas canvas = instantButton.GetComponentInParent<Canvas>();
                    Camera cam = (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                        ? canvas.worldCamera : null;
                    bool isHover = RectTransformUtility.RectangleContainsScreenPoint(
                        rt, Input.mousePosition, cam);

                    if (isHover && Input.GetMouseButton(0))
                        _bgImage.color = ColorPressed;
                    else if (isHover)
                        _bgImage.color = ColorHover;
                    else
                        _bgImage.color = ColorNormal;
                }
            }
        }

        [HarmonyPriority(Priority.Last)]
        [HarmonyPostfix, HarmonyPatch(typeof(UIReplicatorWindow), "_OnUpdate")]
        public static void UIReplicatorWindow_OnUpdate_Postfix(UIReplicatorWindow __instance)
        {
            if (instantButton == null) return;
            MechaForge forge = GetMechaForge();
            bool hasQueue = forge != null && forge.tasks != null && forge.tasks.Count > 0;
            instantButton.GetComponent<Button>().interactable = hasQueue;
        }

        public static void OnClickInstantAll()
        {
            MechaForge forge = GetMechaForge();
            if (forge == null) return;
            Main.LogManager.Logger.LogInfo("DSPCompleteReplicatorQueue: Button clicked, completing " + forge.tasks.Count + " tasks...");
            InstantCraft.CompleteAllTasks(forge);
        }
    }
}
