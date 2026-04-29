using UnityEngine;

public class BoardUnitRepresentative : MonoBehaviour
{
    public int UnitId { get; private set; }

    public void Render(BoardUnitState unitState)
    {
        UnitId = unitState.UnitId;
    }
}