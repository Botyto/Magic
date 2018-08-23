using System.Reflection;
using UnityEngine;

public class PropertyBinding : MonoBehaviour
{
    public MonoBehaviour sourceObject;
    public string sourceProperty;
    public MonoBehaviour targetObject;
    public string targetProperty;

    private FieldInfo m_SourceField;
    private FieldInfo m_TargetField;

    private void Start()
    {
        if (sourceObject == null || targetObject == null)
        {
            Destroy(this);
            return;
        }

        m_SourceField = sourceObject.GetType().GetField(sourceProperty);
        m_TargetField = targetObject.GetType().GetField(targetProperty);
    }

    private void Update()
    {
        if (sourceObject == null || targetObject == null)
        {
            Destroy(this);
        }
        else
        {
            m_TargetField.SetValue(targetObject, m_SourceField.GetValue(sourceObject));
        }
	}
}
