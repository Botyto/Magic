﻿using System;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;

public static class ScriptLibrary
{
    public static void Bind(Script L)
    {
        UnityScriptLibrary.Bind(L);
        UIScriptLibrary.Bind(L);
        MagicScriptLibrary.Bind(L);
    }

    public struct BoundClass
    {
        public string name;
        public StandardUserDataDescriptor userData;
        public Table table;

        public object this[object key]
        {
            get
            {
                return table.Get(key).ToObject();
            }
            set
            {
                table.Set(key, DynValue.FromObject(table.OwnerScript, value));
                //userData.AddMember(key as string, ) TODO - bind methods to userdata as well
            }
        }
    }

    public static BoundClass BindClass<T>(Script L)
    {
        var boundClass = new BoundClass();
        boundClass.name = typeof(T).Name;

        boundClass.userData = UserData.RegisterType<T>() as StandardUserDataDescriptor;
        boundClass.table = new Table(L);
        L.Globals[boundClass.name] = boundClass.table;

        return boundClass;
    }

    public static Table BindEnum<T>(Table ownerTable)
    {
        var tEnum = new Table(ownerTable.OwnerScript);
        ownerTable["Shape"] = tEnum;
        foreach (var value in Enum.GetValues(typeof(T)))
        {
            tEnum[value.ToString()] = (int)value;
        }

        return tEnum;
    }
}
