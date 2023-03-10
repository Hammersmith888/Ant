using UnityEngine;

public static class UnitWalk
{
    public static bool Go(MB_Unit mb_Unit, float posTgt, float speed)
    {
        bool done = false;

        float posCur = mb_Unit.transform.position.x;
        int dir = posTgt.CompareTo(posCur);
        float canPass = Time.deltaTime * speed;
        float dist = Mathf.Abs(posTgt - posCur);


        if (dist < canPass)
        {
            done = true;
            posCur = posTgt;
        }
        else
        {
            posCur += dir * canPass;
        }


        mb_Unit.transform.position = mb_Unit.transform.position.SetX(posCur);


        return done;
    }
}