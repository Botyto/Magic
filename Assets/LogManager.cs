using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LogManager : MonoBehaviour
{
    public const string Combat = "Combat";
    public const string Chat = "Chat";

    public const int maxMessages = 30;
    public static Dictionary<string, List<string>> logs = new Dictionary<string, List<string>>();
    public string logName;

    [SerializeField]
    [HideInInspector]
    private Text m_Text;

    [SerializeField]
    [HideInInspector]
    private List<string> m_Log;

    private void Start()
    {
        m_Text = GetComponent<Text>();
        if (!logs.TryGetValue(logName, out m_Log))
        {
            m_Log = new List<string>();
            logs[logName] = m_Log;
        }

        UpdateLogText();
    }
    
    public static void LogMessage(string logName, string message, params object[] args)
    {
        var allManagers = FindObjectsOfType<LogManager>();
        foreach (var manager in allManagers)
        {
            if (manager.logName == logName)
            {
                manager.LogMessage(message, args);
                break;
            }
        }
    }

    public static void LogMessage(string logName, string message)
    {
        var allManagers = FindObjectsOfType<LogManager>();
        foreach (var manager in allManagers)
        {
            if (manager.logName == logName)
            {
                manager.LogMessage(message);
                break;
            }
        }
    }

    public void LogMessage(string message, params object[] args)
    {
        LogMessage(logName, string.Format(message, args));
    }

    public void LogMessage(string message)
    {
        if (m_Log.Count > maxMessages)
        {
            m_Log.RemoveRange(maxMessages, m_Log.Count - maxMessages);
        }

        var t = TimeSpan.FromSeconds(Time.time);
        string timeFormatted = string.Format("{0:D2}:{1:D2}", t.Minutes, t.Seconds);
        var finalMsg = string.Format("[{0}] {1}", timeFormatted, message);
        m_Log.Add(finalMsg);
        UpdateLogText();
    }

    private void UpdateLogText()
    {
        string str = logName + " Log:\n";
        foreach (var message in m_Log)
        {
            str += message + "\n";
        }

        m_Text.text = str;
    }
}
