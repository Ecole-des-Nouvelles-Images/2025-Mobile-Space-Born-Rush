using UnityEngine;

public class GenerateGrid : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

struct Cell
{
    public int IndexX;
    public int IndexZ;
    public Vector3 Position;
    public GameObject ParentObject;
    public bool IsEmpty;
}
