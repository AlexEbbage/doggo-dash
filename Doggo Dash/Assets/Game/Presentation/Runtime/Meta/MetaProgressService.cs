using Game.Application.Ports;
using Game.Application.Services;
using Game.Infrastructure.Persistence;
using System.Collections.Generic;

namespace Game.Presentation.Runtime.Meta
{
    public sealed class MetaProgressService
    {
        private readonly IProgressSaveGateway _save;
        private PlayerProgressData _data;

        public MetaProgressService(IProgressSaveGateway save)
        {
            _save = save;
            _data = _save.Load();
            ClampProgressSafely();
        }

        public PlayerProgressData Data => _data;

        public void Reload()
        {
            _data = _save.Load();
            ClampProgressSafely();
        }

        public void Save()
        {
            ClampProgressSafely();
            _save.Save(_data);
        }

        public bool SpendKibble(int amount)
        {
            if (amount <= 0) return true;
            if (_data.totalKibble < amount) return false;
            _data.totalKibble -= amount;
            Save();
            return true;
        }

        public bool SpendGems(int amount)
        {
            if (amount <= 0) return true;
            if (_data.totalGems < amount) return false;
            _data.totalGems -= amount;
            Save();
            return true;
        }

        public void AddGems(int amount)
        {
            if (amount <= 0) return;
            _data.totalGems += amount;
            Save();
        }

        public void SetSelectedPet(string petId)
        {
            string resolvedId = string.IsNullOrWhiteSpace(petId) ? PlayerProgressData.DefaultPetId : petId;
            bool changed = _data.selectedPetId != resolvedId;
            _data.selectedPetId = resolvedId;
            changed |= EnsureOwned(ShopItemType.Pet, resolvedId);
            if (changed) Save();
        }

        public void SetSelectedOutfit(string outfitId)
        {
            string resolvedId = string.IsNullOrWhiteSpace(outfitId) ? PlayerProgressData.DefaultOutfitId : outfitId;
            bool changed = _data.selectedOutfitId != resolvedId;
            _data.selectedOutfitId = resolvedId;
            changed |= EnsureOwned(ShopItemType.Outfit, resolvedId);
            if (changed) Save();
        }

        public bool IsOwned(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId)) return false;
            return IsOwnedSafe(ShopItemType.Pet, itemId) || IsOwnedSafe(ShopItemType.Outfit, itemId);
        }

        public bool IsOwned(ShopItemType type, string itemId)
        {
            return IsOwnedSafe(type, itemId);
        }

        public void GrantOwnership(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId)) return;
            GrantOwnership(ShopItemType.Pet, itemId);
            GrantOwnership(ShopItemType.Outfit, itemId);
        }

        public void GrantOwnership(ShopItemType type, string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId)) return;
            if (type == ShopItemType.GemPack) return;
            if (EnsureOwned(type, itemId))
                Save();
        }

        private bool EnsureOwned(ShopItemType type, string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId)) return false;
            List<string> list = GetOwnedList(type);
            if (list.Contains(itemId)) return false;
            list.Add(itemId);
            return true;
        }

        private void ClampProgressSafely()
        {
            if (_data == null) return;
            ProgressClampUtility.ClampProgress(_data);
        }

        private bool IsOwnedSafe(ShopItemType type, string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId)) return false;
            if (type == ShopItemType.GemPack) return false;
            List<string> list = GetOwnedList(type);
            return list != null && list.Contains(itemId);
        }

        private List<string> GetOwnedList(ShopItemType type)
        {
            if (type == ShopItemType.Outfit)
            {
                _data.ownedOutfits ??= new List<string>();
                return _data.ownedOutfits;
            }

            _data.ownedPets ??= new List<string>();
            return _data.ownedPets;
        }
    }
}
