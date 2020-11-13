using UnityEngine;

namespace Com.Github.Knose1.Flow.Example.BulletExample
{
	public class BulletTwo : Bullet
	{
		[SerializeField] protected float m_sinusRate = 1;
		[SerializeField] protected float m_sinusHeight = 1;
		[SerializeField] protected float m_speed = 1;
		protected float initialTime = 0;
		private Vector2 directionNormal;

		override protected void OnEnable()
		{
			base.OnEnable();
			initialTime = Time.time;
			directionNormal = Quaternion.AngleAxis(90, Vector3.forward) * initialDirection;
			directionNormal = directionNormal.normalized;
		}

		protected override void Update()
		{
			//bullet two will make a sinus
			float sine = Mathf.Sin((Time.time - initialTime) * m_sinusRate) * m_sinusHeight;
			velocity = sine * directionNormal + initialDirection * m_speed;
			base.Update();
		}
	}
}
