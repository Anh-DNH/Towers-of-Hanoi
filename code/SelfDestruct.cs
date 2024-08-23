using Sandbox;

public sealed class SelfDestruct : Component
{
	[Property] public float Timer { get; set; } = 3;
	protected override void OnFixedUpdate()
	{
		Timer -= 1f / 60f;
		if ( Timer <= 0 )
			GameObject.Destroy();
	}
}
