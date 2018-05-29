using MoonSharp.Interpreter;

public static class MagicScriptLibrary
{
    #region Bind

    public static void Bind(Script L)
    {
        ScriptLibrary.BindClass<Unit>(L);
        ScriptLibrary.BindClass<Selectable>(L);
        ScriptLibrary.BindClass<PlayerMovement>(L);

        var tEnergyHolder = ScriptLibrary.BindClass<EnergyHolder>(L);
        tEnergyHolder["ResolveOwner"] = new CallbackFunction(EnergyHolderResolveOwner);
        ScriptLibrary.BindClass<EnergyController>(L);
        ScriptLibrary.BindEnum<EnergyActionResult>(L.Globals);
        ScriptLibrary.BindClass<EnergyManifestation>(L);
        ScriptLibrary.BindClass<Wizard>(L);
        
        var tEnergy = new Table(L);
        L.Globals["Energy"] = tEnergy;
        tEnergy["scale"] = Energy.scale;
        tEnergy["scalef"] = Energy.scalef;
        tEnergy["minTemperature"] = Energy.minTemperature;
        tEnergy["speedLimit"] = Energy.speedLimit;
        tEnergy["sqrSpeedLimit"] = Energy.sqrSpeedLimit;
        ScriptLibrary.BindEnum<Energy.Element>(tEnergy);
        ScriptLibrary.BindEnum<Energy.Shape>(tEnergy);

        ScriptLibrary.BindClass<InstantSpellComponent>(L);
        ScriptLibrary.BindClass<ContinuousSpellComponent>(L);
        ScriptLibrary.BindClass<StagedSpellComponent>(L);
        ScriptLibrary.BindClass<ToggleSpellComponent>(L);
    }

    #endregion

    #region Magic

    public static DynValue EnergyHolderResolveOwner(ScriptExecutionContext ctx, CallbackArguments args)
    {
        var target = args.AsUserData<EnergyHolder>(0, "EnergyHolder.ResolveOwner", false);
        return DynValue.FromObject(ctx.OwnerScript, EnergyHolder.ResolveOwner(target));
    }

    #endregion
}
