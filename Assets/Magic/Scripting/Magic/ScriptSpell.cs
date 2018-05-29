using MoonSharp.Interpreter;

public interface IScriptSpell
{
    string SpellType { get; }
    void Bind(Script L, DynValue component);
    DynValue Call(string method, params object[] args);
}

[MoonSharpUserData]
public class ScriptInstantSpell : InstantSpellComponent, IScriptSpell
{
    Script L;
    DynValue component;
    public string SpellType { get { return "Instant"; } }
    public void Bind(Script L, DynValue component) { this.L = L; this.component = component; }
    public DynValue Call(string method, params object[] args) { return L.Call(component.Table.GetField(method), component, args); }

    public override void Cast() { Call("Cast"); }
    
    public override void OnTargetLost() { Call("OnTargetLost"); }
    public override void OnFocusLost(int handle) { Call("OnFocusLost", handle); }
}

[MoonSharpUserData]
public class ScriptContinuousSpell : ContinuousSpellComponent, IScriptSpell
{
    Script L;
    DynValue component;
    public string SpellType { get { return "Continuous"; } }
    public void Bind(Script L, DynValue component) { this.L = L; this.component = component; }
    public DynValue Call(string method, params object[] args) { return L.Call(component.Table.GetField(method), component, args); }

    public override void OnBegin() { Call("OnBegin"); }
    public override void Activate(float dt) { Call("Activate", dt); }
    public override void OnFinish() { Call("OnFinish"); }

    public override void OnTargetLost() { Call("OnTargetLost"); }
    public override void OnFocusLost(int handle) { Call("OnFocusLost", handle); }
}

[MoonSharpUserData]
public class ScriptToggleSpell : ToggleSpellComponent, IScriptSpell
{
    Script L;
    DynValue component;
    public string SpellType { get { return "Toggle"; } }
    public void Bind(Script L, DynValue component) { this.L = L; this.component = component; }
    public DynValue Call(string method, params object[] args) { return L.Call(component.Table.GetField(method), component, args); }

    public override void Activate(float dt) { Call("Activate", dt); }
    public override void OnToggle(bool active) { Call("OnToggle", active); }

    public override void OnTargetLost() { Call("OnTargetLost"); }
    public override void OnFocusLost(int handle) { Call("OnFocusLost", handle); }
}

[MoonSharpUserData]
public class ScriptStagedSpell : StagedSpellComponent, IScriptSpell
{
    Script L;
    DynValue component;
    public string SpellType { get { return "Staged"; } }
    public void Bind(Script L, DynValue component) { this.L = L; this.component = component; }
    public DynValue Call(string method, params object[] args) { return L.Call(component.Table.GetField(method), component, args); }

    public override void OnBegin() { Call("OnBegin"); }
    public override void Cast(float dt) { Call("Cast", dt); }
    public override void Execute(float dt) { Call("Execute", dt); }
    public override void OnFinish() { Call("OnFinish"); }

    public override void OnTargetLost() { Call("OnTargetLost"); }
    public override void OnFocusLost(int handle) { Call("OnFocusLost", handle); }
}
