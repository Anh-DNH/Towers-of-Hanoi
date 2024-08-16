using Sandbox;
using System;
using static Sandbox.PhysicsContact;

public sealed class Floor : Component
{
	[RequireComponent] public ModelRenderer modelRenderer { get; set; }
	[Property] public Island Island { get; set; }
	[Property] public float Level = 1;

	public static int HighestLevel = 0;

	public List<Vector4> MovePoint = new List<Vector4>();

	public float Height = 48;
	public Vector3 FinalPoint;

	protected override void OnAwake()
	{
		Transform.Scale = new Vector3( Level, Level, Transform.Scale.z );
		FinalPoint = Transform.Position;
	}

	protected override void OnUpdate()
	{
		if ( MovePoint.Count > 0 )
		{
			var pos = new Vector3( MovePoint[0] );
			Gizmo.Draw.Line( Transform.Position, pos );
			FinalPoint = new Vector3( MovePoint.Last() );
			Transform.Position += (pos - Transform.Position) * MovePoint[0].w;

			if ( Vector3.DistanceBetween( Transform.Position, MovePoint[0] ) <= 2f )
			{
				Transform.Position = MovePoint[0];
				MovePoint.RemoveAt( 0 );
			}
		}
	}
}
