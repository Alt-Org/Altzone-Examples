using UnityEngine;

namespace Prg.Scripts.Common.Unity
{
    [CreateAssetMenu(menuName = "ALT-Zone/ScoreFlashConfig", fileName = "ScoreFlashConfig")]
    public class ScoreFlashConfig : ScriptableObject
    {
        public Canvas _canvasPrefab;
    }
}