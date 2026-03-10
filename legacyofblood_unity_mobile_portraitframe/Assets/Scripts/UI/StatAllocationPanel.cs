using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace LegendOfBlood
{
    public class StatAllocationPanel : MonoBehaviour
    {
        [Header("UI References")]
        public TextMeshProUGUI panelTitleText;
        public TextMeshProUGUI freePointsText;
        public Button confirmButton;
        public TextMeshProUGUI confirmButtonText;
        public Button closeButton;

        [Header("HP Row")]
        public TextMeshProUGUI hpValueText;
        public Button hpPlusBtn;
        public Button hpMinusBtn;

        [Header("ATK Row")]
        public TextMeshProUGUI atkValueText;
        public Button atkPlusBtn;
        public Button atkMinusBtn;

        [Header("DEF Row")]
        public TextMeshProUGUI defValueText;
        public Button defPlusBtn;
        public Button defMinusBtn;

        [Header("SPD Row")]
        public TextMeshProUGUI spdValueText;
        public Button spdPlusBtn;
        public Button spdMinusBtn;

        private HeroData _currentHero;
        private int _tempFreePoints;
        private float _tempHp, _tempAtk, _tempDef, _tempSpd;

        private void Awake()
        {
            AutoWire();
            
            if (closeButton) closeButton.onClick.AddListener(ClosePanel);
            if (confirmButton) confirmButton.onClick.AddListener(ConfirmAllocation);

            if (hpPlusBtn) hpPlusBtn.onClick.AddListener(() => AddStat(ref _tempHp, 1.5f)); // HP scale 1.5
            if (hpMinusBtn) hpMinusBtn.onClick.AddListener(() => RemoveStat(ref _tempHp, 1.5f, _currentHero.addedStats.hp));

            if (atkPlusBtn) atkPlusBtn.onClick.AddListener(() => AddStat(ref _tempAtk, 1f));
            if (atkMinusBtn) atkMinusBtn.onClick.AddListener(() => RemoveStat(ref _tempAtk, 1f, _currentHero.addedStats.atk));

            if (defPlusBtn) defPlusBtn.onClick.AddListener(() => AddStat(ref _tempDef, 1f));
            if (defMinusBtn) defMinusBtn.onClick.AddListener(() => RemoveStat(ref _tempDef, 1f, _currentHero.addedStats.def));

            if (spdPlusBtn) spdPlusBtn.onClick.AddListener(() => AddStat(ref _tempSpd, 1f));
            if (spdMinusBtn) spdMinusBtn.onClick.AddListener(() => RemoveStat(ref _tempSpd, 1f, _currentHero.addedStats.spd));
        }

        private void AutoWire()
        {
            // Utility cào cấu Hierarchy để tự sướng nếu user quên gán
            Button[] btns = GetComponentsInChildren<Button>(true);
            foreach (var b in btns)
            {
                string n = b.name.ToLower();
                if (!closeButton && (n.Contains("close") || n.Contains("back"))) closeButton = b;
                if (!confirmButton && (n.Contains("confirm") || n.Contains("ok") || n.Contains("apply"))) confirmButton = b;
                if (!hpPlusBtn && n.Contains("hp") && (n.Contains("plus") || n.Contains("add"))) hpPlusBtn = b;
                if (!hpMinusBtn && n.Contains("hp") && (n.Contains("minus") || n.Contains("sub"))) hpMinusBtn = b;
                if (!atkPlusBtn && n.Contains("atk") && (n.Contains("plus") || n.Contains("add"))) atkPlusBtn = b;
                if (!atkMinusBtn && n.Contains("atk") && (n.Contains("minus") || n.Contains("sub"))) atkMinusBtn = b;
                if (!defPlusBtn && n.Contains("def") && (n.Contains("plus") || n.Contains("add"))) defPlusBtn = b;
                if (!defMinusBtn && n.Contains("def") && (n.Contains("minus") || n.Contains("sub"))) defMinusBtn = b;
                if (!spdPlusBtn && n.Contains("spd") && (n.Contains("plus") || n.Contains("add"))) spdPlusBtn = b;
                if (!spdMinusBtn && n.Contains("spd") && (n.Contains("minus") || n.Contains("sub"))) spdMinusBtn = b;
            }

            TextMeshProUGUI[] txts = GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var t in txts)
            {
                string n = t.name.ToLower();
                if (!freePointsText && n.Contains("free")) freePointsText = t;
                if (!hpValueText && n.Contains("hp") && n.Contains("val")) hpValueText = t;
                if (!atkValueText && n.Contains("atk") && n.Contains("val")) atkValueText = t;
                if (!defValueText && n.Contains("def") && n.Contains("val")) defValueText = t;
                if (!spdValueText && n.Contains("spd") && n.Contains("val")) spdValueText = t;
            }
        }

        public void Show(HeroData hero)
        {
            _currentHero = hero;
            _tempFreePoints = hero.freeStatPoints;
            _tempHp = hero.addedStats.hp;
            _tempAtk = hero.addedStats.atk;
            _tempDef = hero.addedStats.def;
            _tempSpd = hero.addedStats.spd;

            UpdateUI();
            gameObject.SetActive(true);
        }

        private void AddStat(ref float statValue, float incrementAmount)
        {
            if (_tempFreePoints > 0)
            {
                _tempFreePoints--;
                statValue += incrementAmount;
                UpdateUI();
            }
        }

        private void RemoveStat(ref float statValue, float incrementAmount, float originalAddedStat)
        {
            if (statValue - incrementAmount >= originalAddedStat - 0.01f) // Float precision fix
            {
                _tempFreePoints++;
                statValue -= incrementAmount;
                UpdateUI();
            }
        }

        private void UpdateUI()
        {
            if (panelTitleText) panelTitleText.text = global::LocalizationSystem.GetText("panel_title_stat_alloc");
            if (freePointsText) freePointsText.text = string.Format(global::LocalizationSystem.GetText("label_free_points"), _tempFreePoints);
            if (confirmButtonText) confirmButtonText.text = global::LocalizationSystem.GetText("btn_confirm");

            if (hpValueText) hpValueText.text = $"{_currentHero.baseStats.hp + _tempHp:F0} (+{_tempHp:F0})";
            if (atkValueText) atkValueText.text = $"{_currentHero.baseStats.atk + _tempAtk:F0} (+{_tempAtk:F0})";
            if (defValueText) defValueText.text = $"{_currentHero.baseStats.def + _tempDef:F0} (+{_tempDef:F0})";
            if (spdValueText) spdValueText.text = $"{_currentHero.baseStats.spd + _tempSpd:F0} (+{_tempSpd:F0})";
        }

        private void ConfirmAllocation()
        {
            _currentHero.freeStatPoints = _tempFreePoints;
            _currentHero.addedStats.hp = _tempHp;
            _currentHero.addedStats.atk = _tempAtk;
            _currentHero.addedStats.def = _tempDef;
            _currentHero.addedStats.spd = _tempSpd;

            // Update UI ngầm
            EventManager.TriggerEvent(GameEvents.OnHeroListChanged);
            EventManager.TriggerEvent(GameEvents.OnHeroCardClicked, _currentHero);
            ClosePanel();
        }

        public void ClosePanel()
        {
            gameObject.SetActive(false);
        }
    }
}
