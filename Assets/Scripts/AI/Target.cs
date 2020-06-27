using System;
using UnityEngine;

public class Target : MonoBehaviour, ITargeteable
{
	public float rotationSpeed = 1.0f;
	public GameObject explosionPrefab;
	public float health = 100.0f;
	public Vector3 locationTarget = Vector3.zero;
	public const float maxDisplacement = 20f;
	public LayerMask displacementLayerMask;
	public void Explode()
	{
		if (explosionPrefab != null)
		{
			Instantiate(explosionPrefab, transform.position, Quaternion.identity);
		}

		Destroy(gameObject, 0.1f);
	}

	public const float HEALTH_TOLERANCE = 0.01f;
	public void ApplyDamage(float damageAmount)
	{
		if (Math.Abs(health) < HEALTH_TOLERANCE) return; //already dead
		health -= damageAmount;
		if (health <= 0.0f)
		{
			health = 0.0f;
			Explode();
		}
	}

	public Vector3 GetLocation()
	{
		return transform.position;
	}

	public float GetHealth()
	{
		return health;
	}

	public void ChangeLocation()
	{
		Vector3 displacement = UnityEngine.Random.insideUnitSphere * maxDisplacement;
		RaycastHit hit;
		locationTarget = (Physics.Raycast(
			new Vector3(displacement.x, 10f, displacement.z), Vector3.down, out hit, 30f, displacementLayerMask.value)
			? hit.point
			: displacement);
		locationTarget.y ++;

	}

	public void OnlyOneLeft()
	{
	    //just change location
	    ChangeLocation();
	}
}