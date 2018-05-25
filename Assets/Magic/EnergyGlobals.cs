using System;
using System.Linq;
using UnityEngine;

public class EnergyGlobals : MonoBehaviour
{
    #region Members

    /// <summary>
    /// Element with which all manifestations are initialized
    /// </summary>
    public Energy.Element DefaultElement;

    /// <summary>
    /// Shape in which all manifestations are initialized
    /// </summary>
    public Energy.Shape DefaultShape;

    /// <summary>
    /// List of elements
    /// </summary>
    [HideInInspector]
    [SerializeField]
    private EnergyElement[] m_Elements;

    /// <summary>
    /// List of shapes
    /// </summary>
    [HideInInspector]
    [SerializeField]
    private EnergyShape[] m_Shapes;

    /// <summary>
    /// Singleton instance
    /// </summary>
    public static EnergyGlobals instance { get; private set; }

    #endregion

    private void Awake()
    {
        Debug.AssertFormat(instance == null, "EnergyGlobals must be singleton!");
        instance = this;

        { //Load elements
            var elements = Enum.GetValues(typeof(Energy.Element)).Cast<Energy.Element>().ToArray();
            var n = elements.Length;
            m_Elements = new EnergyElement[n];
            for (int i = 0; i < n; ++i)
            {
                var element = elements[i];
                m_Elements[i] = Resources.Load<EnergyElement>("Elements/" + element);
            }
        }

        { //Load shapes
            var shapes = Enum.GetValues(typeof(Energy.Shape)).Cast<Energy.Shape>().ToArray();
            var n = shapes.Length;
            m_Shapes = new EnergyShape[n];
            for (int i = 0; i < n; ++i)
            {
                var shape = shapes[i];
                m_Shapes[i] = Resources.Load<EnergyShape>("Shapes/" + shape);
            }
        }
    }

    /// <summary>
    /// Returns the definition of the specified energy element
    /// </summary>
    public EnergyElement GetElement(Energy.Element element)
    {
        return m_Elements[(int)element];
    }

    /// <summary>
    /// Returns the defintion of the specified energy shape
    /// </summary>
    public EnergyShape GetShape(Energy.Shape shape)
    {
        return m_Shapes[(int)shape];
    }
}
