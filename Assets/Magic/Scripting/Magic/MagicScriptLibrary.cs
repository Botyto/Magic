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

        L.Globals["TryStrict"] = new CallbackFunction(GlobalTryStrict);
        L.Globals["Try"] = new CallbackFunction(GlobalTry);

        var tSpellUtilities = new Table(L);
        L.Globals["SpellUtilities"] = tSpellUtilities;
        tSpellUtilities["FindClosestEnemy"] = new CallbackFunction(SpellUtilities_FindClosestEnemy);
        tSpellUtilities["FindClosestFriend"] = new CallbackFunction(SpellUtilities_FindClosestFriend);

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
        ScriptLibrary.BindEnum<StatusEffect.Type>(L.Globals, "StatusEffectType");

        var tSpell = new Table(L);
        L.Globals["__Spell"] = tSpell;
        tSpell["GetTargetPosition"] = new CallbackFunction(Spell_GetTargetPosition);
        tSpell["GetCursorPosition"] = new CallbackFunction(Spell_GetCursorPosition);
        tSpell["ManifestEnergyAndFocus"] = new CallbackFunction(Spell_ManifestEnergyAndFocus);
        tSpell["GetFocusPosition"] = new CallbackFunction(Spell_GetFocusPosition);
        tSpell["GetFocusVelocity"] = new CallbackFunction(Spell_GetFocusVelocity);
        tSpell["DisownFocus"] = new CallbackFunction(Spell_DisownFocus);
        tSpell["Summon"] = new CallbackFunction(Spell_Summon);
        tSpell["Charge"] = new CallbackFunction(Spell_Charge);
        tSpell["Discharge"] = new CallbackFunction(Spell_Discharge);
        tSpell["Merge"] = new CallbackFunction(Spell_Merge);
        tSpell["Separate"] = new CallbackFunction(Spell_Separate);
        tSpell["ChangeElement"] = new CallbackFunction(Spell_ChangeElement);
        tSpell["ChangeShape"] = new CallbackFunction(Spell_ChangeShape);
        tSpell["Deform"] = new CallbackFunction(Spell_Deform);
        tSpell["CreateElasticConnection"] = new CallbackFunction(Spell_CreateElasticConnection);
        tSpell["ApplyForceRelative"] = new CallbackFunction(Spell_ApplyForceRelative);
        tSpell["ApplyForce"] = new CallbackFunction(Spell_ApplyForce);
        tSpell["ApplyTorque"] = new CallbackFunction(Spell_ApplyTorque);
        tSpell["OrientTowards"] = new CallbackFunction(Spell_OrientTowards);
        //tSpell["Summon"] = new CallbackFunction(Spell_ApplyAura);
        tSpell["Substitute"] = new CallbackFunction(Spell_Substitute);
        tSpell["CounterFalling"] = new CallbackFunction(Spell_CounterFalling);
        tSpell["CounterGravity"] = new CallbackFunction(Spell_CounterGravity);
        tSpell["CounterMotion"] = new CallbackFunction(Spell_CounterMotion);

        var tInstantSpell = new Table(L);
        L.Globals["__InstantSpell"] = tInstantSpell;

        var tContinuousSpell = new Table(L);
        L.Globals["__ContinuousSpell"] = tContinuousSpell;

        var tToggleSpell = new Table(L);
        L.Globals["__ToggleSpell"] = tToggleSpell;
        tToggleSpell["GetInterval"] = new CallbackFunction(ToggleSpell_GetInterval);
        tToggleSpell["SetInterval"] = new CallbackFunction(ToggleSpell_SetInterval);

        var tStagedSpell = new Table(L);
        L.Globals["__StagedSpell"] = tStagedSpell;
        tStagedSpell["GetCurrentStage"] = new CallbackFunction(StagedSpell_GetCurrentStage);
        tStagedSpell["NextStage"] = new CallbackFunction(StagedSpell_NextStage);
        ScriptLibrary.BindEnum<StagedSpellComponent.Stage>(tStagedSpell);
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

    public static DynValue GlobalTryStrict(ScriptExecutionContext ctx, CallbackArguments args)
    {
        var actionResult = args.AsInt(0, "TryStrict");
        return DynValue.NewBoolean(EnergyController.TryStrict((EnergyActionResult)actionResult));
    }

    public static DynValue GlobalTry(ScriptExecutionContext ctx, CallbackArguments args)
    {
        var actionResult = args.AsInt(0, "Try");
        return DynValue.NewBoolean(EnergyController.Try((EnergyActionResult)actionResult));
    }

    public static DynValue SpellUtilities_FindClosestEnemy(ScriptExecutionContext ctx, CallbackArguments args)
    {
        var wizard = args.AsUserData<Wizard>(0, "SpellUtilities.FindClosestEnemy", false);
        var maxDistance = float.PositiveInfinity;
        if (args.Count == 2)
        {
            maxDistance = (float)args.RawGet(1, true).Number;
        }

        return DynValue.FromObject(ctx.OwnerScript, SpellUtilities.FindClosestEnemy(wizard, maxDistance));
    }

    public static DynValue SpellUtilities_FindClosestFriend(ScriptExecutionContext ctx, CallbackArguments args)
    {
        var wizard = args.AsUserData<Wizard>(0, "SpellUtilities.FindClosestFriend", false);
        var maxDistance = float.PositiveInfinity;
        if (args.Count == 2)
        {
            maxDistance = (float)args.RawGet(1, true).Number;
        }

        return DynValue.FromObject(ctx.OwnerScript, SpellUtilities.FindClosestFriend(wizard, maxDistance));
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


    public static DynValue Spell_Summon(ScriptExecutionContext ctx, CallbackArguments args)
    {
        var spell = GetSpell<SpellComponent>(args);
        var handle = args.AsInt(1, "SpellComponent.Summon");
        var recipe = args.AsUserData<SummonRecipe>(2, "SpellComponent.Summon", false);
        GameObject summonedObj;
        var result = spell.Summon(handle, recipe, out summonedObj);
        return DynValue.NewTuple(DynValue.NewNumber((int)result), DynValue.FromObject(ctx.OwnerScript, summonedObj));
    }

    public static DynValue Spell_Charge(ScriptExecutionContext ctx, CallbackArguments args)
    {
        var spell = GetSpell<SpellComponent>(args);
        var handle = args.AsInt(1, "SpellComponent.Charge");
        var amount = args.AsInt(2, "SpellComponent.Charge");
        var result = spell.Charge(handle, amount);
        return DynValue.NewNumber((int)result);
    }

    public static DynValue Spell_Discharge(ScriptExecutionContext ctx, CallbackArguments args)
    {
        var spell = GetSpell<SpellComponent>(args);
        var handle = args.AsInt(1, "SpellComponent.Discharge");
        var amount = args.AsInt(2, "SpellComponent.Discharge");
        var result = spell.Discharge(handle, amount);
        return DynValue.NewNumber((int)result);
    }

    public static DynValue Spell_Merge(ScriptExecutionContext ctx, CallbackArguments args)
    {
        var spell = GetSpell<SpellComponent>(args);
        var handleA = args.AsInt(1, "SpellComponent.Merge");
        var handleB = args.AsInt(2, "SpellComponent.Merge");
        var result = spell.Merge(handleA, handleB);
        return DynValue.NewNumber((int)result);
    }

    public static DynValue Spell_Separate(ScriptExecutionContext ctx, CallbackArguments args)
    {
        var spell = GetSpell<SpellComponent>(args);
        var handle = args.AsInt(1, "SpellComponent.Separate");
        var amount = args.AsInt(2, "SpellComponent.Separate");
        var force = args.AsUserData<Vector3>(3, "SpellComponent.Separate", false);
        var forceMode = (ForceMode)args.AsInt(4, "SpellComponent.Separate");
        int separatedEnergyFocusHandle;
        var result = spell.Separate(handle, amount, force, forceMode, out separatedEnergyFocusHandle);
        return DynValue.NewTuple(DynValue.NewNumber((int)result), DynValue.FromObject(ctx.OwnerScript, separatedEnergyFocusHandle));
    }

    public static DynValue Spell_ChangeElement(ScriptExecutionContext ctx, CallbackArguments args)
    {
        var spell = GetSpell<SpellComponent>(args);
        var handle = args.AsInt(1, "SpellComponent.ChangeElement");
        var newElement = (Energy.Element)args.AsInt(2, "SpellComponent.ChangeElement");
        var result = spell.ChangeElement(handle, newElement);
        return DynValue.NewNumber((int)result);
    }

    public static DynValue Spell_ChangeShape(ScriptExecutionContext ctx, CallbackArguments args)
    {
        var spell = GetSpell<SpellComponent>(args);
        var handle = args.AsInt(1, "SpellComponent.ChangeShape");
        var newShape = (Energy.Shape)args.AsInt(2, "SpellComponent.ChangeShape");
        var result = spell.ChangeShape(handle, newShape);
        return DynValue.NewNumber((int)result);
    }

    public static DynValue Spell_Deform(ScriptExecutionContext ctx, CallbackArguments args)
    {
        var spell = GetSpell<SpellComponent>(args);
        var handle = args.AsInt(1, "SpellComponent.Deform");
        var stress = args.AsUserData<Vector3>(1, "SpellComponent.Deform");
        var result = spell.Deform(handle, stress);
        return DynValue.NewNumber((int)result);
    }

    public static DynValue Spell_CreateElasticConnection(ScriptExecutionContext ctx, CallbackArguments args)
    {
        var spell = GetSpell<SpellComponent>(args);
        var handleA = args.AsInt(1, "SpellComponent.CreateElasticConnection");
        var handleB = args.AsInt(2, "SpellComponent.CreateElasticConnection");
        var connectionCharge = args.AsInt(3, "SpellComponent.CreateElasticConnection");
        var result = spell.CreateElasticConnection(handleA, handleB, connectionCharge);
        return DynValue.NewNumber((int)result);
    }

    public static DynValue Spell_ApplyForceRelative(ScriptExecutionContext ctx, CallbackArguments args)
    {
        var spell = GetSpell<SpellComponent>(args);
        var handle = args.AsInt(1, "SpellComponent.ApplyForceRelative");
        var relativeForce = args.AsUserData<Vector3>(2, "SpellComponent.Separate", false);
        var mode = (ForceMode)args.AsInt(3, "SpellComponent.Separate");
        var result = spell.ApplyForceRelative(handle, relativeForce, mode);
        return DynValue.NewNumber((int)result);
    }

    public static DynValue Spell_ApplyForce(ScriptExecutionContext ctx, CallbackArguments args)
    {
        var spell = GetSpell<SpellComponent>(args);
        var handle = args.AsInt(1, "SpellComponent.ApplyForce");
        var force = args.AsUserData<Vector3>(2, "SpellComponent.ApplyForce", false);
        var mode = (ForceMode)args.AsInt(3, "SpellComponent.ApplyForce");
        var result = spell.ApplyForce(handle, force, mode);
        return DynValue.NewNumber((int)result);
    }

    public static DynValue Spell_ApplyTorque(ScriptExecutionContext ctx, CallbackArguments args)
    {
        var spell = GetSpell<SpellComponent>(args);
        var handle = args.AsInt(1, "SpellComponent.ApplyTorque");
        var torque = args.AsUserData<Vector3>(2, "SpellComponent.ApplyTorque", false);
        var mode = (ForceMode)args.AsInt(3, "SpellComponent.ApplyTorque");
        var result = spell.ApplyTorque(handle, torque, mode);
        return DynValue.NewNumber((int)result);
    }

    public static DynValue Spell_OrientTowards(ScriptExecutionContext ctx, CallbackArguments args)
    {
        var spell = GetSpell<SpellComponent>(args);
        var handle = args.AsInt(1, "SpellComponent.OrientTowards");
        var lookat = args.AsUserData<Vector3>(2, "SpellComponent.OrientTowards", false); //TODO add GameObject as alternate object
        var result = spell.OrientTowards(handle, lookat);
        return DynValue.NewNumber((int)result);
    }
    
    /*public static DynValue Spell_ApplyAura<T>(ScriptExecutionContext ctx, CallbackArguments args) //int focusHandle, GameObject obj, int extractedEnergy, out T aura
    {
        var spell = GetSpell<SpellComponent>(args);
        var handle = args.AsInt(1, "SpellComponent.ApplyAura<T>");
        var result = spell.ApplyAura<T>(handle, GameObject obj, int extractedEnergy, out T aura);
        return DynValue.NewNumber((int)result);
    }*/
 
    public static DynValue Spell_Substitute(ScriptExecutionContext ctx, CallbackArguments args)
    {
        var spell = GetSpell<SpellComponent>(args);
        var handle = args.AsInt(1, "SpellComponent.Substitute");
        var obj = args.AsUserData<GameObject>(2, "SpellComponent.Substitute", false);
        if (args.Count == 3)
        {
            var result = spell.Substitute(handle, obj);
            return DynValue.NewNumber((int)result);
        }
        else
        {
            var obj2 = args.AsUserData<GameObject>(2, "SpellComponent.Substitute", false);
            var result = spell.Substitute(handle, obj, obj2);
            return DynValue.NewNumber((int)result);
        }
    }


    public static DynValue Spell_CounterFalling(ScriptExecutionContext ctx, CallbackArguments args)
    {
        var spell = GetSpell<SpellComponent>(args);
        var handle = args.AsInt(1, "SpellComponent.CounterFalling");
        var result = spell.CounterFalling(handle);
        return DynValue.NewNumber((int)result);
    }

    public static DynValue Spell_CounterGravity(ScriptExecutionContext ctx, CallbackArguments args)
    {
        var spell = GetSpell<SpellComponent>(args);
        var handle = args.AsInt(1, "SpellComponent.CounterGravity");
        var result = spell.CounterGravity(handle);
        return DynValue.NewNumber((int)result);
    }

    public static DynValue Spell_CounterMotion(ScriptExecutionContext ctx, CallbackArguments args)
    {
        var spell = GetSpell<SpellComponent>(args);
        var handle = args.AsInt(1, "SpellComponent.CounterMotion");
        var result = spell.CounterMotion(handle);
        return DynValue.NewNumber((int)result);
    }

    #endregion

    #region Specific Spell Components

    public static DynValue ToggleSpell_GetInterval(ScriptExecutionContext ctx, CallbackArguments args)
    {
        return DynValue.NewNumber(GetSpell<ToggleSpellComponent>(args).interval);
    }

    public static DynValue ToggleSpell_SetInterval(ScriptExecutionContext ctx, CallbackArguments args)
    {
        var spell = GetSpell<ToggleSpellComponent>(args);
        var interval = (float)args.RawGet(1, false).Number;
        spell.interval = interval;
        return DynValue.Nil;
    }

    public static DynValue StagedSpell_GetCurrentStage(ScriptExecutionContext ctx, CallbackArguments args)
    {
        return DynValue.NewNumber((int)GetSpell<StagedSpellComponent>(args).stage);
    }

    public static DynValue StagedSpell_NextStage(ScriptExecutionContext ctx, CallbackArguments args)
    {
        var spell = GetSpell<StagedSpellComponent>(args);
        spell.NextStage();
        return DynValue.Nil;
    }

    #endregion
}
