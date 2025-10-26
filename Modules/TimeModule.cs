using UnityEngine;

public class TimeModule
{
    public static float time;

    public static bool isPaused = false;
    public static void Update()
    {
        if (UiManager.Instance == null)
        {
            isPaused = true;
        }
        else if (UiManager.Instance.encounterWindows == null || UiManager.Instance.pause == null)
        {
            isPaused = false;
        }
        else
        {
            isPaused = !(UiManager.Instance.pause.current == null) && UiManager.Instance.pause.current.activeInHierarchy || UiManager.Instance.encounterWindows.activeEncounterWindow != null;
        }
        if (!isPaused)
        {
            time += Time.deltaTime;
        }
    }

    public static float GetTime()
    {
        return time;
    }
    
    public static void Reset()
    {
        time = 0;
    }
    
}