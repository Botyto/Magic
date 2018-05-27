using UnityEngine;
using UnityEngine.UI;
using MoonSharp.Interpreter;

public static class UIScriptLibrary
{
    #region Bind

    public static void Bind(Script L)
    {
        ScriptLibrary.BindClass<RectTransform>(L);
        ScriptLibrary.BindClass<Canvas>(L);
        ScriptLibrary.BindClass<Button>(L);
        ScriptLibrary.BindClass<Text>(L);
        ScriptLibrary.BindClass<Image>(L);
        ScriptLibrary.BindClass<Toggle>(L);
        ScriptLibrary.BindClass<Slider>(L);
        ScriptLibrary.BindClass<InputField>(L);
    }

    #endregion
}
