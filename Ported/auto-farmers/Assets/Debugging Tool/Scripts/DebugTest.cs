using System;
using UnityEngine;

public class DebugTest : MonoBehaviour
{
    private void Awake()
    {
        Application.logMessageReceived += Log;

    }

    private void OnDestroy()
    {
        Application.logMessageReceived -= Log;
    }

    private void Log(string condition, string stacktrace, LogType type)
    {
        Debug.Log("[" + type.ToString() + "] " + condition + ": " + stacktrace);
    }
}
