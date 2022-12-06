using UnityEngine;
using UnityEngine.UI;

namespace GameUi.Scripts.LootLocker
{
    public class LootLockerView : MonoBehaviour
    {
        [SerializeField] private Text _infoLabel;
        
        public string InfoLabel
        {
            set => _infoLabel.text = value;
        }
        
        public void Reset()
        {
            InfoLabel = string.Empty;
        }
    }
}