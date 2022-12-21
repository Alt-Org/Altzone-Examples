using Prg.Scripts.Common.Util;
using UnityEngine;

internal static class BootLoader
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void BeforeSceneLoad()
    {
        var loggerConfig = ScriptableObject.CreateInstance<LoggerConfig>();
        loggerConfig._loggerRules = 
@"^Prg.*=1
^Tests\..*=1";
        loggerConfig._colorForClassName = "white";
        loggerConfig._isDefaultMatchTrue = true;
        loggerConfig._colorForContextTagName = "orange";
        loggerConfig._isLogNoNamespaceForced = true;
        LoggerConfig.CreateLoggerConfig(loggerConfig);
        
        Debug.Log("starting");
    }
}