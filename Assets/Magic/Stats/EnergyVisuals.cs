using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Energy helpers related to manifestation rendering (meshes, materials, ...)
/// </summary>
public static class EnergyVisuals
{
    #region Public getters

    /// <summary>
    /// Retrieve mesh for visualising a specific manifested shape
    /// </summary>
    public static Mesh FindMesh(Energy.Shape shape)
    {
        Mesh mesh;
        if (m_ShapeToMesh.TryGetValue(shape, out mesh))
        {
            return mesh;
        }

        return null;
    }

    /// <summary>
    /// Retrieve collider mesh for a specific manifested shape
    /// </summary>
    public static Mesh FindCollider(Energy.Shape shape)
    {
        Mesh mesh;
        if (m_ShapeToCollider.TryGetValue(shape, out mesh))
        {
            return mesh;
        }

        return null;
    }

    /// <summary>
    /// Retrieve material for visualising a specific manifested element
    /// </summary>
    public static Material FindMaterial(Energy.Element element)
    {
        Material material;
        if (m_ElementToMaterial.TryGetValue(element, out material))
        {
            return material;
        }

        return null;
    }

    #endregion

    #region Initialization

    private static Mesh FindMesh(string meshPath)
    {
        return Resources.Load<Mesh>(meshPath);
    }

    private static Material FindMaterial(string materialPath)
    {
        return Resources.Load<Material>(materialPath);
    }

    /// <summary>
    /// Initialization
    /// </summary>
    static EnergyVisuals()
    {
        //Shape to visual mesh
        m_ShapeToMesh = new Dictionary<Energy.Shape, Mesh>();
        m_ShapeToMesh[Energy.Shape.Sphere] = FindMesh("Meshes/sphere"); // r = 0.62035
        m_ShapeToMesh[Energy.Shape.Cube] = FindMesh("Meshes/cube"); // a = 1
        m_ShapeToMesh[Energy.Shape.Cone] = FindMesh("Meshes/cone"); // r = 1, h = 1,90986
        m_ShapeToMesh[Energy.Shape.HemiSphere] = FindMesh("Meshes/hemisphere"); // r = 0.781593
        m_ShapeToMesh[Energy.Shape.Capsule] = FindMesh("Meshes/capsule"); // r = 0.45708, h = 0.91416 = 2r

        //Shape to collider mesh
        m_ShapeToCollider = new Dictionary<Energy.Shape, Mesh>();
        m_ShapeToCollider[Energy.Shape.Cone] = FindMesh("Meshes/cone");
        m_ShapeToCollider[Energy.Shape.HemiSphere] = FindMesh("Meshes/hemisphere_collider");

        //Element ot render material
        m_ElementToMaterial = new Dictionary<Energy.Element, Material>();
        m_ElementToMaterial[Energy.Element.Raw] = FindMaterial("Materials/RawEnergy"); //Classical
        m_ElementToMaterial[Energy.Element.Mana] = FindMaterial("Materials/ManaEnergy");
        m_ElementToMaterial[Energy.Element.Fire] = FindMaterial("Materials/FireEnergy"); //Elemental
        m_ElementToMaterial[Energy.Element.Water] = FindMaterial("Materials/WaterEnergy");
        m_ElementToMaterial[Energy.Element.Wind] = FindMaterial("Materials/WindEnergy");
        m_ElementToMaterial[Energy.Element.Dirt] = FindMaterial("Materials/DirtEnergy"); //natural(dirt)
        m_ElementToMaterial[Energy.Element.Wood] = FindMaterial("Materials/WoodEnergy"); //natural(dirt)
        m_ElementToMaterial[Energy.Element.Electricity] = FindMaterial("Materials/ElectricityEnergy");
        m_ElementToMaterial[Energy.Element.Ice] = FindMaterial("Materials/IceEnergy");
        m_ElementToMaterial[Energy.Element.Ritual] = FindMaterial("Materials/RitualEnergy"); //Special
    }

    #endregion

    #region Private members

    /// <summary>
    /// Maps shapes to visual meshes
    /// </summary>
    private static Dictionary<Energy.Shape, Mesh> m_ShapeToMesh;

    /// <summary>
    /// Maps shapes to collider meshes
    /// </summary>
    private static Dictionary<Energy.Shape, Mesh> m_ShapeToCollider;

    /// <summary>
    /// Maps element to render material
    /// </summary>
    private static Dictionary<Energy.Element, Material> m_ElementToMaterial;

    #endregion
}
