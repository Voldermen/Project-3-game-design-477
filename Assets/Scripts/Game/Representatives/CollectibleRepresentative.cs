using UnityEngine;

public class CollectibleRepresentative : MonoBehaviour
{
    [SerializeField] private float spinSpeed= 180f;
    [SerializeField] private Vector3 spinAxis= Vector3.up;

    private void Update()
    {
        transform.Rotate(spinAxis, spinSpeed * Time.deltaTime, Space.World);
    }
   public void Render(BoardCollectibleState collectible)
    {
        // animation/ color and other values can be added here for collectibles.
    }
}
