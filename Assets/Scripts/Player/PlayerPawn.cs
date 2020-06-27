using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;


[RequireComponent(typeof(Rigidbody))]
public class PlayerPawn : MonoBehaviour
{
	public Camera backCamera;
	public Transform shootOrigin;
	

	[Header("UI")] 
	public UnityEngine.UI.Text scoreText;

	[Header("Shooting")]
	[Tooltip("Time in seconds between shots")]
	public float recoilTime = 0.1f;
	public LayerMask bulletHitLayer;

	public struct Bullet
	{
		public Transform transform;
		public Renderer renderer;
		public Vector3 origin;
		public Vector3 destiny;
		public float height;
		public float speed;
		public float time;
	};

	[Tooltip("Prefab used to create bullets")]
	public GameObject bulletPrefab;
	
	[HideInInspector]
	public List<Bullet> Bullets { get; private set; } = new List<Bullet>();
	public List<ITargeteable> targets = new List<ITargeteable>();
	private Rigidbody _rigidbody;

	internal void CanDamage(ITargeteable damageable)
	{
		lock (Bullets)
		{
			targets.Add(damageable);
		}
	}

	private float _verticalInput;
	private float _horizontalInput;
	private float _lastShoot;
	private RaycastHit _lastRaycastHit;
	private const float _shootFarDistance = 300f;
	private const float _bulletSphereR2 = 0.5f;
	private const float _targetRadius2 = 2f;
	private const float _bulletDamage = 100f;
	private int _targetHitScore = 1;
	private int _score;
	private Vector3 _camCenter = new Vector3(0.5f, 0.5f, 0f);
	void Awake()
	{
		_rigidbody = GetComponent<Rigidbody>();
		_lastShoot = Time.time;
		_score = 0;
		if (bulletPrefab == null)
		{
			Debug.LogError("No prefab to create bullets!");
			enabled = false;
		}
	}

	void Update ()
	{
		_horizontalInput = Input.GetAxisRaw("Horizontal");
		_verticalInput = Input.GetAxisRaw("Vertical");
		_rigidbody.velocity = transform.forward * _verticalInput * 5f;
		transform.Rotate(0, _horizontalInput * 3f, 0);
		if (Input.GetButton("Fire1") && (Time.time - _lastShoot >= recoilTime))
		{
			StartCoroutine(Shoot());
		}
	}


	void FixedUpdate()
	{
		lock (Bullets)
		{
			float deltaTime = Time.deltaTime;
			
			bool target_damaged = false;
			bool bullet_destiny = false;
			for (int i = Bullets.Count - 1; i >= 0 && !bullet_destiny; i--)
			{
				Bullet bullet = Bullets[i];
				bullet.time += deltaTime / bullet.speed;
				bullet.transform.position = Parabola(bullet.origin, bullet.destiny, bullet.height, bullet.time);
				bullet_destiny = false;
				target_damaged = false;
				for (int j = targets.Count - 1; j >= 0 && !target_damaged; --j)
				{
					if (targets[j].GetHealth() == 0.0f)
					{
						targets.RemoveAt(j);
						continue;
					}

					if (IsInsideSphere(targets[j].GetLocation(), bullet.transform.position, _targetRadius2))
					{
						target_damaged = true;
						targets[j].ApplyDamage(_bulletDamage);
						if (targets[j].GetHealth() == 0.0f)
						{
							targets.RemoveAt(j);
							_score += _targetHitScore;
							scoreText.text = _score.ToString();
							if(targets.Count==1) targets[0].OnlyOneLeft();
						}
					}
				}

				if (!target_damaged && IsInsideSphere(bullet.destiny, bullet.transform.position, _bulletSphereR2))
				{
					bullet_destiny = true;
				}

				if (!bullet_destiny && !target_damaged)
				{
					Bullets[i] = bullet;
				}
				else
				{
					Destroy(bullet.transform.gameObject);
					Bullets.RemoveAt(i);
				}

			}
		}
	}

	static float EvaluateParabola(float x, float h) => -4f * h* x * x + 4f * h* x;

	private Vector3 Parabola(Vector3 start, Vector3 end, float height, float t)
	{
		Vector3 mid = Vector3.Lerp(start, end, t);
		return new Vector3(mid.x, EvaluateParabola(t, height) + Mathf.Lerp(start.y, end.y, t), mid.z);
	}

	private bool IsInsideSphere(Vector3 center, Vector3 position, float radius_doubled)
	{
		float x1 = position.x - center.x;
		float y1 = position.y - center.y;
		float z1 = position.z - center.z;
		float d = (x1 * x1) + (y1 * y1) + (z1 * z1);
		return d < radius_doubled;
	}

	private IEnumerator Shoot()
	{
		_lastShoot = Time.time;
		GameObject bulletGO = Instantiate(bulletPrefab, shootOrigin.position, shootOrigin.rotation);
		Renderer bulletRenderer = bulletGO.GetComponent<Renderer>();
		MaterialPropertyBlock matBlock = new MaterialPropertyBlock();
		bulletRenderer.GetPropertyBlock(matBlock);
		Vector3 shootLocation = transform.position + transform.forward*5f;
		Ray ray = backCamera.ViewportPointToRay(_camCenter);
		if (Physics.Raycast(ray, out _lastRaycastHit, _shootFarDistance, bulletHitLayer.value))
		{
			shootLocation = _lastRaycastHit.point;
		}

		Bullet bullet = new Bullet()
		{
			transform = bulletGO.transform,
			renderer = bulletRenderer,
			origin = shootOrigin.position,
			destiny = shootLocation,
			height = Random.Range(2.3f, 3f),
			speed = Random.Range(1.5f, 2.2f),
			time = 0.0f
		};

		lock (Bullets)
		{
			Bullets.Add(bullet);
		}
		yield return null;
	}

	
}
