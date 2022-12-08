using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

/// <summary>
/// Conditional UnityEngine.Debug wrapper for development.
/// </summary>
[DefaultExecutionOrder(-100)]
public static class Debug
{
    // See: https://answers.unity.com/questions/126315/debuglog-in-build.html
    // StackFrame: https://stackoverflow.com/questions/21884142/difference-between-declaringtype-and-reflectedtype
    // Method: https://stackoverflow.com/questions/2483023/how-to-test-if-a-type-is-anonymous

#if FORCE_LOG
#warning NOTE: Compiling WITH debug logging FORCE_LOG
#endif

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void SubsystemRegistration()
    {
        // Manual reset if UNITY Domain Reloading is disabled.
        _logLineAllowedFilter = null;
        RemoveTags();
        CachedMethods.Clear();
        SetEditorStatus();
    }

    [Conditional("UNITY_EDITOR")]
    private static void SetEditorStatus()
    {
#if UNITY_EDITOR
        void LogPlayModeState(PlayModeStateChange state)
        {
            // UnityEngine.Debug.Log($"PlayModeStateChange {state}");
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                _logLineAllowedFilter = null;
                CachedMethods.Clear();
            }
        }

        if (!_isEditorHook)
        {
            _isEditorHook = true;
            EditorApplication.playModeStateChanged += LogPlayModeState;
        }
#endif
    }

    private static bool _isClassNamePrefix;
    private static string _prefixTag;
    private static string _suffixTag;
    private static string _contextTag = string.Empty;
    private static bool _isEditorHook;

    /// <summary>
    /// Filters log lines based on method name or other method properties.
    /// </summary>
    private static Func<MethodBase, bool> _logLineAllowedFilter;

    private static readonly Dictionary<MethodBase, bool> CachedMethods = new Dictionary<MethodBase, bool>();

    /// <summary>
    /// Adds log line filter.
    /// </summary>
    [Conditional("UNITY_EDITOR"), Conditional("FORCE_LOG")]
    public static void AddLogLineAllowedFilter(Func<MethodBase, bool> filter)
    {
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
        _isClassNamePrefix = true;
        _prefixTag = prefixTag;
        _suffixTag = suffixTag;
    }

    public static void SetContextTag(string contextTag)
    {
        _contextTag = $" {contextTag}";
    }

    private static void RemoveTags()
    {
        _isClassNamePrefix = false;
        _prefixTag = null;
        _suffixTag = null;
        _contextTag = string.Empty;
    }

    [Conditional("UNITY_EDITOR"), Conditional("FORCE_LOG")]
    public static void Log(string message, Object context = null, [CallerMemberName] string memberName = null)
    {
        var frame = new StackFrame(1);
        var method = frame.GetMethod();
        if (method == null || method.ReflectedType == null)
        {
            UnityEngine.Debug.Log(message, context);
        }
        else if (IsMethodAllowedForLog(method))
        {
            var prefix = GetPrefix(method, memberName);
            if (AppPlatform.IsEditor)
            {
                var contextTag = context != null ? _contextTag : string.Empty;
                UnityEngine.Debug.Log($"{Time.frameCount % 1000} {prefix}{message}{contextTag}", context);
            }
            else
            {
                UnityEngine.Debug.Log($"{Time.frameCount % 1000} {prefix}{message}");
            }
        }
    }

    [Conditional("UNITY_EDITOR"), Conditional("FORCE_LOG")]
    public static void LogFormat(string format, params object[] args)
    {
        var frame = new StackFrame(1);
        var method = frame.GetMethod();
        if (method == null || method.ReflectedType == null)
        {
            UnityEngine.Debug.LogFormat(format, args);
        }
        else if (IsMethodAllowedForLog(method))
        {
            UnityEngine.Debug.LogFormat($"{Time.frameCount % 1000} {GetPrefix(method)}{format}", args);
        }
    }

    [Conditional("UNITY_EDITOR"), Conditional("FORCE_LOG")]
    public static void LogWarning(string message, Object context = null)
    {
        UnityEngine.Debug.LogWarning(message, context);
    }

    public static void LogError(string message, Object context = null)
    {
        UnityEngine.Debug.LogError(message, context);
    }

    public static void LogException(Exception exception)
    {
        UnityEngine.Debug.LogException(exception);
    }

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
        if (memberName != null)
        {
            return _isClassNamePrefix
                ? $"{_prefixTag}{className}.{memberName}{_suffixTag} "
                : $"[{className}.{memberName}] ";
        }
        return _isClassNamePrefix
            ? $"{_prefixTag}{className}{_suffixTag} "
            : $"[{className}] ";
    }

    private static bool IsMethodAllowedForLog(MethodBase method)
    {
        if (_logLineAllowedFilter != null)
        {
            if (CachedMethods.TryGetValue(method, out var isMethodAllowed))
            {
                return isMethodAllowed;
            }
            var isAllowed = _logLineAllowedFilter(method);
            if (isAllowed)
            {
                CachedMethods.Add(method, true);
                // UnityEngine.Debug.Log($"[<color=brown>ACCEPT</color>] {method.Name} in {method.ReflectedType?.FullName}");
                return true;
            }
            // Nobody accepted so it is rejected.
            CachedMethods.Add(method, false);
            // UnityEngine.Debug.Log($"[<color=brown>REJECT</color>] {method.Name} in {method.ReflectedType?.FullName}");
            return false;
        }
        return true;
    }
}