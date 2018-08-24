using UnityEngine;

public static class SpellUtilities
{
    #region Target related

    /// <summary>
    /// Looks for and finds the closest enemy unit near the wizard.
    /// If the wizard is not a unit, the closest unit will be returned.
    /// </summary>
    public static GameObject FindClosestEnemy(Wizard wizard, float maxDistance = float.PositiveInfinity)
    {
        var unit = wizard.GetComponent<Unit>();
        if (unit == null)
        {
            return Util.FindClosestObject<Unit>(wizard.transform.position).gameObject;
        }
        else
        {
            var en = unit.FindClosestEnemy(maxDistance);
            return en != null ? en.gameObject : null;
        }
    }

    /// <summary>
    /// Looks for and finds the closest friendly unit near the wizard.
    /// If the wizard is not a unit, the closest unit will be returned.
    /// </summary>
    public static GameObject FindClosestFriend(Wizard wizard, float maxDistance = float.PositiveInfinity)
    {
        var unit = wizard.GetComponent<Unit>();
        if (unit == null)
        {
            return Util.FindClosestObject<Unit>(wizard.transform.position).gameObject;
        }
        else
        {
            var en = unit.FindClosestFriend(maxDistance);
            return en != null ? en.gameObject : null;
        }
    }

    /// <summary>
    /// Checks if a manifestation can do harm to the wizard.
    /// </summary>
    public static bool IsManifestationHarmful(Wizard wizard, EnergyManifestation manifestation)
    {
        var wizardUnit = wizard.GetComponent<Unit>();
        if (wizardUnit == null) { return false; }

        var owner = manifestation.holder.ResolveOwner();
        if (owner == null) { return true; }

        var ownerUnit = owner.GetComponent<Unit>();
        if (ownerUnit == null) { return true; }

        return ownerUnit.CanAttack(wizardUnit);
    }

    #endregion
}
