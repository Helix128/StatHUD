using UnityEngine;

public class TimeModule
{
    public static float time;

    public static bool isPaused = false;
    public static void Update()
    {
        isPaused = !(UiManager.Instance.pause.current == null) && UiManager.Instance.pause.current.activeInHierarchy;
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