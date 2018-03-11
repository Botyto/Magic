using UnityEngine;
using UnityEngine.Events;

public class SpellEnergyCollisionEvent : UnityEvent<EnergyManifestation, EnergyManifestation, EnergyManifestation.CollisionEventType> { }
public class SpellUnitCollisionEvent : UnityEvent<EnergyManifestation, Unit, EnergyManifestation.CollisionEventType> { }
public class SpellObjectCollisionEvent : UnityEvent<EnergyManifestation, GameObject, EnergyManifestation.CollisionEventType> { }
