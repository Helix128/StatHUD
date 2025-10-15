using UnityEngine;

public class TimeModule
{
    public static float time;

    public static void Update()
    {
        bool pause = !(UiManager.Instance.pause.current == null) && UiManager.Instance.pause.current.activeInHierarchy;
        if (!pause)
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