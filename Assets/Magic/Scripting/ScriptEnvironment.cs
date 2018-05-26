using MoonSharp.Interpreter;
using UnityEngine;

public class ScriptEnvironment
{
    public Script L;
    
    public ScriptEnvironment()
    {
        UserData.RegisterAssembly();
        L = new Script(CoreModules.Preset_Complete);
        
        ScriptLibrary.Bind(L);
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

    public void DoFile(string filePath)
    {
        var script = Resources.Load("Scripts/" + filePath);
        if (script == null)
        {
            Debug.LogErrorFormat("[Script][Error] Cannot find file {0}", filePath);
            return;
        }

        L.DoFile(script.name);
    }

    public void DoFolder(string folderPath)
    {
        if (!string.IsNullOrEmpty(folderPath) && !folderPath.EndsWith("/"))
        {
            folderPath += "/";
        }
        folderPath = "Scripts/" + folderPath;

        var scripts = Resources.LoadAll("Scripts");
        foreach (var script in scripts)
        {
            L.DoFile(folderPath + script.name);
        }
    }
}
