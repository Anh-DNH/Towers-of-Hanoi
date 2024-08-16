using Sandbox;
using Sandbox.VR;
using System;
using System.Drawing;

public sealed class GameController : Component
{
	readonly int zOff = 128;

	Island selectedIsland = null;
	Floor selectedFloor = null;

	Vector3 Begin;
	Vector3 End;


	long Step = 0; 

	List<Island> EmptyIslands = new List<Island>();

	Island GetIsland()
	{
		var traceResult = Scene.Trace
			.Ray( Scene.Camera.ScreenPixelToRay( Mouse.Position ), 5000 )
			.Run();
		Begin = traceResult.StartPosition;
		End = traceResult.EndPosition;

		if (traceResult.GameObject == null) return null;

		Floor floor = traceResult.GameObject.Components.Get<Floor>();
		Island island = traceResult.GameObject.Components.Get<Island>();
		//Log.Info( $"{floor} ; {island}" );
		if ( floor != null ) return floor.Island;

		return island;
	}

	protected override void OnAwake()
	{
		Floor1.HighestLevel = 
		Floor2.HighestLevel = 
		Floor3.HighestLevel =
		Floor4.HighestLevel = 1;
	}

	protected override void OnUpdate()
	{
		if ( Input.Pressed( "attack1" ) )
		{
			if ( selectedFloor != null )
				MoveFloorToIsland( GetIsland() );
			else if ( selectedIsland == null )
			{
				selectedIsland = GetIsland();
				if ( selectedIsland != null && selectedIsland.Floors.Count > 0 )
				{
					//Move the upmost floor up
					selectedFloor = selectedIsland.Floors[0];
					var point = new Vector4( selectedFloor.FinalPoint, 0.1f );
					point.z += zOff;
					selectedFloor.MovePoint = new List<Vector4> { point };
				}
				else
					selectedIsland = null;
			}
		}

		if ( selectedIsland != null && selectedIsland.Floors.Count > 0 )
		{
			Gizmo.Draw.Color = Color.FromRgba( 0xff7777aa );
			Gizmo.Draw.LineBBox( selectedIsland.Floors[0].modelRenderer.Bounds );
			Gizmo.Draw.Color = Color.FromRgba( 0xffffffff );
		}

		if (Begin != End )
			Gizmo.Draw.Arrow( Begin, End, 24, 16 );

		//Log.Info( EmptyIslands.Count );
		foreach(var i in EmptyIslands)
		{
			var BoxCollider = i.GameObject.Components.Get<BoxCollider>();
			BBox box = BBox.FromPositionAndSize( i.Transform.Position + BoxCollider.Center, BoxCollider.Scale );
			Gizmo.Draw.Color = Color.Yellow;
			Gizmo.Draw.LineBBox( box );
			Gizmo.Draw.Color = Color.White;
		}
	}
	
	void MoveFloorToIsland( Island newIsland )
	{
		//Return to pos if the Tracer traced to nothing
		if ( newIsland == null || newIsland == selectedIsland )
			ReturnFloorToPlace();

		//Place floor on an empty island
		else if ( newIsland.Floors.Count == 0 )
		{
			//Move path
			List<Vector4> vec4 = new List<Vector4>();
			var pos = newIsland.Transform.Position + newIsland.OriginPoint;
			var point = new Vector4( pos, 0.25f );
			point.z += zOff;
			vec4.Add( point );

			point = new Vector4( pos, 0.1f );
			vec4.Add( point );

			selectedFloor.MovePoint = vec4;

			PostFloorPlacement( newIsland );
		}

		//Return to pos if the target island's floor is smaller than selected floor
		else if ( newIsland.Floors[0].Level < selectedFloor.Level )
			ReturnFloorToPlace();

		//Place floor on a larger floor
		else
		{
			List<Vector4> vec4 = new List<Vector4>();
			var pos = newIsland.Floors[0].FinalPoint;
			pos.z += newIsland.Floors[0].Height;

			var point = new Vector4( pos, 0.25f );
			point.z += zOff;
			vec4.Add( point );

			point = new Vector4( pos, 0.1f );
			vec4.Add( point );

			selectedFloor.MovePoint = vec4;

			PostFloorPlacement( newIsland );
		}
	}

	void ReturnFloorToPlace()
	{
		//Return the floor to previous place
		if ( selectedIsland.Floors.Count == 1 )
		{
			var point = new Vector4( selectedIsland.Transform.Position + selectedIsland.OriginPoint, 0.1f );
			selectedFloor.MovePoint = new List<Vector4> { point };
		}
		else
		{
			var point = new Vector4( selectedIsland.Floors[1].FinalPoint, 0.1f );
			point.z += selectedIsland.Floors[1].Height;
			selectedFloor.MovePoint = new List<Vector4> { point };
		}
		selectedIsland = null;
		selectedFloor = null;
	}

	void PostFloorPlacement( Island newIsland )
	{
		Step++;
		Log.Info( Step );

		selectedIsland.Floors.RemoveAt( 0 );
		if (selectedIsland.Floors.Count == 0)
			{ EmptyIslands.Add( selectedIsland ); }

		selectedFloor.Island = newIsland;
		newIsland.Floors.Insert( 0, selectedFloor );
		if ( EmptyIslands.Count != 0 )
		{
			int i = EmptyIslands.IndexOf( newIsland );
			if ( i != -1 )
				{ EmptyIslands.RemoveAt( i ); }
		}

		//var emptyIsl = EmptyIsland.Count;
		//var totalIsl = TotalIslands.Count
		//if ( EmptyIsland.Count > Math.Max(TotalIslands.Count, 2) || EmptyIslands.Count < )
		if ( EmptyIslands.Count >= 2 )
		{
			//Log.Info("TIME TO ADD NEW FLOOR");
			SpawnFloor( EmptyIslands[Random.Shared.Next( 0, EmptyIslands.Count - 1 )] );
		}
		//else
		//{
		//	Log.Info("It's not the time to add the new floor.. yet");
		//}

		selectedIsland = null;
		selectedFloor = null;
	}

	public void SpawnFloor(Island island)
	{
		GameObject gObj = new GameObject();
		gObj.Transform.Position = island.Transform.Position + island.OriginPoint + new Vector3( 0, 0, zOff * 8 );
		Floor floor = Random.Shared.Next( 0, 3 ) switch
		{
			0 => gObj.Components.Create<Floor1>(),
			1 => gObj.Components.Create<Floor2>(),
			2 => gObj.Components.Create<Floor3>(),
			3 => gObj.Components.Create<Floor4>()
		};
		floor.MovePoint.Add( new Vector4( island.Transform.Position + island.OriginPoint, 0.125f ) );

		island.Floors.Add( floor );
		int i = EmptyIslands.IndexOf( island );
		if ( i != -1 )
			{ EmptyIslands.RemoveAt( i ); }
	}
}
