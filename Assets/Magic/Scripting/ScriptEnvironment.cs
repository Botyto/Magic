using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Loaders;
using UnityEngine;

public class ScriptEnvironment
{
    public Script L;

    public static ScriptEnvironment FetchEnvironment(Script L)
    {
        return L.Globals.RawGet("__SCRIPT_ENVIRONMENT").ToObject<ScriptEnvironment>();
    }

    public ScriptEnvironment()
    {
        UserData.RegisterAssembly();
        UserData.RegisterType<ScriptEnvironment>();

        L = new Script(CoreModules.Preset_Complete);
        L.Options.ScriptLoader = new UnityAssetsScriptLoader("Scripts");
        L.Globals["__SCRIPT_ENVIRONMENT"] = DynValue.FromObject(L, this);

        ReloadScripts();
    }

    public void ReloadScripts()
    {
        ScriptLibrary.Bind(L);
        DoFile("Scripts/autorun");
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

    public int DoFile(string filePath)
    {
        var script = Resources.Load(filePath);
        if (script == null)
        {
            Debug.LogErrorFormat("[Script][Error] Cannot find file {0}", filePath);
            return 0;
        }

        L.DoFile(script.name);
        return 1;
    }

    public int DoFolder(string folderPath)
    {
        if (!string.IsNullOrEmpty(folderPath) && !folderPath.EndsWith("/"))
        {
            folderPath += "/";
        }

        var scripts = Resources.LoadAll("Scripts");
        foreach (var script in scripts)
        {
            L.DoFile(folderPath + script.name);
        }

        return scripts.Length;
    }
}

public static class MoonsharpExtensions
{
    public static DynValue GetField(this Table table, object key)
    {
        return table.GetField(DynValue.FromObject(table.OwnerScript, key));
    }

    public static DynValue GetField(this Table table, string key)
    {
        return table.GetField(DynValue.NewString(key));
    }

    public static DynValue GetField(this Table table, DynValue key)
    {
        var get = table.Get(key);
        if (get.IsNil() && table.MetaTable != null && table.MetaTable.Get("__index").IsNotNil())
        {
            var index = table.MetaTable.Get("__index");
            if (index.Table != null)
            {
                get = index.Table.GetField(key);
            }
            else if (index.Function != null)
            {
                get = index.Function.Call(key);
            }
        }
        return get;
    }
}
