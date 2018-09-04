using MoonSharp.Interpreter;
using UnityEngine;
#if DEBUG
using UnityEditor;
#endif

public static class UnityScriptLibrary
{
    #region Bind

    public static void Bind(Script L)
    {
        ScriptLibrary.BindClass<Object>(L);

        BindUtils(L); //Vector, Quaternion, ..
        BindGameplay(L); //GameObject, Transform, ...

        var tDebug = new Table(L);
        L.Globals["Debug"] = tDebug;
        tDebug["Log"] = new CallbackFunction(DebugLog);

        var tEditor = new Table(L);
        L.Globals["Editor"] = tEditor;
        tEditor["Pause"] = new CallbackFunction(EditorPause);
        tEditor["SelectedObject"] = new CallbackFunction(EditorSelectedObject);
        
        var tScript = new Table(L);
        L.Globals["Script"] = tScript;
        tScript["DoString"] = new CallbackFunction(ScriptDoString);
        tScript["DoFile"] = new CallbackFunction(ScriptDoFile);
        tScript["DoFolder"] = new CallbackFunction(ScriptDoFolder);
        tScript["Reload"] = new CallbackFunction(ScriptReload);

        L.Globals["GetTime"] = new CallbackFunction(GlobalGetTime);
    }

    public static void BindGameplay(Script L)
    {
        var tCamera = ScriptLibrary.BindClass<Camera>(L);
        tCamera["GetMain"] = new CallbackFunction(CameraMain);

        var tGameObject = ScriptLibrary.BindClass<GameObject>(L);
        tGameObject["Find"] = new CallbackFunction(GameObjectFind);
        tGameObject["Destroy"] = new CallbackFunction(GameObjectDestroy);
        L.Globals["GO"] = tGameObject["Find"];

        var tTransform = ScriptLibrary.BindClass<Transform>(L);
        tTransform["ListChildren"] = new CallbackFunction(TransformListChildren);

        ScriptLibrary.BindEnum<ForceMode>(L.Globals);
    }

    public static void BindUtils(Script L)
    {
        var tVector3 = ScriptLibrary.BindClass<Vector3>(L);
        UserData.RegisterExtensionType(typeof(Extensions));
        tVector3["new"] = new CallbackFunction(Vector3New);
        tVector3["one"] = DynValue.FromObject(L, Vector3.one);
        tVector3["zero"] = DynValue.FromObject(L, Vector3.zero);
        tVector3["forward"] = DynValue.FromObject(L, Vector3.forward);
        tVector3["back"] = DynValue.FromObject(L, Vector3.back);
        tVector3["left"] = DynValue.FromObject(L, Vector3.left);
        tVector3["right"] = DynValue.FromObject(L, Vector3.right);
        tVector3["up"] = DynValue.FromObject(L, Vector3.up);
        tVector3["down"] = DynValue.FromObject(L, Vector3.down);

        var tQuaternion = ScriptLibrary.BindClass<Quaternion>(L);
        tQuaternion["Euler"] = new CallbackFunction(QuaternionEuler);
        tQuaternion["identity"] = DynValue.FromObject(L, Quaternion.identity);

        ScriptLibrary.BindEnum<KeyCode>(L.Globals);
        var tInput = ScriptLibrary.BindClass<Input>(L);
        tInput["GetKey"] = new CallbackFunction(InputGetKey);

        var tRandom = ScriptLibrary.BindClass<Random>(L);
        tRandom["new"] = new CallbackFunction(RandomNew);
    }

    #endregion

    #region Utils

    public static DynValue Vector3New(ScriptExecutionContext ctx, CallbackArguments args)
    {
        var x = args.AsType(0, "Vector3.new", DataType.Number, false);
        var y = args.AsType(1, "Vector3.new", DataType.Number, false);
        var z = args.AsType(2, "Vector3.new", DataType.Number, false);

        return DynValue.FromObject(ctx.OwnerScript, new Vector3((float)x.Number, (float)y.Number, (float)z.Number));
    }

    public static DynValue QuaternionEuler(ScriptExecutionContext ctx, CallbackArguments args)
    {
        Vector3 eulerAngles;
        if (args.Count == 1)
        {
            eulerAngles = args.AsUserData<Vector3>(0, "Quaternion.Euler", false);
        }
        else
        {
            var x = args.AsType(0, "Quaternion.Euler", DataType.Number, false);
            var y = args.AsType(1, "Quaternion.Euler", DataType.Number, false);
            var z = args.AsType(2, "Quaternion.Euler", DataType.Number, false);
            eulerAngles = new Vector3((float)x.Number, (float)y.Number, (float)z.Number);
        }

        return DynValue.FromObject(ctx.OwnerScript, Quaternion.Euler(eulerAngles));
    }

    public static DynValue InputGetKey(ScriptExecutionContext ctx, CallbackArguments args)
    {
        var keyCode = args.AsUserData<KeyCode>(0, "Input.GetKey", false);
        return DynValue.FromObject(ctx.OwnerScript, Input.GetKey(keyCode));
    }

    public static DynValue GlobalGetTime(ScriptExecutionContext ctx, CallbackArguments args)
    {
        return DynValue.NewNumber(Time.time);
    }

