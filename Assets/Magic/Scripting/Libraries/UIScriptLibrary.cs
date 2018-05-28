using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using MoonSharp.Interpreter;

public static class UIScriptLibrary
{
    #region Bind

    public static void Bind(Script L)
    {
        ScriptLibrary.BindClass<RectTransform>(L);
        var tEventSystem = ScriptLibrary.BindClass<EventSystem>(L);
        tEventSystem["current"] = new CallbackFunction(EventSystemCurrent);
        ScriptLibrary.BindClass<Canvas>(L);
        ScriptLibrary.BindClass<Button>(L);
        ScriptLibrary.BindClass<Text>(L);
        ScriptLibrary.BindClass<Image>(L);
        ScriptLibrary.BindClass<Toggle>(L);
        ScriptLibrary.BindClass<Slider>(L);
        ScriptLibrary.BindClass<InputField>(L);
    }

    public static DynValue EventSystemCurrent(ScriptExecutionContext ctx, CallbackArguments args)
    {
        return DynValue.FromObject(ctx.OwnerScript, EventSystem.current);
    }

    #endregion
}
