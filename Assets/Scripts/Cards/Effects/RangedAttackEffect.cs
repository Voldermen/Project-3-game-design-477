using UnityEngine;
using System.Collections;
[CreateAssetMenu(menuName= "Strategy Game/Card Effects/Ranged Attack")]

public class RangedAttackEffect : CardEffect
{
    public int Damage =6;
    public float ProjectileSpeed=6f;

    [SerializeField] private ProjectileMovement projectilePrefab;

    public override bool Resolve(CardEffectContext context)
    {
        BoardUnitState actingUnit= context.ActingUnit;
        BoardUnitState targetUnit= context.TargetUnit; // gets the enemy unit.

        if (context.GameManager == null)
        {
            return false;
        }

        if (actingUnit==null || targetUnit == null)
        {
            return false;
        }
        context.GameManager.StartCoroutine(ProjectileThenDamage(context, actingUnit,targetUnit));// this shows the projectile moving.
        return true;
    }

    private IEnumerator ProjectileThenDamage(CardEffectContext context, BoardUnitState actingUnit, BoardUnitState targetUnit)
    {
        Vector3 startPosition=context.GameManager.WorldPosition(actingUnit.Position);
        Vector3 endPosition= context.GameManager.WorldPosition(targetUnit.Position);

        ProjectileMovement projectile= Instantiate(projectilePrefab, startPosition, Quaternion.identity);

        yield return projectile.MoveTo(startPosition,endPosition, ProjectileSpeed);
        Destroy(projectile.gameObject);

        if (!context.BoardState.UnitsById.ContainsKey(targetUnit.UnitId))
        {
            yield break;
        }

        context.DoDamage(targetUnit, Damage);
    }
}
