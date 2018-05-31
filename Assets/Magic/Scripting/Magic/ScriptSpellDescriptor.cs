using System;
using UnityEngine;
using MoonSharp.Interpreter;
using MoonSharp.VsCodeDebugger;

[Serializable]
[CreateAssetMenu(fileName = "New Script Spell", menuName = "Magic/Script Spell Descriptor")]
public class ScriptSpellDescriptor : SpellDescriptor
{
    public string spellScriptClass;

    private ScriptEnvironment m_TempScriptEnvironment; //TODO - workaround this bad hack

    public override SpellCastResult Cast(Wizard wizard, GameObject target, out SpellComponent spell)
    {
        var env = new ScriptEnvironment();

#if DEBUG
        //Attach debugger
        //var server = new MoonSharpVsCodeDebugServer();
        //server.AttachToScript(env.L, "Magic:" + scriptSpellName);
#endif

        //Check if C# spell class is a script-bound spell component
        if (!typeof(IScriptSpell).IsAssignableFrom(spellType))
        {
            spell = null;
            MagicLog.LogErrorFormat("Casting spell '{0}' failed! '{1}' is not a script-compatible spell class!", id, spellClass);
            return SpellCastResult.InvalidDescriptor;
        }

        //Check if there is a script definition of this spell
        var scriptSpellDef = env.L.Globals.Get(spellScriptClass);
        if (scriptSpellDef == null || scriptSpellDef.Type != DataType.Table)
        {
            spell = null;
            MagicLog.LogErrorFormat("Casting spell '{0}' failed! No script counterpart found!", id);
            return SpellCastResult.InvalidDescriptor;
        }
        
        //Check if the script definition has a constructor function
        var ctorFunc = scriptSpellDef.Table.GetField("new");
        if (ctorFunc == null || (ctorFunc.Type != DataType.Function && ctorFunc.Type != DataType.ClrFunction))
        {
            spell = null;
            MagicLog.LogErrorFormat("Casting spell '{0}' failed! No script constructor found!", id);
            return SpellCastResult.InvalidDescriptor;
        }
        
        //Cast the spell
        SpellCastResult castResult;
        lock (this)
        {
            m_TempScriptEnvironment = env;
            castResult = base.Cast(wizard, target, out spell);
            m_TempScriptEnvironment = null;
        }

        //Bind the spell
        if (castResult == SpellCastResult.Success)
        {
            var scriptSpell = spell as IScriptSpell;
            
            //TODO - Move spell behaviour type check before creation (script & component types must be the same)
            if (scriptSpell.SpellType != scriptSpellDef.Table.GetField("SpellType").String)
            {
                Util.Destroy(spell);
                MagicLog.LogErrorFormat("Casting spell '{0}' failed! Different Spell class type from the script spell type!", id);
                return SpellCastResult.InvalidDescriptor;
            }
            
            //Bind the spell
            var obj = new Table(env.L);
            obj[true] = spell;
            var scriptComponent = ctorFunc.Function.Call(scriptSpellDef, obj);
            scriptSpell.Bind(env.L, scriptComponent);
        }

        return castResult;
    }

    public override GameObject TryFindTarget(Wizard wizard)
    {
        var target = base.TryFindTarget(wizard);
        if (target != null)
        {
            return target;
        }

        //Access the script 'TryFindTarget' method and call it
        var scriptSpellDef = m_TempScriptEnvironment.L.Globals.Get(spellScriptClass);
        var tryFindTargetFunc = scriptSpellDef.Table.GetField("TryFindTarget");
        if (tryFindTargetFunc.Type != DataType.Function && tryFindTargetFunc.Type != DataType.ClrFunction)
        {
            return null;
        }

        return tryFindTargetFunc.Function.Call(wizard).ToObject<GameObject>();
    }
}
