using UnityEngine;

public class ControllerHairLightweight : MonoBehaviour
{
    [SerializeField]private ControllerBody bodyController;
    public  Animation anim;
    void Start()
    {
        anim = GetComponent<Animation>();
    }

    void Update()
    {
        transform.rotation = bodyController.transform.rotation;
    }
}