    public static DynValue RandomNew(ScriptExecutionContext ctx, CallbackArguments args)
    {
        return DynValue.FromObject(ctx.OwnerScript, new Random());
    }

    #endregion

    #region GameObject

    public static DynValue CameraMain(ScriptExecutionContext ctx, CallbackArguments args)
    {
        return DynValue.FromObject(ctx.OwnerScript, Camera.main);
    }

    public static DynValue GameObjectDestroy(ScriptExecutionContext ctx, CallbackArguments args)
    {
        var obj = args.AsUserData<GameObject>(0, "GameObject.Destroy", true);
        if (obj == null)
        {
            return DynValue.Nil;
        }

        float delay = 0.0f;
        string note = null;
        if (args.Count == 3)
        {
            delay = (float)args.AsType(1, "GameObject.Destroy", DataType.Number, false).Number;
            note = args.AsStringUsingMeta(ctx, 2, "GameObject.Destroy");
        }
        else if (args.Count == 2)
        {
            var arg1 = args.RawGet(1, true);
            if (arg1.Type == DataType.String)
            {
                note = arg1.ToPrintString();
            }
            else
            {
                delay = (float)arg1.CastToNumber();
            }
        }

        Gameplay.Destroy(obj, delay, note);

        return DynValue.FromObject(ctx.OwnerScript, obj);
    }

    public static DynValue GameObject__Index(ScriptExecutionContext ctx, CallbackArguments args)
    {
        //TODO bind properly

        var obj = args.AsUserData<GameObject>(0, "GameObject.__index", true);
        if (obj == null)
        {
            return DynValue.Nil;
        }

        var key = args.AsStringUsingMeta(ctx, 1, "GameObject.__index");
        if (key == null)
        {
            return DynValue.Nil;
        }

        var component = obj.GetComponent(key);

        return DynValue.FromObject(ctx.OwnerScript, component);
    }

    public static DynValue GameObjectFind(ScriptExecutionContext ctx, CallbackArguments args)
    {
        var name = args.AsStringUsingMeta(ctx, 0, "GameObject.Find");
        return DynValue.FromObject(ctx.OwnerScript, GameObject.Find(name));
    }

    public static DynValue TransformListChildren(ScriptExecutionContext ctx, CallbackArguments args)
    {
        var transform = args.AsUserData<Transform>(0, "Transform.ListChildren", false);

        var n = transform.childCount;
        var childrenValue = DynValue.NewTable(ctx.OwnerScript);
        var children = childrenValue.Table;
        for (int i = 0; i < n; ++i)
        {
            children[i] = DynValue.FromObject(ctx.OwnerScript, transform.GetChild(i));
        }

        return childrenValue;
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
            MagicLog.LogFormat("[Script]{0}", text);
        }
        else
        {
            MagicLog.LogFormat("[Script] {0}", text);
        }

        return DynValue.Nil;
    }

    public static DynValue EditorPause(ScriptExecutionContext ctx, CallbackArguments args)
    {
#if DEBUG
        if (Application.isEditor)
        {
            var paused = args.AsType(0, "Application.Pause", DataType.Boolean, false).Boolean;
            EditorApplication.isPaused = paused;
        }
#endif

        return DynValue.Nil;
    }

    public static DynValue EditorSelectedObject(ScriptExecutionContext ctx, CallbackArguments args)
    {
#if DEBUG
        if (Application.isEditor)
        {
            var obj = Selection.activeGameObject;
            if (obj != null)
            {
                return DynValue.FromObject(ctx.OwnerScript, obj);
            }
        }
#endif

        return DynValue.Nil;
    }

    public static DynValue ScriptDoString(ScriptExecutionContext ctx, CallbackArguments args)
    {
        var code = args.AsStringUsingMeta(ctx, 0, "Script.DoString");
        var env = ScriptEnvironment.FetchEnvironment(ctx.OwnerScript);
        return DynValue.FromObject(ctx.OwnerScript, env.DoString(code));
    }

    public static DynValue ScriptDoFile(ScriptExecutionContext ctx, CallbackArguments args)
    {
        var path = args.AsStringUsingMeta(ctx, 0, "Script.DoFile");
        var env = ScriptEnvironment.FetchEnvironment(ctx.OwnerScript);
        return DynValue.FromObject(ctx.OwnerScript, env.DoFile(path));
    }

    public static DynValue ScriptDoFolder(ScriptExecutionContext ctx, CallbackArguments args)
    {
        var env = ScriptEnvironment.FetchEnvironment(ctx.OwnerScript);
        var path = args.AsStringUsingMeta(ctx, 0, "Script.DoFile");
        if (args.Count == 2)
        {
            var filter = args.RawGet(1, true);
            if (filter.Type == DataType.Function || filter.Type == DataType.ClrFunction)
            {
                return DynValue.NewNumber(env.DoFolder(path, filter.Function));
            }
        }

        return DynValue.NewNumber(env.DoFolder(path));
    }

    public static DynValue ScriptReload(ScriptExecutionContext ctx, CallbackArguments args)
    {
        var env = ScriptEnvironment.FetchEnvironment(ctx.OwnerScript);
        env.ReloadScripts();
        return DynValue.Nil;
    }

    #endregion
}
