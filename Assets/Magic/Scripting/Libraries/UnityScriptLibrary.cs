using MoonSharp.Interpreter;
using UnityEngine;

public static class UnityScriptLibrary
{
    public static void Bind(Script L)
    {
        UserData.RegisterType<GameObject>();

        var tGameObject = new Table(L);
        L.Globals["GameObject"] = tGameObject;
        tGameObject["Find"] = new CallbackFunction(GameObjectFind);

        var tDebug = new Table(L);
        L.Globals["Debug"] = tDebug;
        tDebug["Log"] = new CallbackFunction(DebugLog);

        var tResources = new Table(L);
        L.Globals["Resources"] = tResources;
        tResources["ListAll"] = new CallbackFunction(ResourcesListAll);
    }

    #region GameObject

    public static DynValue GameObjectFind(ScriptExecutionContext ctx, CallbackArguments args)
    {
        var name = args.AsStringUsingMeta(ctx, 0, "GameObject.Find");
        return DynValue.FromObject(ctx.OwnerScript, GameObject.Find(name));
    }

    #endregion

    #region Debug

    public static DynValue DebugLog(ScriptExecutionContext ctx, CallbackArguments args)
    {
        var text = args.AsStringUsingMeta(ctx, 0, "Debug.Log");
        if (string.IsNullOrEmpty(text))
        {
            return DynValue.Nil;
        }

        if (text[0] == '[')
        {
            Debug.LogFormat("[Script]{0}", text);
        }
        else
        {
            Debug.LogFormat("[Script] {0}", text);
        }

        return DynValue.Nil;
    }

    #endregion

    #region Resources

    public static DynValue ResourcesListAll(ScriptExecutionContext ctx, CallbackArguments args)
    {
        var path = args.AsStringUsingMeta(ctx, 0, "Resources.ListAll");
        var resources = Resources.LoadAll(path);
        var names = new string[resources.Length];

        for (int i = 0; i < resources.Length; ++i)
        {
            names[i] = resources[i].name;
        }

        return DynValue.FromObject(ctx.OwnerScript, names);
    }

    #endregion
}
