using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

/// <summary>
/// UnityEngine.Debug (thread-safe) wrapper for development (and optionally for production testing).
/// </summary>
/// <remarks>
/// Using <c>Conditional</c> attribute to disable logging unless compiled with <b>UNITY_EDITOR</b> or <b>FORCE_LOG defines</b>.
/// </remarks>
[DefaultExecutionOrder(-100), SuppressMessage("ReSharper", "CheckNamespace")]
public static class Debug
{
    // See: https://answers.unity.com/questions/126315/debuglog-in-build.html
    // StackFrame: https://stackoverflow.com/questions/21884142/difference-between-declaringtype-and-reflectedtype
    // Method: https://stackoverflow.com/questions/2483023/how-to-test-if-a-type-is-anonymous

#if FORCE_LOG
#warning <b>NOTE</b>: Compiling WITH debug logging define <b>FORCE_LOG</b>
#endif

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void SubsystemRegistration()
    {
        // Manual reset if UNITY Domain Reloading is disabled.
        _mainThreadId = Thread.CurrentThread.ManagedThreadId;
        _currentFrameCount = 0;
        RemoveTags();
        _logLineAllowedFilter = null;
        CachedLogLineMethods.Clear();
        SetEditorStatus();
    }

    [Conditional("UNITY_EDITOR")]
    private static void SetEditorStatus()
    {
        // Reset log line filtering and caching in Editor when we switch form Player Mode to Edit Mode.
#if UNITY_EDITOR
        void LogPlayModeState(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                _logLineAllowedFilter = null;
                CachedLogLineMethods.Clear();
            }
        }

