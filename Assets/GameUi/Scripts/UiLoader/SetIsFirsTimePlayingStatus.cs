using Altzone.Scripts.Config;
using UnityEngine;

namespace GameUi.Scripts.UiLoader
{
    public class SetIsFirsTimePlayingStatus : MonoBehaviour
    {
        private void Awake()
        {
            Debug.Log("SetIsFirsTimePlayingStatus");
            RuntimeGameConfig.SetIsFirsTimePlayingStatus();
        }
    }
}
