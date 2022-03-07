using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Prg.Scripts.Service.FMOD
{
    /// <summary>
    /// AudioManager using FMOD
    /// </summary>
    public class FMODManager : MonoBehaviour
    {
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void BeforeSceneLoad()
        {
            var fMODManager = FindObjectOfType<FMODManager>();
            if (fMODManager == null)
            {
                
                UnityExtensions.CreateGameObjectAndComponent<FMODManager>(nameof(FMODManager), true);
            }
        }

    }
}
