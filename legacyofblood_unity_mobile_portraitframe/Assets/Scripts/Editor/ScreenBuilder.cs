
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.UI;

// This script creates a tool in the Unity Editor to automatically generate placeholder UI panels
// based on the screenbuilder.txt file.
public class ScreenBuilder : Editor
{
    // The script expects the plan file to be in the root of the Assets folder.
    private const string PLAN_FILE_PATH = "Assets/screenbuilder.txt";

    [MenuItem("Tools/Screen Builder/Build UI from Plan")]
    private static void BuildUIFromPlan()
    {
        if (!File.Exists(PLAN_FILE_PATH))
        {
            Debug.LogError($"Screen Builder Error: Plan file not found at '{PLAN_FILE_PATH}'. Please move screenbuilder.txt into the root Assets folder.");
            return;
        }

        Canvas canvas = FindOrCreateCanvas();
        string[] lines = File.ReadAllLines(PLAN_FILE_PATH);

        Debug.Log("--- Starting Screen Build Process ---");

        foreach (string line in lines)
        {
            if (line.StartsWith("## "))
            {
                // Found a new panel definition line, e.g., "## 1. Panel Phân Phối Điểm Chỉ Số (StatAllocationPanel)"
                string panelName = ParsePanelNameFromLine(line);
                if (!string.IsNullOrEmpty(panelName))
                {
                    Debug.Log($"Found panel definition: {panelName}. Creating placeholder...");
                    CreatePlaceholderPanel(panelName, canvas.transform);
                }
            }
        }

        Debug.Log("--- Screen Build Process Finished ---");
    }

    private static string ParsePanelNameFromLine(string line)
    {
        int startIndex = line.IndexOf('(');
        int endIndex = line.IndexOf(')');
        
        if (startIndex != -1 && endIndex != -1 && endIndex > startIndex)
        {
            // Extract the string inside the parentheses, e.g., "StatAllocationPanel"
            return line.Substring(startIndex + 1, endIndex - startIndex - 1);
        }
        
        return null;
    }

    private static void CreatePlaceholderPanel(string name, Transform parent)
    {
        // Check if an object with the same name already exists to avoid duplicates
        if (GameObject.Find(name))
        {
            Debug.LogWarning($"A GameObject named '{name}' already exists in the scene. Skipping.");
            return;
        }

        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);

        // Add RectTransform, which is essential for all UI elements
        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(600, 400); // A default size for the panel

        // Add a placeholder background image to make it visible
        Image img = panel.AddComponent<Image>();
        img.color = new Color(0.1f, 0.1f, 0.1f, 0.85f); // Dark semi-transparent background

        // Add a simple title text
        CreateTitleText(name, panel.transform);
    }

    private static void CreateTitleText(string title, Transform parent)
    {
        GameObject textGO = new GameObject("TitleText");
        textGO.transform.SetParent(parent, false);

        Text txt = textGO.AddComponent<Text>();
        txt.text = title;
        txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        txt.fontSize = 24;
        txt.alignment = TextAnchor.MiddleCenter;

        RectTransform rect = textGO.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(0.5f, 1);
        rect.anchoredPosition = new Vector2(0, -20);
        rect.sizeDelta = new Vector2(0, 30);
    }

    private static Canvas FindOrCreateCanvas()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.Log("No Canvas found in the scene. Creating a new one.");
            GameObject canvasGO = new GameObject("Canvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
        }
        return canvas;
    }
}
