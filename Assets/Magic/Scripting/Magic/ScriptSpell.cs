using System;
using UnityEngine;
using MoonSharp.Interpreter;
using System.Text;

public interface IScriptSpell
{
    string SpellType { get; }
    void Bind(Script L, DynValue component);
    DynValue CallScript(string method);
    DynValue CallScript(string method, params object[] args);
}

public class ScriptInstantSpell : InstantSpellComponent, IScriptSpell
{
    Script L;
    DynValue component;
    public string spellScriptClass;
    public string SpellType { get { return "Instant"; } }
    public void Bind(Script L, DynValue component) { this.L = L; this.component = component; }
    public DynValue CallScript(string method) { return L.Call(component.Table.GetField(method), component); }
    public DynValue CallScript(string method, params object[] args) { return L.Call(component.Table.GetField(method), component, args); }
    protected override void HandleException(Exception exception)
    {
        var scriptException = exception as InterpreterException;
        if (scriptException != null)
        {
            var sb = new StringBuilder();
            sb.AppendFormat("[Script][Error] {0}\n", exception.Message);
            sb.AppendLine("Lua stacktrace (most recent call on top):");

            foreach (var wi in scriptException.CallStack)
            {
                string name;

                if (wi.Name == null)
                    if (wi.RetAddress < 0)
                        name = "main chunk";
                    else
                        name = "?";
                else
                    name = "function '" + wi.Name + "'";

                string loc = wi.Location != null ? wi.Location.FormatLocation(L) : "[clr]";
                sb.AppendFormat("\t{0}: in {1}\n", loc, name);
            }

            MagicLog.LogError(sb.ToString());
        }

        MagicLog.LogErrorFormat("Spell '{0}' failed due to an exception: '{1}' (see below)", GetType().Name, exception.Message);
        MagicLog.LogException(exception);
        Cancel();
    }

    public override void Cast() { CallScript("Cast"); }
    
    public override void OnTargetLost() { CallScript("OnTargetLost"); }
    public override void OnFocusLost(int handle) { CallScript("OnFocusLost", handle); }
    
}

public class ScriptContinuousSpell : ContinuousSpellComponent, IScriptSpell
{
    Script L;
    DynValue component;
    public string spellScriptClass;
    public string SpellType { get { return "Continuous"; } }
    public void Bind(Script L, DynValue component) { this.L = L; this.component = component; }
    public DynValue CallScript(string method) { return L.Call(component.Table.GetField(method), component); }
    public DynValue CallScript(string method, params object[] args) { return L.Call(component.Table.GetField(method), component, args); }
    protected override void HandleException(Exception exception)
    {
        var scriptException = exception as InterpreterException;
        if (scriptException != null)
        {
            var sb = new StringBuilder();
            sb.AppendFormat("[Script][Error] {0}\n", exception.Message);
            sb.AppendLine("Lua stacktrace (most recent call on top):");

            foreach (var wi in scriptException.CallStack)
            {
                string name;

                if (wi.Name == null)
                    if (wi.RetAddress < 0)
                        name = "main chunk";
                    else
                        name = "?";
                else
                    name = "function '" + wi.Name + "'";

                string loc = wi.Location != null ? wi.Location.FormatLocation(L) : "[clr]";
                sb.AppendFormat("\t{0}: in {1}\n", loc, name);
            }

            MagicLog.LogError(sb.ToString());
        }

        MagicLog.LogErrorFormat("Spell '{0}' failed due to an exception: '{1}' (see below)", GetType().Name, exception.Message);
        MagicLog.LogException(exception);
        Cancel();
    }

    public override void OnBegin() { CallScript("OnBegin"); }
    public override void Activate(float dt) { CallScript("Activate", dt); }
    public override void OnFinish() { CallScript("OnFinish"); }

    public override void OnTargetLost() { CallScript("OnTargetLost"); }
    public override void OnFocusLost(int handle) { CallScript("OnFocusLost", handle); }

}

public class ScriptToggleSpell : ToggleSpellComponent, IScriptSpell
{
    Script L;
    DynValue component;
    public string spellScriptClass;
    public string SpellType { get { return "Toggle"; } }
    public void Bind(Script L, DynValue component) { this.L = L; this.component = component; }
    public DynValue CallScript(string method) { return L.Call(component.Table.GetField(method), component); }
    public DynValue CallScript(string method, params object[] args) { return L.Call(component.Table.GetField(method), component, args); }
    protected override void HandleException(Exception exception)
    {
        var scriptException = exception as InterpreterException;
        if (scriptException != null)
        {
            var sb = new StringBuilder();
            sb.AppendFormat("[Script][Error] {0}\n", exception.Message);
            sb.AppendLine("Lua stacktrace (most recent call on top):");

            foreach (var wi in scriptException.CallStack)
            {
                string name;

                if (wi.Name == null)
                    if (wi.RetAddress < 0)
                        name = "main chunk";
                    else
                        name = "?";
                else
                    name = "function '" + wi.Name + "'";

                string loc = wi.Location != null ? wi.Location.FormatLocation(L) : "[clr]";
                sb.AppendFormat("\t{0}: in {1}\n", loc, name);
            }

            MagicLog.LogError(sb.ToString());
        }

        MagicLog.LogErrorFormat("Spell '{0}' failed due to an exception: '{1}' (see below)", GetType().Name, exception.Message);
        MagicLog.LogException(exception);
        Cancel();
    }

    public override void Activate(float dt) { CallScript("Activate", dt); }
    public override void OnToggle(bool active) { CallScript("OnToggle", active); }

    public override void OnTargetLost() { CallScript("OnTargetLost"); }
    public override void OnFocusLost(int handle) { CallScript("OnFocusLost", handle); }

}

public class ScriptStagedSpell : StagedSpellComponent, IScriptSpell
{
    Script L;
    DynValue component;
    public string spellScriptClass;
    public string SpellType { get { return "Staged"; } }
    public void Bind(Script L, DynValue component) { this.L = L; this.component = component; }
    public DynValue CallScript(string method) { return L.Call(component.Table.GetField(method), component); }
    public DynValue CallScript(string method, params object[] args) { return L.Call(component.Table.GetField(method), component, args); }
    protected override void HandleException(Exception exception)
    {
        var scriptException = exception as InterpreterException;
        if (scriptException != null)
        {
            var sb = new StringBuilder();
            sb.AppendFormat("[Script][Error] {0}\n", exception.Message);
            sb.AppendLine("Lua stacktrace (most recent call on top):");

            foreach (var wi in scriptException.CallStack)
            {
                string name;

                if (wi.Name == null)
                    if (wi.RetAddress < 0)
                        name = "main chunk";
                    else
                        name = "?";
                else
                    name = "function '" + wi.Name + "'";

                string loc = wi.Location != null ? wi.Location.FormatLocation(L) : "[clr]";
                sb.AppendFormat("\t{0}: in {1}\n", loc, name);
            }

            MagicLog.LogError(sb.ToString());
        }

        MagicLog.LogErrorFormat("Spell '{0}' failed due to an exception: '{1}' (see below)", GetType().Name, exception.Message);
        MagicLog.LogException(exception);
        Cancel();
    }

    public override void OnBegin() { CallScript("OnBegin"); }
    public override void Cast(float dt) { CallScript("Cast", dt); }
    public override void Execute(float dt) { CallScript("Execute", dt); }
    public override void OnFinish() { CallScript("OnFinish"); }

    public override void OnTargetLost() { CallScript("OnTargetLost"); }
    public override void OnFocusLost(int handle) { CallScript("OnFocusLost", handle); }

}
