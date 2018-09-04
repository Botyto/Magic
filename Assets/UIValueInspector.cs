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
    public const string generalCategory = "General";

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
    public GameObject unityGameObject { get { return value as GameObject; } }
    public MonoBehaviour unityComponent { get { return value as MonoBehaviour; } }

    private List<object> m_History = new List<object>();

    void Start()
    {
        SetValue(value);
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.localPosition += new Vector3(eventData.delta.x, eventData.delta.y);
    }

    public void SetValue(object newValue)
    {
        m_History.Add(newValue);
        value = newValue;

        foreach (Transform child in content)
        {
            Gameplay.Destroy(child.gameObject);
        }
        SpawnValues();
    }

    public void SpawnValues()
    {
        if (value == null)
        {
            title.text = "Null";
            return;
        }
        
        if (unityGameObject != null)
        {
            title.text = "[GameObject] " + unityGameObject.name;
        }
        else if (unityComponent != null)
        {
            title.text = string.Format("[{0}] {1}", unityComponent.GetType().Name, unityComponent.name);
        }
        else if (value is UnityEngine.Object)
        {
            title.text = "[Unity] " + (value as UnityEngine.Object).name;
        }
        else
        {
            title.text = string.Format("[{0}]", value.GetType().Name);
        }

        SpawnCustomValues();
        SpawnGeneralValues();
    }

    public void SpawnCustomValues()
    {
        if (unityGameObject != null)
        {
            var transformTypeName = transform.GetType().Name;
            var obj = AddLine(transformTypeName, "<i>[MonoBehaviour]</i> " + transformTypeName);
            var btn = obj.AddComponent<Button>();
            btn.onClick.AddListener(() => SetValue(transform));

            var comps = unityGameObject.GetComponents<MonoBehaviour>();
            foreach (var comp in comps)
            {
                var compTypeName = comp.GetType().Name;
                obj = AddLine(compTypeName, "<i>[MonoBehaviour]</i> " + compTypeName);
                btn = obj.AddComponent<Button>();
                btn.onClick.AddListener(() => SetValue(comp));
            }
        }

        if (typeof(IEnumerable).IsAssignableFrom(value.GetType()))
        {
            var enumerable = value as IEnumerable;
            foreach (var subValue in enumerable)
            {
                var obj = AddLine(subValue.GetType().Name, subValue.ToString());
                var btn = obj.AddComponent<Button>();
                btn.onClick.AddListener(() => SetValue(subValue));
            }
        }
    }

    public void SpawnGeneralValues()
    {
        var ty = value.GetType();
        var inspectedValues = new List<InspectedValue>();

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

        inspectedValues.Sort(CompareInspectedValues);

        string category = null;
        foreach (var iv in inspectedValues)
        {
            if (iv.category != category)
            {
                category = iv.category;
                var categoryObj = AddLine(category, "<color=blue>--- " + category + " ---</color>");
                var categoryTxt = categoryObj.GetComponent<Text>();
                categoryTxt.alignment = TextAnchor.MiddleCenter;
                categoryTxt.fontSize = 16;
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

                displayedString += " = " + displayedValue.ToString();

                var obj = AddLine(iv.name, displayedString);
                var btn = obj.AddComponent<Button>();
                btn.onClick.AddListener(() => SetValue(displayedValue));
            }
            catch (Exception) { }
        }
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

        if (value == null) { return null; }

        var dlg = Spawn<UIValueInspector>();
        dlg.value = value;
        return dlg;
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
            name == "scene";
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
}
