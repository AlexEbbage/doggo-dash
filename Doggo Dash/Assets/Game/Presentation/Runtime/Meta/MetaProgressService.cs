using Game.Application.Ports;
using Game.Application.Services;
using Game.Infrastructure.Persistence;

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
            ProgressClampUtility.ClampProgress(_data);
        }

        public PlayerProgressData Data => _data;

        public void Reload()
        {
            _data = _save.Load();
            ProgressClampUtility.ClampProgress(_data);
        }

        public void Save()
        {
            ProgressClampUtility.ClampProgress(_data);
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

        public void SetSelectedPet(string petId)
        {
            _data.selectedPetId = string.IsNullOrWhiteSpace(petId) ? "dog_default" : petId;
            Save();
        }

        public void SetSelectedOutfit(string outfitId)
        {
            _data.selectedOutfitId = string.IsNullOrWhiteSpace(outfitId) ? "outfit_default" : outfitId;
            Save();
        }
    }
}
