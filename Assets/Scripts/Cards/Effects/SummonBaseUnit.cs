using UnityEngine;

[CreateAssetMenu(menuName= "Strategy Game/Card Effects/Summon Base Unit")]
public class SummonBaseUnit : CardEffect
{
   [SerializeField] private UnitDefinition unitToSpawn;

   public override bool Resolve(CardEffectContext context)
    {
        if (context.GameManager== null || context.BoardState == null)
        {
            return false;
        }
        

        if (unitToSpawn == null)
        {
            return false;
        }

        Vector2Int spawnPosition= context.TargetPosition;

        if(!context.BoardState.IsInsideBoard(spawnPosition.x, spawnPosition.y)) // checks to make sure the unit is being spawned in on the board.
        {
            return false;
        }

        if (context.BoardState.GetUnitAtTile(spawnPosition.x, spawnPosition.y) != null) // prevents the unit from being summoned on top of another unit.
        {
            return false;
        }

        
        BoardUnitState newUnit= context.GameManager.CreateUnit(unitToSpawn, spawnPosition);// 1
        context.GameManager.AddUnitToWorkingState(newUnit); //2 , both these lines spawn the unit in on the board.
        return true;
    }
}
