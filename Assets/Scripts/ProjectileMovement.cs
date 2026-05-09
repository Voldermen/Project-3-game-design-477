using UnityEngine;
using System.Collections;

public class ProjectileMovement : MonoBehaviour
{
    public IEnumerator MoveTo(Vector3 startPosition, Vector3 endPosition, float speed)
    {
        transform.position= startPosition;

        while(Vector3.Distance(transform.position, endPosition)> 0.05f)
        {
            transform.position= Vector3.MoveTowards(transform.position, endPosition, speed*Time.deltaTime);
            yield return null;
            
        }
        transform.position= endPosition;
    }
} // this makes the projectile move from the friendly unit to the enemy unit.
