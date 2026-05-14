using UnityEngine;
using HighScore;
public class InitalizeGame : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        HS.Init(this, "Worlds Apart");
    }

}
