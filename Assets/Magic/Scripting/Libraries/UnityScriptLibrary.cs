using MoonSharp.Interpreter;
using UnityEngine;
using UnityEditor;

public static class UnityScriptLibrary
{
    #region Bind

    public static void Bind(Script L)
    {
        BindUtils(L); //Vector, Quaternion, ..
        BindGameplay(L); //GameObject, Transform, ...

        var tDebug = new Table(L);
        L.Globals["Debug"] = tDebug;
        tDebug["Log"] = new CallbackFunction(DebugLog);

        var tEditor = new Table(L);
        L.Globals["Editor"] = tEditor;
        tEditor["Pause"] = new CallbackFunction(EditorPause);
        tEditor["SelectedObject"] = new CallbackFunction(EditorSelectedObject);

        var tResources = new Table(L);
        L.Globals["Resources"] = tResources;
        tResources["ListAll"] = new CallbackFunction(ResourcesListAll);

        var tScript = new Table(L);
        L.Globals["Script"] = tScript;
        tScript["DoFile"] = new CallbackFunction(ScriptDoFile);
        tScript["DoFolder"] = new CallbackFunction(ScriptDoFolder);
        tScript["Reload"] = new CallbackFunction(ScriptReload);
    }

    public static void BindGameplay(Script L)
    {
        var tCamera = ScriptLibrary.BindClass<Camera>(L);
        tCamera["Main"] = new CallbackFunction(CameraMain);

        var tGameObject = ScriptLibrary.BindClass<GameObject>(L);
        tGameObject["Find"] = new CallbackFunction(GameObjectFind);
        tGameObject["Destroy"] = new CallbackFunction(GameObjectDestroy);
        L.Globals["GO"] = tGameObject["Find"];

        var tTransform = ScriptLibrary.BindClass<Transform>(L);
        tTransform["ListChildren"] = new CallbackFunction(TransformListChildren);
    }

    public static void BindUtils(Script L)
    {
        var tVector3 = ScriptLibrary.BindClass<Vector3>(L);
        tVector3["new"] = new CallbackFunction(Vector3New);
        tVector3["one"] = DynValue.FromObject(L, Vector3.one);
        tVector3["zero"] = DynValue.FromObject(L, Vector3.zero);

        var tQuaternion = ScriptLibrary.BindClass<Quaternion>(L);
        tQuaternion["Euler"] = new CallbackFunction(QuaternionEuler);
        tQuaternion["identity"] = DynValue.FromObject(L, Quaternion.identity);

        ScriptLibrary.BindEnum<KeyCode>(L.Globals);
        var tInput = ScriptLibrary.BindClass<Input>(L);
        tInput["GetKey"] = new CallbackFunction(InputGetKey);
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

        Util.Destroy(obj, delay, note);

        return DynValue.FromObject(ctx.OwnerScript, obj);
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
            Debug.LogFormat("[Script]{0}", text);
        }
        else
        {
            Debug.LogFormat("[Script] {0}", text);
        }

        return DynValue.Nil;
    }

    public static DynValue EditorPause(ScriptExecutionContext ctx, CallbackArguments args)
    {
        if (Application.isEditor)
        {
            var paused = args.AsType(0, "Application.Pause", DataType.Boolean, false).Boolean;
            EditorApplication.isPaused = paused;
        }

        return DynValue.Nil;
    }

    public static DynValue EditorSelectedObject(ScriptExecutionContext ctx, CallbackArguments args)
    {
        if (Application.isEditor)
        {
            var obj = Selection.activeGameObject;
            if (obj != null)
            {
                return DynValue.FromObject(ctx.OwnerScript, obj);
            }
        }

        return DynValue.Nil;
    }

    public static DynValue ScriptDoFile(ScriptExecutionContext ctx, CallbackArguments args)
    {
        var path = args.AsStringUsingMeta(ctx, 0, "Script.DoFile");
        var env = ScriptEnvironment.FetchEnvironment(ctx.OwnerScript);
        return DynValue.FromObject(ctx.OwnerScript, env.DoFile(path));
    }

    public static DynValue ScriptDoFolder(ScriptExecutionContext ctx, CallbackArguments args)
    {
        var path = args.AsStringUsingMeta(ctx, 0, "Script.DoFile");
        var env = ScriptEnvironment.FetchEnvironment(ctx.OwnerScript);
        return DynValue.FromObject(ctx.OwnerScript, env.DoFolder(path));
    }

    public static DynValue ScriptReload(ScriptExecutionContext ctx, CallbackArguments args)
    {
        var env = ScriptEnvironment.FetchEnvironment(ctx.OwnerScript);
        env.ReloadScripts();
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
            var fullPath = AssetDatabase.GetAssetPath(resources[i]);
            var pathWithExtension = fullPath.Substring(17); //17 == #"Assets/Resources/"
            var periodIdx = pathWithExtension.LastIndexOf('.');
            var finalPath = pathWithExtension.Substring(0, periodIdx);
            names[i] = finalPath;
        }

        return DynValue.FromObject(ctx.OwnerScript, names);
    }

    #endregion
}
;