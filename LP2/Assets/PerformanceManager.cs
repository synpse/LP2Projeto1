using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Profiling;

public class PerformanceManager : MonoBehaviour
{
    private float deltaTime = 0.0f;

    private void Update()
    {
        CalculateDelta();
    }

    private void CalculateDelta()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
    }

    private void OnGUI()
    {
        int w = Screen.width, h = Screen.height;

        GUIStyle style = new GUIStyle();
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = h * 2 / 100;
        style.normal.textColor = new Color(0.0f, 0.0f, 0.5f, 1.0f);

        // Label1
        Rect rect1 = new Rect(10, 10, w, h * 2 / 100);

        float msec = deltaTime * 1000.0f;
        float fps = 1.0f / deltaTime;
        string text1 = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);

        GUI.Label(rect1, text1, style);

        // Label2
        Rect rect2 = new Rect(10, 30, w, h * 2 / 100);

        string text2 = $"Managed Heap: " +
            $"{Profiler.GetMonoHeapSizeLong() / 1024 / 1024} " +
            $"MB";

        GUI.Label(rect2, text2, style);

        // Label3
        // Label4
    }
}
