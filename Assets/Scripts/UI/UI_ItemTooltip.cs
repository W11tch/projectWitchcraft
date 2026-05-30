using System.Text;
using TMPro;
using UnityEngine;
using ProjectWitchcraft.Core;

namespace ProjectWitchcraft.UI
{
    public class UI_ItemTooltip : MonoBehaviour
    {
        public static UI_ItemTooltip Instance { get; private set; }

        [Header("Primary Panel")]
        [SerializeField] private RectTransform _panel;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _typeText;
        [SerializeField] private TextMeshProUGUI _statsText;
        [SerializeField] private TextMeshProUGUI _durabilityText;
        [SerializeField] private TextMeshProUGUI _descriptionText;

        [Header("Comparison Panel")]
        [SerializeField] private RectTransform _comparisonPanel;
        [SerializeField] private TextMeshProUGUI _cmpNameText;
        [SerializeField] private TextMeshProUGUI _cmpTypeText;
        [SerializeField] private TextMeshProUGUI _cmpStatsText;
        [SerializeField] private TextMeshProUGUI _cmpDurabilityText;
        [SerializeField] private TextMeshProUGUI _cmpDescriptionText;
        [SerializeField] private float _comparisonGap = 8f;

        [Header("Settings")]
        [SerializeField] private Vector2 _cursorOffset = new Vector2(16f, 16f);

        private Canvas _canvas;

        private readonly struct PanelRefs
        {
            public readonly TextMeshProUGUI name, type, stats, durability, description;
            public PanelRefs(TextMeshProUGUI n, TextMeshProUGUI t, TextMeshProUGUI s,
                             TextMeshProUGUI d, TextMeshProUGUI desc)
            { name = n; type = t; stats = s; durability = d; description = desc; }
        }

        private void Awake()
        {
            Instance = this;
            _canvas = GetComponentInParent<Canvas>();
            _panel.gameObject.SetActive(false);
            if (_comparisonPanel != null) _comparisonPanel.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (!_panel.gameObject.activeSelf) return;
            FollowCursor();
        }

        public void Show(ItemInstance item)
        {
            if (item == null || item.IsEmpty) return;
            Populate(item, PrimaryRefs());
            _panel.gameObject.SetActive(true);
            if (_comparisonPanel != null) _comparisonPanel.gameObject.SetActive(false);
            FollowCursor();
        }

        public void ShowWithComparison(ItemInstance primary, ItemInstance comparison)
        {
            if (primary == null || primary.IsEmpty) return;
            Populate(primary, PrimaryRefs());
            _panel.gameObject.SetActive(true);

            if (_comparisonPanel != null && comparison != null && !comparison.IsEmpty)
            {
                Populate(comparison, ComparisonRefs());
                _comparisonPanel.gameObject.SetActive(true);
            }
            else if (_comparisonPanel != null)
            {
                _comparisonPanel.gameObject.SetActive(false);
            }

            FollowCursor();
        }

        public void Hide()
        {
            _panel.gameObject.SetActive(false);
            if (_comparisonPanel != null) _comparisonPanel.gameObject.SetActive(false);
        }

        private PanelRefs PrimaryRefs() =>
            new PanelRefs(_nameText, _typeText, _statsText, _durabilityText, _descriptionText);

        private PanelRefs ComparisonRefs() =>
            new PanelRefs(_cmpNameText, _cmpTypeText, _cmpStatsText, _cmpDurabilityText, _cmpDescriptionText);

        private static void Populate(ItemInstance item, PanelRefs r)
        {
            var def = item.Definition;

            r.name.text = def.Name;

            string typeLine = GetTypeLine(def);
            r.type.text = typeLine;
            r.type.gameObject.SetActive(!string.IsNullOrEmpty(typeLine));

            string stats = BuildAffixText(def);
            r.stats.text = stats;
            r.stats.gameObject.SetActive(!string.IsNullOrEmpty(stats));

            if (item.HasDurability)
            {
                float max = def.GetMaxDurability();
                r.durability.text = $"Durability: {item.CurrentDurability:0}/{max:0}";
                r.durability.gameObject.SetActive(true);
            }
            else
            {
                r.durability.gameObject.SetActive(false);
            }

            r.description.text = def.description;
            r.description.gameObject.SetActive(!string.IsNullOrEmpty(def.description));
        }

