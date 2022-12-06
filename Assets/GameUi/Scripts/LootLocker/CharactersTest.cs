using UnityEngine;
using UnityEngine.UI;
#if USE_LOOTLOCKER
using LootLocker.Requests;
#endif

namespace GameUi.Scripts.LootLocker
{
    public class CharactersTest : MonoBehaviour
    {
        private const string WaitText = "Wait";

        [SerializeField] private GameObject _content;
        [SerializeField] private Text _resultLabel;

        [SerializeField] private Button _buttonCharacterTypes;
        [SerializeField] private Button _buttonLoadouts;

        private void Awake()
        {
            Debug.Log("");
            _buttonCharacterTypes.onClick.AddListener(ListCharacterTypes);
            _buttonLoadouts.onClick.AddListener(ListLoadouts);
            enabled = false;
        }

        private void OnEnable()
        {
            Debug.Log("");
            _resultLabel.text = GetType().Name;
            _content.SetActive(true);
        }

        private void OnDisable()
        {
            Debug.Log("");
            _resultLabel.text = string.Empty;
            _content.SetActive(false);
            enabled = false;
        }

        private void ListCharacterTypes()
        {
            Debug.Log("");
#if USE_LOOTLOCKER
            _buttonCharacterTypes.interactable = false;
            _resultLabel.text = WaitText;
            LootLockerSDKManager.ListCharacterTypes((response) =>
            {
                _buttonCharacterTypes.interactable = true;
                Debug.Log($"{(response.success ? response.text : response.Error)}");
                if (!response.success)
                {
                    _resultLabel.text = "Failed";
                    return;
                }
                var characterTypes = response.character_types;
                _resultLabel.text = $"characterTypes {characterTypes.Length}";
            });
#endif
        }

        private void ListLoadouts()
        {
            Debug.Log("");
#if USE_LOOTLOCKER
            _buttonLoadouts.interactable = false;
            _resultLabel.text = WaitText;
            LootLockerSDKManager.GetCharacterLoadout((response) =>
            {
                _buttonLoadouts.interactable = false;
                Debug.Log($"{(response.success ? response.text : response.Error)}");
                if (!response.success)
                {
                    _resultLabel.text = "Failed";
                    return;
                }
                var characters = response.GetCharacters();
                var loadouts = response.loadouts;
                _resultLabel.text = $"characters {characters.Length} loadouts {loadouts.Length}";
            });
#endif
        }
    }
}