using UnityEngine;
using UnityEngine.UI;

namespace GameUi.Scripts.LootLocker
{
    public class LootLockerView : MonoBehaviour
    {
        [SerializeField] private Text _infoLabel;
        [SerializeField] private Text _resultLabel;

        [SerializeField] private Button _button1;
        [SerializeField] private Button _button2;
        [SerializeField] private Button _button3;

        public string InfoLabel
        {
            set => _infoLabel.text = value;
        }

        public string ResultLabel
        {
            set => _resultLabel.text = value;
        }

        public Button TestButton1 => _button1;
        public Button TestButton2 => _button2;
        public Button TestButton3 => _button3;

        public void Reset()
        {
            InfoLabel = string.Empty;
            ResultLabel = string.Empty;
        }
    }
}