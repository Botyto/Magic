using MoonSharp.Interpreter;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIValueInspector : Dialog, IDragHandler
{
    public Text title;
    public RectTransform content;
    
    public object value;
    public GameObject unityGameObject { get { return value as GameObject; } }
    public MonoBehaviour unityComponent { get { return value as MonoBehaviour; } }

    void Start()
    {
        SpawnValues();
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.localPosition += new Vector3(eventData.delta.x, eventData.delta.y);
    }

    public void SpawnValues()
    {
        if (unityGameObject != null)
        {
            title.text = "[GameObject] " + unityGameObject.name;

            var transformTypeName = transform.GetType().Name;
            AddLine(transformTypeName, "<i>[MonoBehaviour]</i> " + transformTypeName);

            var comps = unityGameObject.GetComponents<MonoBehaviour>();
            foreach (var comp in comps)
            {
                var compTypeName = comp.GetType().Name;
                AddLine(compTypeName, "<i>[MonoBehaviour]</i> " + compTypeName);
            }
        }
        else if (unityComponent != null)
        {
            title.text = string.Format("[{0}] {1}", unityComponent.GetType().Name, unityComponent.name);
        }
        else if (value is UnityEngine.Object)
        {
            title.text = "[Unity] " + (value as UnityEngine.Object).name;
        }

        var ty = value.GetType();
        
        var members = ty.GetMembers();
        foreach (var member in members)
        {
            if (IgnoreMember(member.Name)) { continue; }

            var memTy = member.MemberType;
            if (memTy == MemberTypes.Property)
            {
                var prop = member as PropertyInfo;
                if (!prop.CanRead) { continue; }
                var name = prop.Name;
                try
                {
                    var val = prop.GetGetMethod().Invoke(value, null);
                    AddLine(name, val);
                }
                catch (Exception) { }
            }

            if (memTy == MemberTypes.Field)
            {
                var field = member as FieldInfo;
                var name = field.Name;
                if (field.IsPrivate) { name = "<i>private</i> " + name; }
                if (field.IsStatic) { name = "<i>static</i> " + name; }
                var val = field.GetValue(value);
                
                AddLine(name, val);
            }
        }

        
    }

    public GameObject AddLine(string id, object value)
    {
        return AddLine(id, id + " = " + ((value != null) ? value.ToString() : "null"));
    }

    public GameObject AddLine(string title, string text)
    {
        var obj = new GameObject(title + "Inspector");

        var txt = obj.AddComponent<Text>();
        txt.text = text;
        txt.font = Font.CreateDynamicFontFromOSFont("Aerial", 14);
        txt.color = Color.black;
        txt.horizontalOverflow = HorizontalWrapMode.Overflow;
        txt.supportRichText = true;

        var rt = obj.GetComponent<RectTransform>() ?? obj.AddComponent<RectTransform>();
        rt.SetParent(content);
        rt.SetAsLastSibling();

        return obj;
    }

    public static UIValueInspector Inspect(object value)
    {
        if (value is DynValue)
        {
            var dynValue = value as DynValue;
            switch (dynValue.Type)
            {
                case DataType.Boolean: value = dynValue.Boolean; break;
                case DataType.ClrFunction:
                case DataType.Function: value = dynValue.Function; break;
                case DataType.Nil: value = null; break;
                case DataType.Number: value = dynValue.Number; break;
                case DataType.String: value = dynValue.String; break;
                case DataType.Table: value = dynValue.Table; break;
                case DataType.UserData: value = dynValue.UserData.Object; break;
                default: value = string.Format("LuaValue({0})", dynValue.Type.ToString()); break;
            }
        }

        var dlg = Spawn<UIValueInspector>();
        dlg.value = value;
        return dlg;
    }

    public static bool IgnoreMember(string name)
    {
        return name == "useGUILayout" ||
            name == "runInEditMode" ||
            name == "isActiveAndEnabled" ||
            name == "hideFlags" ||
            name == "active" ||
            name == "activeInHierarchy" ||
            name == "gameObject" ||
            name == "transform" ||
            name == "isStatic" ||
            name == "scene";
    }
}
