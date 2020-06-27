using UnityEngine;
using Unity.Entities;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class TargetSystem : ComponentSystem
{
	public const float minRotationSpeed = 100f;
	public const float maxRotationSpeed = 200f;
	private PlayerPawn _player = null;
	struct Components
	{
		public Target target;
		public Transform transform;

		public Components(Target target, Transform transform)
		{
			this.target = target;
			this.transform = transform;
		}
	};

	protected override void OnCreateManager()
	{
		base.OnCreateManager();
		_player = Object.FindObjectOfType<PlayerPawn>();
	}

	protected override void OnStartRunning()
	{
		base.OnStartRunning();
		var components = GetEntities<Components>();
		
		if (!_player)
		{
			Debug.LogError("Could not find a player in the scene!");
			return;
		}

		for (int i = 0; i < components.Length; i++)
		{
			components[i].target.rotationSpeed = Random.Range(minRotationSpeed, maxRotationSpeed);
			_player.CanDamage(components[i].target);
		}

	}

	//one update for all targets
	protected override void OnUpdate()
	{
		var components = GetEntities<Components>();
		float deltaTime = Time.deltaTime;

		for (int i = 0; i < components.Length; i++)
		{
			if (components[i].target.health > 0.0f)
			{
				components[i].transform.Rotate(0, components[i].target.rotationSpeed * deltaTime, 0);
				if (components[i].target.locationTarget != Vector3.zero)
				{
					components[i].transform.position = Vector3.Lerp(components[i].transform.position,
						components[i].target.locationTarget, deltaTime * 3f);
					if (Vector3.Distance(components[i].transform.position, components[i].target.locationTarget) < 0.2f)
					{
						components[i].target.ChangeLocation();
					}
				}
			}
		}
	}

}
