using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class ControllerWeapon : MonoBehaviour
{
    [SerializeField] private ControllerBody bodyController;
    public Animation anim;
    void Start()
    {
        anim = GetComponent<Animation>();
    }

    void Update()
    {
        transform.parent.LookAt(bodyController.target);
    }
}
