using UnityEngine;
using System.Collections;

public class RespawnManager : SingletonBase<RespawnManager>
{
    public void RespawnAfterDelay(AbsorbableObject obj, float delay)
    {
        StartCoroutine(RespawnCoroutine(obj, delay));
    }

    private IEnumerator RespawnCoroutine(AbsorbableObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        obj.Respawn();
    }
}
