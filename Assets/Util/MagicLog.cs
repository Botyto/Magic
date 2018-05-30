//Possible defines:
//LOG_TO_UNITY
//LOG_TO_INGAME_CONSOLE
//ALWAYS_LOG_EVERYWHERE

#if DEBUG
#define LOG_TO_UNITY
#else
#define LOG_TO_INGAME_CONSOLE
#endif

using System;
using UnityEngine;

public static class MagicLog
{
    #region Helpers

    private enum LogType
    {
        Message,
        Warning,
        Error,
        Assert,
        Exception,
    }

    /// <summary>
    /// Prints the message in the unity console (forwards to Debug.* calls)
    /// </summary>
    private static void LogToUnity(object message, LogType type)
    {
        switch (type)
        {
            case LogType.Message:
                Debug.Log(message);
                break;

            case LogType.Warning:
                Debug.LogWarning(message);
                break;

            case LogType.Error:
                Debug.LogError(message);
                break;

            case LogType.Assert:
                Debug.LogAssertion(message);
                break;

            case LogType.Exception:
                Debug.LogException(message as Exception);
                break;
        }
    }

    /// <summary>
    /// Prints the message in the ingame console
    /// </summary>
    /// <param name="message"></param>
    /// <param name="type"></param>
    private static void LogToIngameConsole(object message, LogType type)
    {
        var console = UnityEngine.Object.FindObjectOfType<ScriptConsole>();
        if (console != null)
        {
            var str = message.ToString();

            switch (type)
            {
                case LogType.Warning:
                    str = "<color=orange>" + str + "</color>";
                    break;

                case LogType.Error:
                case LogType.Assert:
                case LogType.Exception:
                    str = "<color=red>" + str + "</color>";
                    break;
            }

            console.Log(message.ToString());
        }
    }

    /// <summary>
    /// Prints the message in every available logging device
    /// </summary>
    private static void LogEverywhere(object message, LogType type)
    {
        LogToUnity(message, type);
        LogToIngameConsole(message, type);
    }

    /// <summary>
    /// Prints the message in all enabled device (see defines above)
    /// </summary>
    private static void LogOnEnabledDevices(object message, LogType type)
    {
#if ALWAYS_LOG_EVERYWHERE
        LogEverywhere(message, type);
#else
#if LOG_TO_UNITY
        LogToUnity(message, type);
#endif
#if LOG_TO_INGAME_CONSOLE
        LogToIngameConsole(message, type);
#endif
#endif
    }

    #endregion

    #region Public Logging Interface

    public static void Log(string message)
    {
        LogOnEnabledDevices(message, LogType.Message);
    }

    public static void LogFormat(string format, params object[] args)
    {
        LogOnEnabledDevices(string.Format(format, args), LogType.Message);
    }

    public static void LogError(string message)
    {
        LogOnEnabledDevices(message, LogType.Error);
    }

    public static void LogErrorFormat(string format, params object[] args)
    {
        LogOnEnabledDevices(string.Format(format, args), LogType.Error);
    }

    public static void LogWarning(string message)
    {
        LogOnEnabledDevices(message, LogType.Warning);
    }

    public static void LogWarningFormat(string format, params object[] args)
    {
        LogOnEnabledDevices(string.Format(format, args), LogType.Warning);
    }

    public static void LogAssertion(string message)
    {
        LogOnEnabledDevices(message, LogType.Assert);
    }

    public static void LogAssertionFormat(string format, params object[] args)
    {
        LogOnEnabledDevices(string.Format(format, args), LogType.Assert);
    }

    public static void LogException(object exception)
    {
        LogOnEnabledDevices(exception, LogType.Exception);
    }

    #endregion
}
