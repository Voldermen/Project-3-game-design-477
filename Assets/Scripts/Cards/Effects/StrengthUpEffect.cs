using UnityEngine;

[CreateAssetMenu(menuName= "Strategy Game/Card Effects/Strength Up")]
public class StrengthUpEffect : CardEffect
{
   public int IncreaseStrength=3;
   public int TurnDuration=3;

   public override bool Resolve(CardEffectContext context)
    {
        BoardUnitState targetUnit= context.TargetUnit;

        if(targetUnit== null)
        {
            return false;
        }

        if(targetUnit.Team != UnitTeam.Friendly)
        {
            return false;
        }

        targetUnit.strengthUp += IncreaseStrength;
        targetUnit.strengthTurnsRemaining=Mathf.Max(targetUnit.strengthTurnsRemaining,TurnDuration);

        if (context.GameManager != null)
        {
            context.GameManager.RefreshBoard(context.BoardState);
        }
        return true;
    }
}
