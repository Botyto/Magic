﻿using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Loaders;
using UnityEngine;

public class ScriptEnvironment
{
    public Script L;

    public static ScriptEnvironment FetchEnvironment(Script L)
    {
        return L.Globals.RawGet("__SCRIPT_ENVIRONMENT").ToObject<ScriptEnvironment>();
    }

    public ScriptEnvironment(string additionalCode = "")
    {
        UserData.RegisterAssembly();
        UserData.RegisterType<ScriptEnvironment>();

        L = new Script(CoreModules.Preset_Complete);
        L.Options.ScriptLoader = new UnityAssetsScriptLoader("Scripts");
        L.Globals["__SCRIPT_ENVIRONMENT"] = DynValue.FromObject(L, this);
        L.Globals["__ADDITIONAL_CODE"] = DynValue.FromObject(L, additionalCode);
#if DEBUG
        L.Globals["__DEBUG"] = true;
#else
        L.Globals["__DEBUG"] = false;
#endif

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
        catch (InterpreterException e)
        {
            MagicLog.LogErrorFormat("[Script][Error] {0}", e.DecoratedMessage);
            return null;
        }
    }

    public int DoFile(string filePath)
    {
        var script = Resources.Load(filePath);
        if (script == null)
        {
            MagicLog.LogErrorFormat("[Script][Error] Cannot find file {0}", filePath);
            return 0;
        }

        L.DoFile(script.name, null, filePath);
        return 1;
    }

    public int DoFolder(string folderPath)
    {
        if (!string.IsNullOrEmpty(folderPath) && !folderPath.EndsWith("/"))
        {
            folderPath += "/";
        }

        var scripts = Resources.LoadAll<TextAsset>("Scripts");
        foreach (var script in scripts)
        {
            var filePath = folderPath + script.name;
            L.DoFile(filePath, null, filePath);
        }

        return scripts.Length;
    }

    public int DoFolder(string folderPath, Closure filterFunction)
    {
        if (!string.IsNullOrEmpty(folderPath) && !folderPath.EndsWith("/"))
        {
            folderPath += "/";
        }

        var scripts = Resources.LoadAll<TextAsset>("Scripts");
        foreach (var script in scripts)
        {
            var filePath = folderPath + script.name;
            if (filterFunction.Call(filePath).Boolean)
            {
                L.DoFile(filePath, null, filePath);
            }
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
