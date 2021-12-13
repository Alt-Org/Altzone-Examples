using Altzone.Scripts.Config;
using UnityEngine;

namespace GameUi.Scripts.MainMenu
{
    public class RemoveIsFirsTimePlayingStatus : MonoBehaviour
    {
        private void Awake()
        {
            Debug.Log("RemoveIsFirsTimePlayingStatus");
            RuntimeGameConfig.RemoveIsFirsTimePlayingStatus();
        }
    }
}