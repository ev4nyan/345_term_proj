using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameLog : MonoBehaviour
{
    public static GameLog Instance { get; private set; }

    [SerializeField] private TMP_Text logText;
    [SerializeField] private int maxLines = 8;

    private readonly Queue<string> lines = new Queue<string>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        // If you want it to persist across scenes:
        // DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        // Mirror Debug.Log into this UI log (optional but handy)
        Application.logMessageReceived += HandleLogMessage;
    }

    private void OnDisable()
    {
        Application.logMessageReceived -= HandleLogMessage;
    }

    private void HandleLogMessage(string condition, string stackTrace, LogType type)
    {
        // Only show normal logs as tips; skip warnings/errors if you want
        if (type == LogType.Log)
        {
            AddLine(condition);
        }
    }

    public void AddLine(string message)
    {
        if (string.IsNullOrEmpty(message))
            return;

        lines.Enqueue(message);

        while (lines.Count > maxLines)
            lines.Dequeue();

        logText.text = string.Join("\n", lines);
    }
}
