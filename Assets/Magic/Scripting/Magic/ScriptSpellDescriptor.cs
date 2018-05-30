using System;
using UnityEngine;
using MoonSharp.Interpreter;
using MoonSharp.VsCodeDebugger;

[Serializable]
[CreateAssetMenu(fileName = "New Script Spell", menuName = "Magic/Script Spell Descriptor")]
public class ScriptSpellDescriptor : SpellDescriptor
{
    public string scriptSpellName;

    public override SpellCastResult Cast(Wizard wizard, GameObject target, out SpellComponent spell)
    {
        var env = new ScriptEnvironment();
        var server = new MoonSharpVsCodeDebugServer();
        server.AttachToScript(env.L, "Magic:" + scriptSpellName);

        var scriptSpellDef = env.L.Globals.Get(scriptSpellName);
        if (scriptSpellDef == null || scriptSpellDef.Type != DataType.Table)
        {
            spell = null;
            MagicLog.LogErrorFormat("Casting spell '{0}' failed! No script counterpart found!", id);
            return SpellCastResult.InvalidDescriptor;
        }

        //TODO: move 'SpellType' check from below
        //TODO: check if class is indeed a IScriptSpell (remove from below)

        var castResult = base.Cast(wizard, target, out spell);
        if (castResult == SpellCastResult.Success)
        {
            var scriptSpell = spell as IScriptSpell;

            //TODO: Move class check before creation
            if (scriptSpell == null)
            {
                Util.Destroy(spell);
                MagicLog.LogErrorFormat("Casting spell '{0}' failed! '{1}' is not a script-compatible spell class!", id, spellClass);
                return SpellCastResult.InvalidDescriptor;
            }

            //TODO: Move type check before creation
            if (scriptSpell.SpellType != scriptSpellDef.Table.GetField("SpellType").String)
            {
                Util.Destroy(spell);
                MagicLog.LogErrorFormat("Casting spell '{0}' failed! Different Spell class type from the script spell type!", id);
                return SpellCastResult.InvalidDescriptor;
            }

            var newFunction = scriptSpellDef.Table.GetField("new");
            if (newFunction == null || (newFunction.Type != DataType.Function && newFunction.Type != DataType.ClrFunction))
            {
                Util.Destroy(spell);
                MagicLog.LogErrorFormat("Casting spell '{0}' failed! No script constructor found!", id);
                return SpellCastResult.InvalidDescriptor;
            }

            var obj = new Table(env.L);
            obj[true] = spell;
            var scriptComponent = newFunction.Function.Call(scriptSpellDef, obj);
            scriptSpell.Bind(env.L, scriptComponent);
        }

        return castResult;
    }
}
