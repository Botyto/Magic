using System.Collections.Generic;
using UnityEngine;

public class UINode : MonoBehaviour
{
    public Dictionary<string, RectTransform> children;

    void Start()
    {
        UpdateChildren();
    }

    void OnTransformChildrenChanged()
    {
        UpdateChildren();
    }

    void UpdateChildren()
    {
        children.Clear();
        AddChildrenRecursive(transform);
    }

    void AddChildrenRecursive(Transform root)
    {
        int n = root.childCount;
        for (int i = 0; i < n; ++i)
        {
            var child = root.GetChild(i) as RectTransform;
            Debug.Assert(child != null, "UINode child is not a RectTransform", child);
            children.Add(child.name, child);
            if (child.GetComponent<UINode>() == null)
            {
                AddChildrenRecursive(child);
            }
        }
    }
}
