using UnityEngine;
using TMPro;

namespace Game.Presentation.Runtime.Missions
{
    public sealed class MissionHudPresenterBehaviour : MonoBehaviour
    {
        public MissionSystemBehaviour missionSystem = default!;

        public TMP_Text mission1Text;
        public TMP_Text mission2Text;
        public TMP_Text mission3Text;
        public TMP_Text multiplierText;

        private void Update()
        {
            if (missionSystem == null) return;

            var list = missionSystem.Manager.Active;

            Set(mission1Text, list, 0);
            Set(mission2Text, list, 1);
            Set(mission3Text, list, 2);

            if (multiplierText != null)
                multiplierText.text = $"x{missionSystem.ScoreMultiplier}";
        }

        private static void Set(TMP_Text text, System.Collections.Generic.IReadOnlyList<Game.Application.Services.MissionManager.ActiveMission> list, int i)
        {
            if (text == null) return;
            if (list == null || i >= list.Count) { text.text = "-"; return; }

            var m = list[i];
            string done = m.Progress.Completed ? "✅" : "";
            text.text = $"{m.Definition.displayName}: {m.Progress.Current}/{m.Progress.Target} {done}";
        }
    }
}
