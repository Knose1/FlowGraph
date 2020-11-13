using UnityEngine;

namespace Com.Github.Knose1.Flow.Example.BulletExample
{
	public class BulletOne : Bullet
	{
		[SerializeField] protected float m_acceleration = 1;
		protected override void Update()
		{
			base.Update();
			velocity += initialDirection * m_acceleration; //bullet one will accelerate
		}
	}
}