        if (_isEditorHook)
        {
            return;
        }
        _isEditorHook = true;
        EditorApplication.playModeStateChanged += LogPlayModeState;
#endif
    }

    #region Log formatting and filtering support

    private static bool _isClassNamePrefix;
    private static string _prefixTag;
    private static string _suffixTag;
    private static string _contextTag = string.Empty;

    private static bool _isEditorHook;
    private static int _mainThreadId;
    private static int _currentFrameCount;

    private static void RemoveTags()
    {
        _isClassNamePrefix = false;
        _prefixTag = null;
        _suffixTag = null;
        _contextTag = string.Empty;
    }

    /// <summary>
    /// Filters log lines based on method class, name or other method properties.
    /// </summary>
    private static Func<MethodBase, bool> _logLineAllowedFilter;

    /// <summary>
    /// Cache for tracking whether method should be logged or not.
    /// </summary>
    private static readonly Dictionary<MethodBase, bool> CachedLogLineMethods = new();

    /// <summary>
    /// Adds log line filter.
    /// </summary>
    [Conditional("UNITY_EDITOR"), Conditional("FORCE_LOG")]
    public static void AddLogLineAllowedFilter(Func<MethodBase, bool> filter)
    {
        Assert.AreEqual(_mainThreadId, Thread.CurrentThread.ManagedThreadId);
        Assert.IsNull(_logLineAllowedFilter);
        _logLineAllowedFilter = filter;
    }

    /// <summary>
    /// Sets tag markers for class name field in debug log line.
    /// </summary>
    /// <remarks>
    /// See: https://docs.unity3d.com/Packages/com.unity.ugui@1.0/manual/StyledText.html
    /// and
    /// https://docs.unity3d.com/Packages/com.unity.ugui@1.0/manual/StyledText.html#ColorNames
    /// </remarks>
    [Conditional("UNITY_EDITOR"), Conditional("FORCE_LOG")]
    public static void SetTagsForClassName(string prefixTag, string suffixTag)
    {
        _isClassNamePrefix = !string.IsNullOrEmpty(prefixTag) || string.IsNullOrEmpty(suffixTag);
        _prefixTag = prefixTag ?? string.Empty;
        _suffixTag = suffixTag ?? string.Empty;
    }

    public static void SetContextTag(string contextTag)
    {
        _contextTag = contextTag;
    }

    private static int GetSafeFrameCount()
    {
        if (_mainThreadId == Thread.CurrentThread.ManagedThreadId)
        {
            _currentFrameCount = Time.frameCount % 1000;
        }
        return _currentFrameCount;
    }

    #endregion

    #region DUPLICATED : Just for compability in UnityEngine namespace

    public static void Break() => UnityEngine.Debug.Break();
    public static void DebugBreak() => UnityEngine.Debug.DebugBreak();

    [Conditional("UNITY_EDITOR")]
    public static void DrawLine(Vector3 start, Vector3 end, Color color, float duration, bool depthTest) =>
        UnityEngine.Debug.DrawLine(start, end, color, duration, depthTest);

    [Conditional("UNITY_EDITOR")]
    public static void DrawLine(Vector3 start, Vector3 end, Color color, float duration) => UnityEngine.Debug.DrawLine(start, end, color, duration);

    [Conditional("UNITY_EDITOR")]
    public static void DrawLine(Vector3 start, Vector3 end, Color color) => UnityEngine.Debug.DrawLine(start, end, color);

    [Conditional("UNITY_EDITOR")]
    public static void DrawLine(Vector3 start, Vector3 end) => UnityEngine.Debug.DrawLine(start, end);

    [Conditional("UNITY_EDITOR")]
    public static void DrawRay(Vector3 start, Vector3 dir, Color color, float duration, bool depthTest) =>
        UnityEngine.Debug.DrawRay(start, dir, color, duration, depthTest);

    [Conditional("UNITY_EDITOR")]
    public static void DrawRay(Vector3 start, Vector3 dir, Color color, float duration) => UnityEngine.Debug.DrawRay(start, dir, color, duration);

    [Conditional("UNITY_EDITOR")]
    public static void DrawRay(Vector3 start, Vector3 dir, Color color) => UnityEngine.Debug.DrawRay(start, dir, color);

    [Conditional("UNITY_EDITOR")]
    public static void DrawRay(Vector3 start, Vector3 dir) => UnityEngine.Debug.DrawRay(start, dir);

    #endregion

    #region DUPLICATED : Actual UnityEngine.Debug Logging API

    /// <summary>
    /// Logs a string message.
    /// </summary>
    /// <remarks>
    /// Note that string interpolation using $ can be expensive and should be avoided if logging is intended for production builds.<br />
    /// It is bettor to use <c>LogFormat</c> 'composite formatting' to delay actual string formatting when (if) message is logged.
    /// </remarks>
    [Conditional("UNITY_EDITOR"), Conditional("FORCE_LOG")]
    public static void Log(string message, Object context = null, [CallerMemberName] string memberName = null)
    {
        var frame = new StackFrame(1);
        var method = frame.GetMethod();
        if (method == null || method.ReflectedType == null)
        {
            UnityEngine.Debug.unityLogger.Log(LogType.Log, (object)message, context);
            return;
        }
        if (!IsMethodAllowedForLog(method))
        {
            return;
        }
        var prefix = GetPrefix(method, memberName);
        if (AppPlatform.IsEditor)
        {
            var contextTag = context != null ? _contextTag : string.Empty;
            UnityEngine.Debug.unityLogger.Log(LogType.Log, (object)$"{prefix}{message}{contextTag}", context);
        }
        else
        {
            UnityEngine.Debug.unityLogger.Log(LogType.Log, (object)$"{prefix}{message}", context);
        }
    }

    /// <summary>
    /// Logs a string message using 'composite formatting'.
    /// </summary>
    [Conditional("UNITY_EDITOR"), Conditional("FORCE_LOG")]
    public static void LogFormat(string format, params object[] args)
    {
        var frame = new StackFrame(1);
        var method = frame.GetMethod();
        if (method == null || method.ReflectedType == null)
        {
            UnityEngine.Debug.unityLogger.LogFormat(LogType.Log, format, args);
            return;
        }
        if (!IsMethodAllowedForLog(method))
        {
            return;
        }
        UnityEngine.Debug.unityLogger.LogFormat(LogType.Log, $"{GetPrefix(method)}{format}", args);
    }

    [Conditional("UNITY_EDITOR"), Conditional("FORCE_LOG")]
    public static void LogFormat(Object context, string format, params object[] args)
    {
        var frame = new StackFrame(1);
        var method = frame.GetMethod();
        if (method == null || method.ReflectedType == null)
        {
            UnityEngine.Debug.unityLogger.LogFormat(LogType.Log, context, format, args);
            return;
        }
        if (!IsMethodAllowedForLog(method))
        {
            return;
        }
        UnityEngine.Debug.unityLogger.LogFormat(LogType.Log, context, $"{GetPrefix(method)}{format}", args);
    }

    [Conditional("UNITY_EDITOR"), Conditional("FORCE_LOG")]
    public static void LogWarning(string message, Object context = null)
    {
        UnityEngine.Debug.unityLogger.Log(LogType.Warning, (object)message, context);
    }

    [Conditional("UNITY_EDITOR"), Conditional("FORCE_LOG")]
    public static void LogWarningFormat(string format, params object[] args)
    {
        UnityEngine.Debug.unityLogger.LogFormat(LogType.Warning, format, args);
    }

    [Conditional("UNITY_EDITOR"), Conditional("FORCE_LOG")]
    public static void LogWarningFormat(Object context, string format, params object[] args)
    {
        UnityEngine.Debug.unityLogger.LogFormat(LogType.Warning, context, format, args);
    }

    public static void LogError(object message) => UnityEngine.Debug.unityLogger.Log(LogType.Error, message);

    public static void LogError(string message, Object context = null)
    {
        UnityEngine.Debug.unityLogger.Log(LogType.Error, (object)message, context);
    }

    public static void LogErrorFormat(string format, params object[] args)
    {
        UnityEngine.Debug.unityLogger.LogFormat(LogType.Error, format, args);
    }

    public static void LogErrorFormat(Object context, string format, params object[] args)
    {
        UnityEngine.Debug.unityLogger.LogFormat(LogType.Error, context, format, args);
    }

    public static void LogException(Exception exception)
    {
        UnityEngine.Debug.unityLogger.LogException(exception);
    }

    #endregion

    #region Static helpers

    private static string GetPrefix(MemberInfo method, string memberName = null)
    {
        var className = method.ReflectedType?.Name ?? nameof(Debug);
        if (className.StartsWith("<"))
        {
            // For anonymous types we try its parent type.
            className = method.ReflectedType?.DeclaringType?.Name ?? nameof(Debug);
        }
        var methodName = method.Name;
        if (methodName.StartsWith("<"))
        {
            // Local methods are compiled to internal static methods with a name of the following form:
            // <Name1>g__Name2|x_y
            // Name1 is the name of the surrounding method. Name2 is the name of the local method.
            const string methodPrefix = ">g__";
            const string methodSuffix = "|";
            var pos1 = methodName.IndexOf(methodPrefix, StringComparison.Ordinal);
            if (pos1 > 0)
            {
                pos1 += methodPrefix.Length;
                var pos2 = methodName.IndexOf(methodSuffix, pos1, StringComparison.Ordinal);
                if (pos2 > 0)
                {
                    var localName = $">{methodName.Substring(pos1, pos2 - pos1)}";
                    if (memberName == null)
                    {
                        memberName = localName;
                    }
                    else
                    {
                        memberName += localName;
                    }
                }
            }
        }
        var frameCount = GetSafeFrameCount();
        if (memberName != null)
        {
            return _isClassNamePrefix
                ? $"{frameCount} {_prefixTag}{className}.{memberName}{_suffixTag} "
                : $"{frameCount} [{className}.{memberName}] ";
        }
        return _isClassNamePrefix
            ? $"{frameCount} {_prefixTag}{className}{_suffixTag} "
            : $"{frameCount} [{className}] ";
    }

    private static bool IsMethodAllowedForLog(MethodBase method)
    {
        if (_logLineAllowedFilter == null)
        {
            return true;
        }
        if (CachedLogLineMethods.TryGetValue(method, out var isAllowed))
        {
            return isAllowed;
        }
        isAllowed = _logLineAllowedFilter(method);
        TryAddCachedMethod();
        return isAllowed;

        // Local function to make code more readable.
        void TryAddCachedMethod()
        {
            // UnityEngine.Debug.Log(isAllowed
            //     ? $"[{RichText.Brown("ACCEPT")}] {method.Name} in {method.ReflectedType?.FullName}"
            //     : $"[{RichText.Brown("REJECT")}] {method.Name} in {method.ReflectedType?.FullName}");
            // Dictionary is not thread safe - so we guard it without locking!
            if (CachedLogLineMethods.ContainsKey(method))
            {
                return;
            }
            // On rare cases some other thread might add the same method before us - and it is ok.
            try
            {
                CachedLogLineMethods.Add(method, isAllowed);
            }
            catch (ArgumentException)
            {
                // Swallow and ignore - An element with the same key already exists (has been added just before us).
            }
        }
    }

    #endregion
}
