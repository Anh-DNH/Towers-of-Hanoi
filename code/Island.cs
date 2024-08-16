using Sandbox;

public sealed class Island : Component
{
	[Property] public Vector3 OriginPoint { get; set; }
	[Property] MapInstance Map { get; set; }

	[Property] public List<Floor> Floors { get; set; }
	[RequireComponent] BoxCollider BoxCollider {  get; set; }

	protected override void OnAwake()
	{
		foreach(var f in Floors)
			f.Island = this;
	}

	protected override void OnUpdate()
	{
		//var pos = Floors.Count > 0 ? Floors[0].Transform.Position : OriginPoint;

		//Gizmo.Draw.Color = Color.Red;
		//Gizmo.Draw.Arrow( pos, pos + new Vector3( 0, 0, 64 ) );

		//BBox box = BBox.FromPositionAndSize( Transform.Position + BoxCollider.Center, BoxCollider.Scale );
		//Gizmo.Draw.Color = Color.Green;
		//Gizmo.Draw.LineBBox( box );
		//Gizmo.Draw.Color = Color.White;
	}

	//protected override void OnDestroy()
	//{
	//	if (Map.IsValid)
	//		Map.GameObject.Destroy();
	//}
}
