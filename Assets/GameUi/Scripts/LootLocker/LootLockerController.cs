using Altzone.Scripts.Service.LootLocker;
using UnityEngine;

namespace GameUi.Scripts.LootLocker
{
    public class LootLockerController : MonoBehaviour
    {
        [SerializeField] private LootLockerView _view;

        private void OnEnable()
        {
            _view.Reset();
            if (!LootLockerWrapper.IsRunning)
            {
                _view.InfoLabel = "LootLocker is not running";
                return;
            }
            _view.InfoLabel = $"{LootLockerWrapper.PlayerName}";
        }
    }
}