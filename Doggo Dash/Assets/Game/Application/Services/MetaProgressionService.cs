using System;
using System.Collections.Generic;
using Game.Application.Ports;

namespace Game.Application.Services
{
    public enum MetaUpgradeType
    {
        EnergyMax,
        StartSpeed,
        GemBonus
    }

    public enum MetaUpgradeCurrency
    {
        Kibble,
        Gems
    }

    public enum MetaUpgradePurchaseResult
    {
        Purchased,
        NotEnoughCurrency,
        Maxed
    }

    public readonly struct MetaUpgradeDefinition
    {
        public MetaUpgradeDefinition(
            MetaUpgradeType type,
            string displayName,
            MetaUpgradeCurrency currency,
            int baseCost,
            int costPerLevel,
            int maxLevel,
            float valuePerLevel)
        {
            Type = type;
            DisplayName = displayName;
            Currency = currency;
            BaseCost = baseCost;
            CostPerLevel = costPerLevel;
            MaxLevel = maxLevel;
            ValuePerLevel = valuePerLevel;
        }

        public MetaUpgradeType Type { get; }
        public string DisplayName { get; }
        public MetaUpgradeCurrency Currency { get; }
        public int BaseCost { get; }
        public int CostPerLevel { get; }
        public int MaxLevel { get; }
        public float ValuePerLevel { get; }

        public int GetCost(int currentLevel) => BaseCost + (CostPerLevel * currentLevel);
    }

    public sealed class MetaProgressionService
    {
        private const float BaseEnergyMax = 100f;
        private const float EnergyMaxPerLevel = 10f;
        private const float StartSpeedPerLevel = 0.25f;
        private const float GemBonusPerLevel = 0.05f;

        private static readonly Dictionary<MetaUpgradeType, MetaUpgradeDefinition> Definitions = new()
        {
            {
                MetaUpgradeType.EnergyMax,
                new MetaUpgradeDefinition(
                    MetaUpgradeType.EnergyMax,
                    "Energy Capacity",
                    MetaUpgradeCurrency.Kibble,
                    baseCost: 50,
                    costPerLevel: 25,
                    maxLevel: 10,
                    valuePerLevel: EnergyMaxPerLevel)
            },
            {
                MetaUpgradeType.StartSpeed,
                new MetaUpgradeDefinition(
                    MetaUpgradeType.StartSpeed,
                    "Start Speed",
                    MetaUpgradeCurrency.Kibble,
                    baseCost: 75,
                    costPerLevel: 50,
                    maxLevel: 8,
                    valuePerLevel: StartSpeedPerLevel)
            },
            {
                MetaUpgradeType.GemBonus,
                new MetaUpgradeDefinition(
                    MetaUpgradeType.GemBonus,
                    "Gem Bonus",
                    MetaUpgradeCurrency.Gems,
                    baseCost: 2,
                    costPerLevel: 2,
                    maxLevel: 5,
                    valuePerLevel: GemBonusPerLevel)
            }
        };

        private readonly IProgressSaveGateway _save;
        private PlayerProgressData _data;

        public MetaProgressionService(IProgressSaveGateway save)
        {
            _save = save ?? throw new ArgumentNullException(nameof(save));
            _data = _save.Load();
            ApplyUpgrades();
            ProgressClampUtility.ClampProgress(_data);
        }

        public PlayerProgressData Data => _data;

        public void Reload()
        {
            _data = _save.Load();
            ApplyUpgrades();
            ProgressClampUtility.ClampProgress(_data);
        }

        public MetaUpgradeDefinition GetDefinition(MetaUpgradeType type) => Definitions[type];

        public int GetUpgradeLevel(MetaUpgradeType type)
        {
            return type switch
            {
                MetaUpgradeType.EnergyMax => _data.upgradeEnergyMax,
                MetaUpgradeType.StartSpeed => _data.upgradeStartSpeed,
                MetaUpgradeType.GemBonus => _data.upgradeGemBonus,
                _ => 0
            };
        }

        public int GetUpgradeCost(MetaUpgradeType type)
        {
            MetaUpgradeDefinition def = GetDefinition(type);
            int level = GetUpgradeLevel(type);
            if (level >= def.MaxLevel) return 0;
            return def.GetCost(level);
        }

        public bool IsMaxed(MetaUpgradeType type)
        {
            MetaUpgradeDefinition def = GetDefinition(type);
            return GetUpgradeLevel(type) >= def.MaxLevel;
        }

        public bool CanAfford(MetaUpgradeType type)
        {
            if (IsMaxed(type)) return false;
            int cost = GetUpgradeCost(type);
            MetaUpgradeDefinition def = GetDefinition(type);
            return def.Currency switch
            {
                MetaUpgradeCurrency.Kibble => _data.totalKibble >= cost,
                MetaUpgradeCurrency.Gems => _data.totalGems >= cost,
                _ => false
            };
        }

        public MetaUpgradePurchaseResult TryPurchaseUpgrade(MetaUpgradeType type)
        {
            if (IsMaxed(type)) return MetaUpgradePurchaseResult.Maxed;
            MetaUpgradeDefinition def = GetDefinition(type);
            int cost = GetUpgradeCost(type);

            if (!TrySpend(def.Currency, cost))
            {
                return MetaUpgradePurchaseResult.NotEnoughCurrency;
            }

            IncrementUpgrade(type);
            ApplyUpgrades();
            Save();
            return MetaUpgradePurchaseResult.Purchased;
        }

        public float GetStartSpeedBonus() => _data.upgradeStartSpeed * StartSpeedPerLevel;

        public float GetGemBonusMultiplier() => 1f + (_data.upgradeGemBonus * GemBonusPerLevel);

        public void ApplyUpgrades()
        {
            _data.energyMax = BaseEnergyMax + (_data.upgradeEnergyMax * EnergyMaxPerLevel);
            if (_data.energyCurrent > _data.energyMax)
            {
                _data.energyCurrent = _data.energyMax;
            }
        }

        private void IncrementUpgrade(MetaUpgradeType type)
        {
            switch (type)
            {
                case MetaUpgradeType.EnergyMax:
                    _data.upgradeEnergyMax++;
                    break;
                case MetaUpgradeType.StartSpeed:
                    _data.upgradeStartSpeed++;
                    break;
                case MetaUpgradeType.GemBonus:
                    _data.upgradeGemBonus++;
                    break;
            }
        }

        private bool TrySpend(MetaUpgradeCurrency currency, int amount)
        {
            if (amount <= 0) return true;

            switch (currency)
            {
                case MetaUpgradeCurrency.Kibble:
                    if (_data.totalKibble < amount) return false;
                    _data.totalKibble -= amount;
                    return true;
                case MetaUpgradeCurrency.Gems:
                    if (_data.totalGems < amount) return false;
                    _data.totalGems -= amount;
                    return true;
                default:
                    return false;
            }
        }

        private void Save()
        {
            ProgressClampUtility.ClampProgress(_data);
            _save.Save(_data);
        }
    }
}
