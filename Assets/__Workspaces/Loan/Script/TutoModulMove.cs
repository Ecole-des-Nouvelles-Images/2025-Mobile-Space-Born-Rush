using UnityEngine;

public class TutoModulMove : MonoBehaviour
{
    private float _width = 25f;
    public TutoManager TutoManager;
    private void Update()
    {
        var vector3 = transform.position;
        vector3.z = vector3.z - 1 * Time.deltaTime;
        transform.position = vector3;
        if (transform.position.z < -_width)
        {
            GameObject modul =Instantiate(TutoManager.CurrentModulSpawn, new Vector3(0,0,53), Quaternion.identity,TutoManager._parentModulTransform.transform);
            modul.GetComponent<TutoModulMove>().TutoManager = TutoManager;
            Destroy(gameObject);
        }
    }
}