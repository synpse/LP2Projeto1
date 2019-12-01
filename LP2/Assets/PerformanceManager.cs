﻿using System;
using UnityEngine;
using UnityEngine.Profiling;

public class PerformanceManager : MonoBehaviour
{
    private float deltaTime = 0.0f;

    private void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
    }

    private void OnGUI()
    {
        int w = Screen.width, h = Screen.height;

        GUIStyle style = new GUIStyle();

        Rect rect1 = new Rect(10, 10, w, h * 2 / 100);
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = h * 2 / 100;
        style.normal.textColor = new Color(0.0f, 0.0f, 0.5f, 1.0f);
        float msec = deltaTime * 1000.0f;
        float fps = 1.0f / deltaTime;
        string text1 = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);

        GUI.Label(rect1, text1, style);

        Rect rect2 = new Rect(10, 25, w, h * 2 / 100);
        string text2 = $"Managed Heap: " +
            $"{Profiler.GetMonoHeapSizeLong() / 1024 / 1024} " +
            $"MB";

        GUI.Label(rect2, text2, style);

        Rect rect3 = new Rect(10, 40, w, h * 2 / 100);
        string text3 = $"Total Allocated by Unity: " +
            $"{Profiler.GetTotalReservedMemoryLong() / 1024 / 1024} " +
            $"MB";

        GUI.Label(rect3, text3, style);

    }
}
