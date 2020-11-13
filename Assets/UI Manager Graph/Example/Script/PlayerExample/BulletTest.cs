using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Github.Knose1.Flow.Example.BulletExample
{

	public class BulletTest : BulletTestAbstract
	{
		[SerializeField] protected BulletOne m_bulletOnePrefab;
		[SerializeField] protected BulletTwo m_bulletTwoPrefab;

		// Start is called before the first frame update
		override protected void SetupMachine()
		{
			base.SetupMachine();

			AllowTrigger("Time");
			for (int i = 2; i < 100; i++)
			{
				AllowTrigger("Time" + i);
			}
		}

		protected T CreateBullet<T>(T prefab, bool active = true) where T : Bullet => CreateBullet(prefab, UnityEngine.Random.insideUnitCircle, active);
			
		protected T CreateBullet<T>(T prefab, Vector2 direction, bool active = true) where T : Bullet
		{
			T bll = Instantiate(prefab);

			bll.initialDirection = direction;
			if (active) bll.gameObject.SetActive(true);
			return bll;
		}


		public BulletOne CreateOne()
		{
			return CreateBullet(m_bulletOnePrefab);
		}

		public BulletTwo CreateTwo()
		{
			Vector2 dir = UnityEngine.Random.insideUnitCircle;
			int index = 0;
			List<BulletTwo> bullets = new List<BulletTwo>();
			for (int i = 0; i < 10; i++)
			{
				bullets.Add(CreateBullet(m_bulletTwoPrefab, dir, false));
			}


			Coroutine cor = null;
			void LActivate()
			{
				if (bullets.Count == index) return;

				if (cor != null) StopCoroutine(cor);

				bullets[index].gameObject.SetActive(true);

				cor = StartCoroutine(routine: LRoutine());
				index++;
			}
			IEnumerator LRoutine()
			{
				yield return new WaitForSeconds(0.1f);
				LActivate();
			}


			LActivate();
			return bullets[0];
		}

	}

	public class Bullet : MonoBehaviour
	{
		private float elapsedTime = 0;
		public float m_lifetime;
		[System.NonSerialized] public Vector2 initialDirection;
		[System.NonSerialized] public Vector2 velocity;

		protected virtual void Start()
		{
			//OnEnable();
		}

		protected virtual void OnEnable()
		{
			initialDirection = initialDirection.normalized;
			velocity = initialDirection;
		}

		protected virtual void Update()
		{
			Vector3 pos = transform.position;

			pos.x += velocity.x * Time.deltaTime;
			pos.y += velocity.y * Time.deltaTime;
			pos.z = 0;

			transform.position = pos;
		}

		protected virtual void LateUpdate()
		{
			elapsedTime += Time.deltaTime;

			if (elapsedTime > m_lifetime)
			{
				Destroy(gameObject);
			}
		}
	}
}
