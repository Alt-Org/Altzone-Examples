using System;
using System.Collections;
using Altzone.Scripts.Config;
using Prg.Scripts.Service.BrainCloud;
using UnityEngine;

namespace GameUi.Scripts.BrainCloud
{
    [RequireComponent(typeof(BrainCloudUiView))]
    public class BrainCloudUiController : MonoBehaviour
    {
        [SerializeField] private BrainCloudUiView _view;

        private void OnEnable()
        {
            _view.ResetView();

            var playerData = RuntimeGameConfig.Get().PlayerDataCache;
            _view.PlayerInfo = playerData.PlayerName;

            _view.LoginButton.onClick.AddListener(OnLoginButton);
            _view.UpdateUserNameButton.onClick.AddListener(OnUpdateUserNameButton);

            var brainCLoud = BrainCloudService.Get();
            var isBrainCLoud = brainCLoud != null;
            var isBrainCLoudUser = isBrainCLoud && brainCLoud.IsReady;
            if (isBrainCLoudUser)
            {
                _view.BrainCloudInfo = $"User: {brainCLoud.BrainCloudUser.UserName}";
                SetButtonStates(true);
                return;
            }
            if (isBrainCLoud)
            {
                _view.BrainCloudInfo = "Wait";
                return;
            }
            _view.BrainCloudInfo = "Connecting";
            StartCoroutine(StartBrainCLoud());
        }

        private IEnumerator StartBrainCLoud()
        {
            Debug.Log($"StartBrainCLoud start");
            var brainCLoud = BrainCloudService.Create();
            yield return new WaitUntil(() => brainCLoud.IsReady);
            _view.BrainCloudInfo = "Ready to login";
            SetButtonStates(false);
            Debug.Log($"StartBrainCLoud done");
        }

        private void SetButtonStates(bool isBrainCLoud)
        {
            _view.LoginButton.interactable = !isBrainCLoud;
            _view.UpdateUserNameButton.interactable = isBrainCLoud;
        }

        private async void OnLoginButton()
        {
            Debug.Log($"OnLoginButton");
            var brainCLoud = BrainCloudService.Get();
            var (userId, password) = BrainCloudSupport.GetCredentials();
            var isSuccess = await brainCLoud.Authenticate(userId, password);
            if (isSuccess)
            {
                _view.BrainCloudInfo = $"User: {brainCLoud.BrainCloudUser.UserName}";
                SetButtonStates(true);
                return;
            }
            _view.BrainCloudInfo = "Login failed";
        }

        private async void OnUpdateUserNameButton()
        {
            var playerData = RuntimeGameConfig.Get().PlayerDataCache;
            _view.PlayerInfo = playerData.PlayerName;
            Debug.Log($"OnUpdateUserNameButton '{playerData.PlayerName}'");
            var brainCLoud = BrainCloudService.Get();
            var isSuccess = await brainCLoud.UpdateUserName(playerData.PlayerName);
            if (isSuccess)
            {
                _view.BrainCloudInfo = $"User: {brainCLoud.BrainCloudUser.UserName}";
            }
        }
    }
}