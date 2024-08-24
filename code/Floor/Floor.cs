using System;

public class Floor : Component
{
	[RequireComponent] public ModelRenderer modelRenderer { get; set; }
	[RequireComponent] public ModelCollider modelCollider { get; set; }
	//[RequireComponent] public Rigidbody Rigidbody { get; set; }

	public int Type = 0;
	public Island Island { get; set; }
	public float Level = 1;

	public List<Vector4> MovePoint = new List<Vector4>();

	public float Height = 48;
	public Vector3 FinalPoint;

	public bool PlaySound = false;

	protected override void OnAwake()
	{
		Level = Random.Shared.Next( 1, 7 );
		var size = 1f + ((Level - 1) * 0.3f);
		Height *= size;
		Transform.Scale = new Vector3( size );
		FinalPoint = Transform.Position;
		modelCollider.Model = modelRenderer.Model;
	}

	protected override void OnFixedUpdate()
	{
		if ( MovePoint.Count > 0 )
		{
			var pos = new Vector3( MovePoint[0] );
			FinalPoint = new Vector3( MovePoint.Last() );
			Transform.Position += (pos - Transform.Position) * MovePoint[0].w;

			if ( Vector3.DistanceBetween( Transform.Position, MovePoint[0] ) <= 5f )
			{
				if ( PlaySound && MovePoint.Count == 1 )
				{
					var sound = Sound.Play( "floor-pick", Transform.World.Position );
					sound.Pitch = 2.0f;
					sound.Volume = 5.0f;
				}

				Transform.Position = MovePoint[0];
				MovePoint.RemoveAt( 0 );

				if ( MovePoint.Count == 0 )
					PlaySound = true;
			}
		}
	}
}

public static class FloorCtrl
{
	public static int[] HighestLevel = {
		0, 0, 0, 0
	};
	public static int[] GoalLevel = {
		0, 0, 0, 0
	};

	public static void Restart()
	{
		for (var i = 0; i < HighestLevel.Count(); i++)
		{
			HighestLevel[i] = 1;
			GoalLevel[i] = 3;
		}
	}
}
