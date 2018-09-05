using MoonSharp.Interpreter;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Collections;
using System;

public class CategoryAttribute : Attribute
{
    public CategoryAttribute(string category)
    {
        this.category = category;
    }

    public string category;
}

public class HideFromIngameDebuggerAttribute : Attribute
{ }

public class UIValueInspector : Dialog, IDragHandler
{
    public struct InspectedValue
    {
        public string name;
        public string category;
        public bool isExpandable;
        public MemberInfo info;
    }

    public Text title;
    public RectTransform content;
    
    public object value;

    private List<object> m_History = new List<object>();
    
    void Start()
    {
        SetValue(value);
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.localPosition += new Vector3(eventData.delta.x, eventData.delta.y);
    }

    public static UIValueInspector Inspect(object value)
    {
        value = LuaRemapValue(value);

        if (value == null) { return null; }

        var dlg = Spawn<UIValueInspector>();
        dlg.value = value;
        return dlg;
    }

    #region Callbacks

    public void SetValue(object newValue)
    {
        m_History.Add(newValue);
        value = newValue;

        Refresh();
    }

    public void Refresh()
    {
        foreach (Transform child in content)
        {
            Gameplay.Destroy(child.gameObject);
        }

        SpawnValues(value);
    }

    public void Back()
    {
        var n = m_History.Count;
        if (n > 1)
        {
            var previousValue = m_History[n - 2];
            m_History.RemoveAt(n - 1);
            m_History.RemoveAt(n - 2);
            SetValue(previousValue);
        }
    }

    #endregion

    #region Generation
    
    public void SpawnValues(object value)
    {
        if (value == null)
        {
            title.text = "Null";
            return;
        }

        title.text = string.Format("[{0}]", value.GetType().Name);
        UnityAssignTitle(value);

        SpawnCustomValues(value);
        SpawnGeneralValues(value);
    }

    public void SpawnCustomValues(object value)
    {
        if (value.GetType().IsPrimitive || value is string || value is Decimal)
        {
            AddLine("Primitive", SerializeValue(value));
            return;
        }

        if (UnityAddCustomValues(value))
        {
            return;
        }

        if (typeof(IEnumerable).IsAssignableFrom(value.GetType()))
        {
            AddCategory("IEnumerable");
            var enumerable = value as IEnumerable;
            int i = 0;
            foreach (var subValue in enumerable)
            {
                AddObject("Subvalue" + i.ToString(), SerializeValue(subValue), subValue);
                ++i;
            }
            return;
        }
    }

    public void SpawnGeneralValues(object value)
    {
        var ty = value.GetType();
        var inspectedValues = new List<InspectedValue>();

        //Extract members
        var members = ty.GetMembers();
        foreach (var member in members)
        {
            if (IgnoreMember(member)) { continue; }

            var iv = new InspectedValue();
            iv.name = member.Name;
            iv.category = DetermineCategory(member);
            iv.info = member;
            inspectedValues.Add(iv);
        }

        //Sort them
        inspectedValues.Sort(CompareInspectedValues);

        //And generate inspectors for each one
        string category = null;
        foreach (var iv in inspectedValues)
        {
            if (iv.category != category)
            {
                category = iv.category;
                AddCategory(category);
            }

            try
            {
                object displayedValue = null;
                var displayedString = "<b>" + iv.name + "</b>";
                if (iv.info.MemberType == MemberTypes.Field)
                {
                    var field = iv.info as FieldInfo;
                    if (field.IsPrivate) { displayedString = "<i>private</i> " + displayedString; }
                    if (field.IsStatic) { displayedString = "<i>static</i> " + displayedString; }
                    displayedValue = field.GetValue(value);
                }
                else if (iv.info.MemberType == MemberTypes.Property)
                {
                    displayedValue = (iv.info as PropertyInfo).GetGetMethod().Invoke(value, null);
                }

                displayedString += " = " + SerializeValue(displayedValue);
                AddObject(iv.name, displayedString, displayedValue);
            }
            catch (Exception) { }
        }
    }
    
    public GameObject AddCategory(string category)
    {
        var obj = AddLine(category + "Category", "<color=blue>--- " + category + " ---</color>");
        var categoryTxt = obj.GetComponent<Text>();
        categoryTxt.alignment = TextAnchor.MiddleCenter;
        categoryTxt.fontSize = 16;

        return obj;
    }

    public GameObject AddObject(string title, string text, object value)
    {
        var obj = AddLine(title + "Inspector", text);
        if (IsInspectable(value))
        {
            var btn = obj.AddComponent<Button>();
            btn.onClick.AddListener(() => SetValue(value));
        }

        return obj;
    }
    
