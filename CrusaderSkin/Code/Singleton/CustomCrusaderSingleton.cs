using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using System.Collections.Generic;

namespace CrusaderSkin.Code.Singleton;

/// <summary>
/// Model that will passively receive hooks at all times.
/// </summary>
public abstract class CustomCrusaderSingletonModel : AbstractModel//SingletonModel
{
    public enum HookType
    {
        None,
        Combat,
        Run
    }

    /// <summary>
    /// This property seems effectively unused; it is set anyways in case of future changes.
    /// </summary>
    public override bool ShouldReceiveCombatHooks { get; }


    public CustomCrusaderSingletonModel(HookType hookType)
    {
        switch (hookType)
        {
            case HookType.None:
                break;
            case HookType.Combat:
                ShouldReceiveCombatHooks = true;
                ModHelper.SubscribeForCombatStateHooks(Id.Entry, CombatSubModels);
                break;
            case HookType.Run:
                ModHelper.SubscribeForRunStateHooks(Id.Entry, RunSubModels);
                break;
        }
    }

    protected IRunState? CurrentRunState { get; private set; }
    protected CombatState? CurrentCombatState { get; private set; }

    private IEnumerable<AbstractModel> RunSubModels(RunState runState)
    {
        CurrentRunState = runState;
        CurrentCombatState = null;
        return [this];
    }
    private IEnumerable<AbstractModel> CombatSubModels(CombatState combatState)
    {
        CurrentCombatState = combatState;
        CurrentRunState = combatState.RunState;
        return [this];
    }
}