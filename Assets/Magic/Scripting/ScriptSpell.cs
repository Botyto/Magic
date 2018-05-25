﻿using MoonSharp.Interpreter;

public interface IScriptSpell
{
    void Bind(Script L, DynValue component);
    void Call(string method, params object[] args);
}

public class ScriptInstantSpell : InstantSpellComponent, IScriptSpell
{
    Script L;
    DynValue component;
    public void Bind(Script L, DynValue component) { this.L = L; this.component = component; }
    public void Call(string method, params object[] args) { L.Call(component.Table[method], component, args); }

    public override void Cast() { Call("Cast"); }

    public override void OnTargetLost() { Call("OnTargetLost"); }
    public override void OnFocusLost(int handle) { Call("OnFocusLost", handle); }
    
}

public class ScriptContinuousSpell : ContinuousSpellComponent, IScriptSpell
{
    Script L;
    DynValue component;
    public void Bind(Script L, DynValue component) { this.L = L; this.component = component; }
    public void Call(string method, params object[] args) { L.Call(component.Table[method], args); }

    public override void OnBegin() { Call("OnBegin"); }
    public override void Activate(float dt) { Call("Activate", dt); }
    public override void OnFinish() { Call("OnFinish"); }

    public override void OnTargetLost() { Call("OnTargetLost"); }
    public override void OnFocusLost(int handle) { Call("OnFocusLost", handle); }
}

public class ScriptToggleSpell : ToggleSpellComponent, IScriptSpell
{
    Script L;
    DynValue component;
    public void Bind(Script L, DynValue component) { this.L = L; this.component = component; }
    public void Call(string method, params object[] args) { L.Call(component.Table[method], args); }

    public override void Activate(float dt) { Call("Activate", dt); }
    public override void OnToggle(bool active) { Call("OnToggle", active); }

    public override void OnTargetLost() { Call("OnTargetLost"); }
    public override void OnFocusLost(int handle) { Call("OnFocusLost", handle); }
}

public class ScriptStagedSpell : StagedSpellComponent, IScriptSpell
{
    Script L;
    DynValue component;
    public void Bind(Script L, DynValue component) { this.L = L; this.component = component; }
    public void Call(string method, params object[] args) { L.Call(component.Table[method], args); }

    public override void OnBegin() { Call("OnBegin"); }
    public override void Cast(float dt) { Call("Cast", dt); }
    public override void Execute(float dt) { Call("Execute", dt); }
    public override void OnFinish() { Call("OnFinish"); }

    public override void OnTargetLost() { Call("OnTargetLost"); }
    public override void OnFocusLost(int handle) { Call("OnFocusLost", handle); }
}