using UnityEngine;

public class blackHoleSpining : MonoBehaviour
{
    [SerializeField] private float spinSpeed= 180f;
    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.World);
    }
}
