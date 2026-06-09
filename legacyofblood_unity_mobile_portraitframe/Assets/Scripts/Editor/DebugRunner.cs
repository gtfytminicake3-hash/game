using UnityEditor;
public class DebugRunner
{
    [InitializeOnLoadMethod]
    public static void Run() { DebugReplayUI.Run(); }
}