        private static string GetTypeLine(ItemData def)
        {
            if (def is EquipmentItemData equip)
                return equip.Slot.ToString();
            if (def is WeaponItemData weapon)
                return weapon.IsTwoHanded ? "Two-Handed Weapon" : "Weapon";
            if (def is ToolItemData)
                return "Tool";
            return string.Empty;
        }

        private static string BuildAffixText(ItemData def)
        {
            System.Collections.Generic.IReadOnlyList<EquipmentAffix> affixes = null;
            if (def is EquipmentItemData equip) affixes = equip.Affixes;
            else if (def is WeaponItemData weapon) affixes = weapon.Affixes;
            else if (def is ToolItemData tool) affixes = tool.Affixes;

            if (affixes == null || affixes.Count == 0) return string.Empty;

            var sb = new StringBuilder();
            foreach (var affix in affixes)
            {
                if (affix.Stat == null) continue;
                sb.AppendLine(FormatAffix(affix));
            }
            return sb.ToString().TrimEnd();
        }

        private static string FormatAffix(EquipmentAffix affix)
        {
            float val = affix.Value;
            string stat = affix.Stat.statName;

            switch (affix.Stage)
            {
                case CalculationStage.Flat:
                    return val >= 0 ? $"+{val:0.##} {stat}" : $"{val:0.##} {stat}";

                case CalculationStage.Additive:
                    float addPct = val * 100f;
                    return addPct >= 0 ? $"+{addPct:0.##}% increased {stat}" : $"{addPct:0.##}% reduced {stat}";

                case CalculationStage.Multiplicative:
                    float morePct = val * 100f;
                    return morePct >= 0 ? $"+{morePct:0.##}% more {stat}" : $"{morePct:0.##}% less {stat}";

                default:
                    return string.Empty;
            }
        }

        private void FollowCursor()
        {
            Vector2 mouse = Input.mousePosition;
            bool hasComparison = _comparisonPanel != null && _comparisonPanel.gameObject.activeSelf;

            Vector3[] corners = new Vector3[4];
            _panel.GetWorldCorners(corners);
            float w  = corners[2].x - corners[0].x;
            float h  = corners[1].y - corners[0].y;

            float cw = 0f, ch = 0f;
            if (hasComparison)
            {
                Vector3[] cmpCorners = new Vector3[4];
                _comparisonPanel.GetWorldCorners(cmpCorners);
                cw = cmpCorners[2].x - cmpCorners[0].x;
                ch = cmpCorners[1].y - cmpCorners[0].y;
            }

            // X: flip the pair as a unit when the combined width would go off-screen.
            float totalW = hasComparison ? w + _comparisonGap + cw : w;
            bool flipX = mouse.x + _cursorOffset.x + totalW > Screen.width;

            float primBlX, cmpBlX;
            if (!flipX)
            {
                primBlX = mouse.x + _cursorOffset.x;
                cmpBlX  = primBlX + w + _comparisonGap;
            }
            else
            {
                // Comparison sits where a single flipped tooltip would (right edge at cursor - offset).
                cmpBlX  = mouse.x - _cursorOffset.x - cw;
                primBlX = cmpBlX - _comparisonGap - w;
            }

            // Y: align both panels at the same bottom, flip when the taller one would go off-screen.
            float maxH = hasComparison ? Mathf.Max(h, ch) : h;
            float blY = mouse.y + _cursorOffset.y;
            if (blY + maxH > Screen.height)
                blY = mouse.y - _cursorOffset.y - maxH;

            SetPanelPosition(_panel, primBlX, blY, w, h);
            if (hasComparison)
                SetPanelPosition(_comparisonPanel, cmpBlX, blY, cw, ch);
        }

        private void SetPanelPosition(RectTransform panel, float blX, float blY, float w, float h)
        {
            float posX = blX + panel.pivot.x * w;
            float posY = blY + panel.pivot.y * h;

            if (_canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                panel.position = new Vector2(posX, posY);
            }
            else
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _canvas.transform as RectTransform,
                    new Vector2(posX, posY),
                    _canvas.worldCamera,
                    out Vector2 localPoint);
                panel.localPosition = localPoint;
            }
        }
    }
}
