using UnityEngine;
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

        BindSpellComponent(L);
    }

    private static void BindSpellComponent(Script L)
    {
        var tSpellBase = new Table(L);
        L.Globals["__SpellBase"] = tSpellBase;
        tSpellBase["GetWizard"] = new CallbackFunction(SpellBase_GetWizard);
        tSpellBase["GetController"] = new CallbackFunction(SpellBase_GetController);
        tSpellBase["GetUnit"] = new CallbackFunction(SpellBase_GetUnit);
        tSpellBase["GetParam"] = new CallbackFunction(SpellBase_GetParam);
        tSpellBase["GetMaxFocus"] = new CallbackFunction(SpellBase_GetMaxFocus);
        tSpellBase["SetTarget"] = new CallbackFunction(SpellBase_SetTarget);
        tSpellBase["CanFocusMore"] = new CallbackFunction(SpellBase_CanFocusMore);
        tSpellBase["IsFocusValid"] = new CallbackFunction(SpellBase_IsFocusValid);
        tSpellBase["DosposeAllFocused"] = new CallbackFunction(SpellBase_DisposeAllFocused);
        tSpellBase["DisownAllFocused"] = new CallbackFunction(SpellBase_DisownAllFocused);
        tSpellBase["Cancel"] = new CallbackFunction(SpellBase_Cancel);
        tSpellBase["Finish"] = new CallbackFunction(SpellBase_Finish);

        var tSpell = new Table(L);
        L.Globals["__Spell"] = tSpell;
        tSpell["GetTargetPosition"] = new CallbackFunction(Spell_GetTargetPosition);
        tSpell["GetCursorPosition"] = new CallbackFunction(Spell_GetCursorPosition);
        tSpell["ManifestEnergyAndFocus"] = new CallbackFunction(Spell_ManifestEnergyAndFocus);
        tSpell["GetFocusPosition"] = new CallbackFunction(Spell_GetFocusPosition);
        tSpell["GetFocusVelocity"] = new CallbackFunction(Spell_GetFocusVelocity);
        tSpell["DisownFocus"] = new CallbackFunction(Spell_DisownFocus);
    }

    #endregion

    #region Magic General

    public static DynValue EnergyHolderResolveOwner(ScriptExecutionContext ctx, CallbackArguments args)
    {
        var target = args.AsUserData<EnergyHolder>(0, "EnergyHolder.ResolveOwner", false);
        return DynValue.FromObject(ctx.OwnerScript, EnergyHolder.ResolveOwner(target));
    }

    private static T GetSpell<T>(CallbackArguments args, int argIdx = 0)
    {
        var scriptComponent = args.RawGet(argIdx, true);
        if (scriptComponent.Type != DataType.Table)
        {
            throw ScriptRuntimeException.BadArgument(argIdx, "SpellComponentBase.Getspell", DataType.Table, scriptComponent.Type, false);
            //return default(T);
        }

        var trueValue = scriptComponent.Table.RawGet(true);
        if (trueValue.Type != DataType.UserData)
        {
            throw ScriptRuntimeException.BadArgument(argIdx, "SpellComponentBase.Getspell", "`spell[true]` is not a UserData type");
            //return default(T);
        }

        var unityComponent = trueValue.CheckUserDataType<T>("GetSpellComponent", 0);
        return unityComponent;
    }

    #endregion

    #region Spell Component Base

    public static DynValue SpellBase_GetWizard(ScriptExecutionContext ctx, CallbackArguments args)
    {
        return DynValue.FromObject(ctx.OwnerScript, GetSpell<SpellComponentBase>(args).wizard);
    }

    public static DynValue SpellBase_GetController(ScriptExecutionContext ctx, CallbackArguments args)
    {
        return DynValue.FromObject(ctx.OwnerScript, GetSpell<SpellComponentBase>(args).controller);
    }

    public static DynValue SpellBase_GetUnit(ScriptExecutionContext ctx, CallbackArguments args)
    {
        return DynValue.FromObject(ctx.OwnerScript, GetSpell<SpellComponentBase>(args).unit);
    }

    public static DynValue SpellBase_GetTarget(ScriptExecutionContext ctx, CallbackArguments args)
    {
        return DynValue.FromObject(ctx.OwnerScript, GetSpell<SpellComponentBase>(args).target);
    }

    public static DynValue SpellBase_GetParam(ScriptExecutionContext ctx, CallbackArguments args)
    {
        var result = DynValue.NewTable(ctx.OwnerScript);
        var table = result.Table;
        var param = GetSpell<SpellComponentBase>(args).param;
        table["level"] = param.level;
        table["element"] = param.element;
        table["shape"] = param.shape;
        table["obj"] = param.obj;
        table["statusEffect"] = param.statusEffect;
        table["integer"] = param.integer;
        table["real"] = param.real;
        return result;
    }

    public static DynValue SpellBase_GetMaxFocus(ScriptExecutionContext ctx, CallbackArguments args)
    {
        return DynValue.FromObject(ctx.OwnerScript, GetSpell<SpellComponentBase>(args).maxFocus);
    }

    public static DynValue SpellBase_SetTarget(ScriptExecutionContext ctx, CallbackArguments args)
    {
        var newTarget = args.AsUserData<GameObject>(0, "SpellComponent.SetTarget", true);
        GetSpell<SpellComponentBase>(args).SetTarget(newTarget);
        return DynValue.Nil;
    }

    public static DynValue SpellBase_CanFocusMore(ScriptExecutionContext ctx, CallbackArguments args)
    {
        return DynValue.FromObject(ctx.OwnerScript, GetSpell<SpellComponentBase>(args).CanFocusMore());
    }

    public static DynValue SpellBase_IsFocusValid(ScriptExecutionContext ctx, CallbackArguments args)
    {
        var spell = GetSpell<SpellComponentBase>(args);
        var handle = args.AsInt(1, "SpellComponent.IsFocusValid");
        return DynValue.NewBoolean(spell.IsFocusValid(handle));
    }

    public static DynValue SpellBase_DisposeAllFocused(ScriptExecutionContext ctx, CallbackArguments args)
    {
        var spell = GetSpell<SpellComponentBase>(args);
        spell.DisposeAllFocused();
        return DynValue.Nil;
    }

    public static DynValue SpellBase_DisownAllFocused(ScriptExecutionContext ctx, CallbackArguments args)
    {
        var spell = GetSpell<SpellComponentBase>(args);
        spell.DisownAllFocused();
        return DynValue.Nil;
    }

    public static DynValue SpellBase_Cancel(ScriptExecutionContext ctx, CallbackArguments args)
    {
        var spell = GetSpell<SpellComponentBase>(args);
        var disposeFocused = args.RawGet(1, true).Boolean;
        spell.Cancel(disposeFocused);
        return DynValue.Nil;
    }

    public static DynValue SpellBase_Finish(ScriptExecutionContext ctx, CallbackArguments args)
    {
        var spell = GetSpell<SpellComponentBase>(args);
        spell.Finish();
        return DynValue.Nil;
    }

    #endregion

    #region Spell Component

    public static DynValue Spell_GetTargetPosition(ScriptExecutionContext ctx, CallbackArguments args)
    {
        return DynValue.FromObject(ctx.OwnerScript, GetSpell<SpellComponent>(args).TargetPosition);
    }

    public static DynValue Spell_GetCursorPosition(ScriptExecutionContext ctx, CallbackArguments args)
    {
        return DynValue.FromObject(ctx.OwnerScript, GetSpell<SpellComponent>(args).CursorPosition);
    }

    public static DynValue Spell_ManifestEnergyAndFocus(ScriptExecutionContext ctx, CallbackArguments args)
    {
        var spell = GetSpell<SpellComponent>(args);
        var amount = args.AsInt(1, "SpellComponent.ManifestEnergyAndFocus");
        var relativePos = args.AsUserData<Vector3>(2, "SpellComponent.ManifestEnergyAndFocus", false);

        var status = EnergyActionResult.UndefinedError;
        var handle = -1;

        if (args.Count == 3)
        {
            status = spell.ManifestEnergyAndFocus(amount, relativePos, out handle);
        }
        else
        {

            var element = (Energy.Element)args.AsInt(3, "SpellComponent.ManifestEnergyAndFocus");
            var shape = (Energy.Shape)args.AsInt(4, "SpellComponent.ManifestEnergyAndFocus");
            status = spell.ManifestEnergyAndFocus(amount, relativePos, element, shape, out handle);
        }

        return DynValue.NewTuple(DynValue.NewNumber((int)status), DynValue.NewNumber(handle));
    }
    
    public static DynValue Spell_GetFocusPosition(ScriptExecutionContext ctx, CallbackArguments args)
    {
        var spell = GetSpell<SpellComponent>(args);
        var handle = args.AsInt(1, "SpellComponent.GetFocusPosition");
        return DynValue.FromObject(ctx.OwnerScript, spell.GetFocusPosition(handle));
    }

    public static DynValue Spell_GetFocusVelocity(ScriptExecutionContext ctx, CallbackArguments args)
    {
        var spell = GetSpell<SpellComponent>(args);
        var handle = args.AsInt(1, "SpellComponent.GetFocusVelocity");
        return DynValue.FromObject(ctx.OwnerScript, spell.GetFocusVelocity(handle));
    }

    public static DynValue Spell_DisownFocus(ScriptExecutionContext ctx, CallbackArguments args)
    {
        var spell = GetSpell<SpellComponent>(args);
        var handle = args.AsInt(1, "SpellComponent.DisownFocus");
        return DynValue.NewBoolean(spell.DisownFocus(handle));
    }

    #endregion
}
