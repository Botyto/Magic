using MoonSharp.Interpreter;
using UnityEngine;

public class ScriptEnvironment
{
    public Script L;
    
    public ScriptEnvironment()
    {
        UserData.RegisterAssembly();
        L = new Script(CoreModules.Preset_Complete);

        L.Globals["print"] = (System.Action<string>)Print;
    }

    public void Print(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        if (text[0] == '[')
        {
            Debug.LogFormat("[Script]{0}", text);
        }
        else
        {
            Debug.LogFormat("[Script] {0}", text);
        }
    }

    public DynValue DoString(string code, string name = "c#string")
    {
        try
        {
            return L.DoString(code, null, name);
        }
        catch (ScriptRuntimeException ex)
        {
            Debug.LogErrorFormat("[Script][Error] {0}", ex.DecoratedMessage);
            return null;
        }
    }
}
