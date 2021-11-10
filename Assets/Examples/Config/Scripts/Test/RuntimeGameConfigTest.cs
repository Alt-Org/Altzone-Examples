using UnityEngine;

namespace Examples.Config.Scripts.Test
{
    public class RuntimeGameConfigTest : MonoBehaviour
    {
        public bool synchronizeAll;
        public bool synchronizeFeatures;
        public bool synchronizeVariables;

#if UNITY_EDITOR
        private void Update()
        {
            if (synchronizeAll)
            {
                synchronizeAll = false;
                GameConfigSynchronizer.Synchronize(What.All);
                return;
            }
            if (synchronizeFeatures)
            {
                synchronizeFeatures = false;
                GameConfigSynchronizer.Synchronize(What.Features);
                return;
            }
            if (synchronizeVariables)
            {
                synchronizeVariables = false;
                GameConfigSynchronizer.Synchronize(What.Variables);
            }
        }
#endif
    }
}