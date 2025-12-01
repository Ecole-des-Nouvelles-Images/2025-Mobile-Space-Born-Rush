using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Pool;

public class DropsManager : MonoBehaviour
{

    private DropsData _dropsData;
    
    private PooledObject<GameObject> _drops;


    void SpawnDrop(Transform position, [Optional] DropType dropType)
    {
        //This method will spawn the drop at the position of the enemy killed
        //It should Always drop xp there
    }
}
