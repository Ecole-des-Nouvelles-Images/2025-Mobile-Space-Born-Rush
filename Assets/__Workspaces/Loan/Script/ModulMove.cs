using UnityEngine;

public class ModulMove : MonoBehaviour
{
    private Vector3 _speedMovement = new Vector3(0f, 0f, -5f);
    
    void Update()
    {
        transform.position += _speedMovement * Time.deltaTime;
    }
}
