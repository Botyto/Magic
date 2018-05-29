using MoonSharp.Interpreter;

public interface IScriptSpell
{
    string SpellType { get; }
    void Bind(Script L, DynValue component);
    DynValue CallScript(string method);
    DynValue CallScript(string method, params object[] args);
}

[MoonSharpUserData]
public class ScriptInstantSpell : InstantSpellComponent, IScriptSpell
{
    Script L;
    DynValue component;
    public string SpellType { get { return "Instant"; } }
    public void Bind(Script L, DynValue component) { this.L = L; this.component = component; }
    public DynValue CallScript(string method) { return L.Call(component.Table.GetField(method), component); }
    public DynValue CallScript(string method, params object[] args) { return L.Call(component.Table.GetField(method), component, args); }

    public override void Cast() { CallScript("Cast"); }
    
    public override void OnTargetLost() { CallScript("OnTargetLost"); }
    public override void OnFocusLost(int handle) { CallScript("OnFocusLost", handle); }
}

[MoonSharpUserData]
public class ScriptContinuousSpell : ContinuousSpellComponent, IScriptSpell
{
    Script L;
    DynValue component;
    public string SpellType { get { return "Continuous"; } }
    public void Bind(Script L, DynValue component) { this.L = L; this.component = component; }
    public DynValue CallScript(string method) { return L.Call(component.Table.GetField(method), component); }
    public DynValue CallScript(string method, params object[] args) { return L.Call(component.Table.GetField(method), component, args); }

    public override void OnBegin() { CallScript("OnBegin"); }
    public override void Activate(float dt) { CallScript("Activate", dt); }
    public override void OnFinish() { CallScript("OnFinish"); }

    public override void OnTargetLost() { CallScript("OnTargetLost"); }
    public override void OnFocusLost(int handle) { CallScript("OnFocusLost", handle); }
}

[MoonSharpUserData]
public class ScriptToggleSpell : ToggleSpellComponent, IScriptSpell
{
    Script L;
    DynValue component;
    public string SpellType { get { return "Toggle"; } }
    public void Bind(Script L, DynValue component) { this.L = L; this.component = component; }
    public DynValue CallScript(string method) { return L.Call(component.Table.GetField(method), component); }
    public DynValue CallScript(string method, params object[] args) { return L.Call(component.Table.GetField(method), component, args); }

    public override void Activate(float dt) { CallScript("Activate", dt); }
    public override void OnToggle(bool active) { CallScript("OnToggle", active); }

    public override void OnTargetLost() { CallScript("OnTargetLost"); }
    public override void OnFocusLost(int handle) { CallScript("OnFocusLost", handle); }
}

[MoonSharpUserData]
public class ScriptStagedSpell : StagedSpellComponent, IScriptSpell
{
    Script L;
    DynValue component;
    public string SpellType { get { return "Staged"; } }
    public void Bind(Script L, DynValue component) { this.L = L; this.component = component; }
    public DynValue CallScript(string method) { return L.Call(component.Table.GetField(method), component); }
    public DynValue CallScript(string method, params object[] args) { return L.Call(component.Table.GetField(method), component, args); }

    public override void OnBegin() { CallScript("OnBegin"); }
    public override void Cast(float dt) { CallScript("Cast", dt); }
    public override void Execute(float dt) { CallScript("Execute", dt); }
    public override void OnFinish() { CallScript("OnFinish"); }

    public override void OnTargetLost() { CallScript("OnTargetLost"); }
    public override void OnFocusLost(int handle) { CallScript("OnFocusLost", handle); }
}
