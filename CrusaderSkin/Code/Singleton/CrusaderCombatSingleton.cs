using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Logging;
using CrusaderSkin.Code.Config;
using CrusaderSkin.Code.Helper;
using System.Threading.Tasks;
using CrusaderSkin.Code.ModConfig;

namespace CrusaderSkin.Code.Singleton;

public class CrusaderCombatSingleton : CustomCrusaderSingletonModel
{
    public CrusaderCombatSingleton() : base(HookType.Combat)
    {

    }
    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        //Card.Owner在111版本可以换成Card.Player吗？
        if (cardPlay != null && cardPlay.Card.Owner != null && CrusaderHelper.IsIronclad(cardPlay.Card.Owner))
        {
            string animName = CrusaderConfigLoader.GetValueAsString(cardPlay.Card.Id.Entry);
            if (!string.IsNullOrEmpty(animName))
            {
                return CreatureCmd.TriggerAnim(cardPlay.Card.Owner.Creature, "CardPlay/" + animName, 0f);
            }
        }
        return Task.CompletedTask;
    }

    public override Task AfterCurrentHpChanged(Creature creature, decimal delta)
    {
        if (CrusaderHelper.IsIronclad(creature))
        {
            if (delta > 0)
            {
                if (!CrusaderHelper.IsLowHealth(creature, 25m, 0m) && CrusaderHelper.IsLowHealth(creature, 25m, delta) && CrusaderHelper.IsInAnyIdle(null, creature)
                    && CrusaderSettings.UseLowHealthIdle)
                {
                    return CreatureCmd.TriggerAnim(creature, "Revive", 0.0f);
                }
                //复活之后仍处于低血量。发现被致命一击的那次'Hit'没有触发角色的受击动画，而且复活时没有Trigger角色Idle但刷新了Idle动画，不知咋回事 反正手动触发一下
                if (creature.CurrentHp - delta <= 0m && CrusaderHelper.IsLowHealth(creature, 25m, 0m)
                    && CrusaderSettings.UseLowHealthIdle)
                {
                    return CreatureCmd.TriggerAnim(creature, "DeathDoor", 0.0f);
                }
            }
            else if (delta < 0)
            {
                if (CrusaderHelper.IsLowHealth(creature, 25m, 0m) && !CrusaderHelper.IsLowHealth(creature, 25m, delta) && CrusaderHelper.IsInAnyIdle(null, creature)
                    && CrusaderSettings.UseLowHealthIdle)
                {
                    return CreatureCmd.TriggerAnim(creature, "DeathDoor", 0.0f);
                }
            }
        }
        return Task.CompletedTask;
    }
}