using UnityEngine;

public class UpdateBattle : MonoBehaviour
{
    public void Update()
    {
        TimeManager.ManaIncrementation();
        TimeManager.CooldownDurationDecrementation();
    }
}