    public GameObject AddLine(string title, string text)
    {
        var obj = new GameObject(title);

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

    #endregion

    #region Helpers

    public const string generalCategory = "General";

    public static string SerializeValue(object value)
    {
        var str = value.ToString();

        if (str.StartsWith("System."))
        {
            str = str.Substring(("System.").Length);
        }

        return str;
    }

    public static string DetermineCategory(MemberInfo member)
    {
        var explicitCategory = member.GetCustomAttributes(typeof(CategoryAttribute), true) as CategoryAttribute[];
        if (explicitCategory != null && explicitCategory.Length > 0)
        {
            return explicitCategory[0].category;
        }

        var decalringTypeName = member.DeclaringType.Name;
        if (decalringTypeName == "Component" ||
            decalringTypeName == "Object" ||
            decalringTypeName == "MonoBehaviour" ||
            decalringTypeName == "Behaviour" ||
            decalringTypeName == "GameObject")
        {
            return generalCategory;
        }

        return decalringTypeName;
    }

    public static int CompareInspectedValues(InspectedValue a, InspectedValue b)
    {
        var categoryComparison = a.category.CompareTo(b.category);
        var nameComparison = a.name.CompareTo(b.name);

        if (categoryComparison != 0)
        {
            var generalA = (a.category == generalCategory);
            var generalB = (b.category == generalCategory);

            if (generalA && !generalB) { return -1; }
            if (generalB && !generalA) { return 1; }
            if (generalA && generalB) { return nameComparison; }

            return categoryComparison;
        }

        return nameComparison;
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
            name == "scene" ||
            name == "eulerAngles" ||
            name == "right" ||
            name == "up" ||
            name == "forward" ||
            name == "rotation" ||
            name == "localRotation" ||
            name == "worldToLocalMatrix" ||
            name == "localToWorldMatrix" ||
            name == "root" ||
            name == "lossyScale" ||
            name == "hasChanged" ||
            name == "hierarchyCapacity" ||
            name == "hierarchyCount" ||
            name == "ReferenceID";
    }

    public static bool IgnoreMember(MemberInfo member)
    {
        if (IgnoreMember(member.Name))
        {
            return true;
        }

        var memTy = member.MemberType;
        if (memTy == MemberTypes.Property)
        {
            var prop = member as PropertyInfo;
            if (!prop.CanRead) { return false; }
            return false;
        }

        if (memTy == MemberTypes.Field)
        {
            return false;
        }

        return true;
    }

    public static bool IsInspectable(object value)
    {
        return !(value.GetType().IsPrimitive || value is string || value is Decimal);
    }

    #endregion

    #region Lua library

    public void LuaAssign_o1()
    {
        LuaAssign("o1");
    }

    public void LuaAssign(string global)
    {
        var console = FindObjectOfType<ScriptConsole>();
        console.environment.L.Globals[global] = value;
    }

    public static object LuaRemapValue(object value)
    {
        if (value is DynValue)
        {
            var dynValue = value as DynValue;
            switch (dynValue.Type)
            {
                case DataType.Boolean: return dynValue.Boolean;
                case DataType.ClrFunction:
                case DataType.Function: return dynValue.Function;
                case DataType.Nil: return null;
                case DataType.Number: return dynValue.Number;
                case DataType.String: return dynValue.String;
                case DataType.Table: return dynValue.Table;
                case DataType.UserData: return dynValue.UserData.Object;
                default: return string.Format("LuaValue({0})", dynValue.Type.ToString());
            }
        }

        return value;
    }

    #endregion

    #region Unity library

    public bool UnityAddCustomValues(object value)
    {
        var unityGameObject = value as GameObject;
        if (unityGameObject != null)
        {
            AddCategory("GameObject");

            var comps = unityGameObject.GetComponents<Component>();
            foreach (var comp in comps)
            {
                var compTypeName = comp.GetType().Name;
                AddObject(compTypeName, "<i>[Component]</i> " + compTypeName, comp);
            }
            return true;
        }

        return false;
    }

    public void UnityAssignTitle(object value)
    {
        if (value is GameObject)
        {
            title.text = "[GameObject] " + (value as GameObject).name;
        }
        else if (value is Component)
        {
            var unityComponent = value as Component;
            title.text = string.Format("[{0}] {1}", unityComponent.GetType().Name, unityComponent.name);
        }
        else if (value is UnityEngine.Object)
        {
            title.text = "[Unity] " + (value as UnityEngine.Object).name;
        }
    }

    #endregion
}
