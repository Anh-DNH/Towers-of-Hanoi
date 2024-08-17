using Sandbox;
using Sandbox.VR;
using System;
using System.Drawing;

public sealed class GameController : Component
{
	readonly int zOff = 128;

	Island selectedIsland = null;
	List<Floor> selectedFloor = new List<Floor>();

	Vector3 Begin;
	Vector3 End;

	//int Money = 2;

	long Step = 0; 

	List<Island> EmptyIslands = new List<Island>();

	protected override void OnAwake()
	{
		Floor1.HighestLevel = 
		Floor2.HighestLevel = 
		Floor3.HighestLevel =
		Floor4.HighestLevel = 1;

		Random.Shared.Next();
	}

	protected override void OnUpdate()
	{
		if ( Input.Pressed( "attack1" ) )
		{
			var newIsland = GetIsland();

			if ( selectedIsland == null)
				selectedIsland = newIsland;

			if ( selectedIsland != newIsland )
				MoveFloorToIsland( newIsland );
			else if ( newIsland != null )
			{
				if (selectedIsland.Floors.Count == selectedFloor.Count)
					ReturnFloorToPlace();
				else if ( selectedIsland.Floors.Count > 0 )
					AddFloorToList();
			}
		}

		if ( selectedFloor.Count != 0 )
		{
			foreach( var floor in selectedFloor )
			{
				Gizmo.Draw.Color = Color.FromRgba( 0xff7777aa );
				Gizmo.Draw.LineBBox( floor.modelRenderer.Bounds );
				Gizmo.Draw.Color = Color.FromRgba( 0xffffffff );
			}
		}

		if (Begin != End )
			Gizmo.Draw.Arrow( Begin, End, 24, 16 );

		foreach(var i in EmptyIslands)
		{
			var BoxCollider = i.GameObject.Components.Get<BoxCollider>();
			BBox box = BBox.FromPositionAndSize( i.Transform.Position + BoxCollider.Center, BoxCollider.Scale );
			Gizmo.Draw.Color = Color.Yellow;
			Gizmo.Draw.LineBBox( box );
			Gizmo.Draw.Color = Color.White;
		}
	}
	Island GetIsland()
	{
		var traceResult = Scene.Trace
			.Ray( Scene.Camera.ScreenPixelToRay( Mouse.Position ), 5000 )
			.Run();
		Begin = traceResult.StartPosition;
		End = traceResult.EndPosition;

		if ( traceResult.GameObject == null ) return null;

		Floor floor = traceResult.GameObject.Components.Get<Floor>();
		Island island = traceResult.GameObject.Components.Get<Island>();
		//Log.Info( $"{floor} ; {island}" );
		if ( floor != null ) return floor.Island;

		return island;
	}

	void AddFloorToList()
	{
		//Move the upmost floor up
		var floor = selectedIsland.Floors[selectedFloor.Count];

		//Add floor to list if there's only one floor left on island
		//or below floor has the same level
		if ( selectedFloor.Count == 0 || selectedFloor[0].Level == floor.Level )
		{
			selectedFloor.Add( floor );
			var point = new Vector4( floor.FinalPoint, 0.1f );
			point.z += zOff;
			floor.MovePoint = new List<Vector4> { point };
		}
		else if (selectedFloor[0].Level != floor.Level )
			ReturnFloorToPlace();
	}
	
	void MoveFloorToIsland( Island newIsland )
	{
		//Return to pos if the Tracer traced to nothing
		if ( newIsland == null)
			ReturnFloorToPlace();

		//Place floor on an empty island
		else if ( newIsland.Floors.Count == 0 )
		{
			float height = 0f;
			for (int i = selectedFloor.Count - 1; i >= 0; i-- )
			{
				//Move path
				List<Vector4> vec4 = new List<Vector4>();
				var pos = newIsland.Transform.Position + newIsland.OriginPoint;
				pos.z += height;
				//pos.z += newIsland.Floors[0].Height;

				var point = new Vector4( pos, 0.25f );
				point.z += zOff;
				vec4.Add( point );

				point = new Vector4( pos, 0.1f );
				vec4.Add( point );
				height += selectedFloor[i].Height;

				selectedFloor[i].MovePoint = vec4;
			}

			PostFloorPlacement( newIsland );
		}

		//Return to pos if the target island's floor is smaller than selected floor
		else if ( newIsland.Floors[0].Level < selectedFloor[0].Level )
			ReturnFloorToPlace();

		//Place floor on a larger floor
		else
		{
			float height = 0f;
			for ( int i = selectedFloor.Count - 1; i >= 0; i-- )
			{
				List<Vector4> vec4 = new List<Vector4>();
				var pos = newIsland.Floors[0].FinalPoint;
				pos.z += newIsland.Floors[0].Height;
				pos.z += height;

				var point = new Vector4( pos, 0.25f );
				point.z += zOff;
				vec4.Add( point );

				point = new Vector4( pos, 0.1f );
				vec4.Add( point );
				height += selectedFloor[i].Height;

				selectedFloor[i].MovePoint = vec4;
			}

			PostFloorPlacement( newIsland );
		}
	}

	void ReturnFloorToPlace()
	{
		foreach (var i in selectedFloor)
		{
			var point = new Vector4( i.FinalPoint, 0.1f );
			point.z -= zOff;
			i.MovePoint = new List<Vector4> { point };
		}
		selectedIsland = null;
		selectedFloor = new List<Floor>();
	}

	void PostFloorPlacement( Island newIsland )
	{
		Step++;
		//Log.Info( Step );

		//Exchange floors
		for ( int i = selectedFloor.Count - 1; i >= 0; i-- )
		{
			selectedIsland.Floors.RemoveAt( i );

			selectedFloor[i].Island = newIsland;
			newIsland.Floors.Insert( 0, selectedFloor[i] );
		}
		//Log.Info( $"newIsland.Floors.Count = {newIsland.Floors.Count}" );

		//Remove newIsland from EmptyIsland
		if ( EmptyIslands.Count != 0 )
		{
			int ii = EmptyIslands.IndexOf( newIsland );
			if ( ii != -1 )
				EmptyIslands.RemoveAt( ii );
		}

		//Spawn new floor on a random empty island
		if ( selectedIsland.Floors.Count == 0 )
			EmptyIslands.Add( selectedIsland );
		if ( EmptyIslands.Count >= 2 )
			SpawnFloor( EmptyIslands[Random.Shared.Next( 0, EmptyIslands.Count - 1 )] );

		selectedIsland = null;
		selectedFloor = new List<Floor>();
	}

	public void SpawnFloor(Island island)
	{
		Log.Info( "Should we spawn floor rn?" );

		GameObject gObj = new GameObject();
		gObj.Transform.Position = island.Transform.Position + island.OriginPoint + new Vector3( 0, 0, zOff * 8 );
		Floor floor = Random.Shared.Next( 0, 4 )
		switch
		{
			1 => gObj.Components.Create<Floor2>(),
			2 => gObj.Components.Create<Floor3>(),
			3 => gObj.Components.Create<Floor4>(),
			_ => gObj.Components.Create<Floor1>()
		};
		floor.MovePoint.Add( new Vector4( island.Transform.Position + island.OriginPoint, 0.125f ) );

		island.Floors.Add( floor );
		int i = EmptyIslands.IndexOf( island );
		if ( i != -1 )
			{ EmptyIslands.RemoveAt( i ); }
	}
}
