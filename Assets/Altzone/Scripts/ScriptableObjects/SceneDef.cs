﻿using Prg.Scripts.Common.Unity;
using UnityEngine;

namespace Altzone.Scripts.ScriptableObjects
{
    [CreateAssetMenu(menuName = "ALT-Zone/SceneDef", fileName = "scene-")]
    public class SceneDef : ScriptableObject
    {
        [SerializeField] private UnitySceneName _sceneName;
        [SerializeField] private bool _isNetworkScene;

        public string SceneName => _sceneName.sceneName;
        public bool IsNetworkScene => _isNetworkScene;
    }
}