using MoonSharp.Interpreter;

public static class ScriptLibrary
{
    public static void Bind(Script L)
    {
        UnityScriptLibrary.Bind(L);
    }
}
