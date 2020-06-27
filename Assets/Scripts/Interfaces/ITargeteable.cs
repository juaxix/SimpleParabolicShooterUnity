using UnityEngine;

public interface ITargeteable
{
    void ApplyDamage(float damageAmount);
    Vector3 GetLocation();
    float GetHealth();
    void OnlyOneLeft();
}
