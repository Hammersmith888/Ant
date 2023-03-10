using UnityEngine;

public static class GameTools
{
    public static void Set_LookDir(Transform transform, bool left)
    {
        transform.localScale = transform.localScale.SetX(Mathf.Abs(transform.localScale.x) * (left ? -1 : +1));
    }

    public static void Set_LookDir(Transform transform, float dir)
    {
        Set_LookDir(transform, dir < 0);
    }

    public static void Set_Z(Transform transform, float addon = 0)
    {
        var pos = transform.position;
        pos.z = (pos.y + addon) / 100;
        transform.position = pos;
    }

    public static AudioClip GetRandom(AudioClip[] clips)
    {
        return clips[Random.Range(0, clips.Length)];
    }

    public static void DoubleSpeed(Animation animation)
    {
        foreach (AnimationState animationState in animation)
            animationState.speed = 2;
    }

    public static void SetAwakeTimer(Timer timer, float sleepMin, bool randomForward)
    {
        var forward01 = randomForward ? Random.Range(0, .75f) : 0;

        const float dayInSeconds = 60 * 60 * 24;
        var sleepInSeconds = sleepMin * 60;
        var awake = dayInSeconds - sleepInSeconds;
        var forward = awake * forward01;

        timer.Set(awake);
        timer.ForwardTime(forward);
    }
}