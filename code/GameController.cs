using Sandbox.Audio;
using Sandbox.Services;
using System;

public sealed class GameController : Component
{
	public enum CtrlMode
	{
		Select,
		Camera,
		Shop
	}
	public CtrlMode ControlMode = CtrlMode.Select;

	readonly int zOff = 128;

	Island selectedIsland = null;
	List<Floor> selectedFloors = new List<Floor>();
	List<Floor> matchedFloors = new List<Floor>();
	List<Floor> matchedFloorsAbove = new List<Floor>();

	Vector3 Begin;
	Vector3 End;

	public long Highest = 1;
	public long Score = 0;

	List<Island> Islands = new List<Island>();
	List<Island> EmptyIslands = new List<Island>();

	float CamAccel = 0;

	protected override void OnAwake()
	{
		FloorCtrl.Restart();

		Random.Shared.Next();

		foreach ( var obj in Scene.Children )
		{
			//Log.Info( obj );
			var island = obj.Components.Get<Island>();
			if (island != null)
				Islands.Add( island );
		}

		foreach ( var island in Islands )
			SpawnFloor( island );

		Score = 0;
	}

	protected override void OnFixedUpdate()
	{
		if ( Input.Pressed( "attack1" ) && ControlMode == CtrlMode.Select )
		{
			var newIsland = GetIsland();

			if ( selectedIsland == null)
				{ selectedIsland = newIsland; }

			if ( selectedIsland != newIsland )
				MoveFloorToIsland( newIsland );
			else if ( newIsland != null )
			{
				if (selectedIsland.Floors.Count == selectedFloors.Count)
					ReturnFloorToPlace();
				else if ( selectedIsland.Floors.Count > 0 )
					AddFloorToList();
			}
		}

		//Clear matched floors
		if (
			matchedFloors.Count > 0 &&
			matchedFloors[0].MovePoint.Count == 0
		)
			{ ClearMatchedFloor(); }


		var vInput = Input.MouseWheel.y;
		if (vInput != 0)
			CamAccel += 7f * vInput;
		else
			CamAccel -= 0.2f * CamAccel;

		//Log.Info(String.Format("{0:0.0}", CamAccel));
		var pos = new Vector3(Transform.Position);
		pos.z = Math.Clamp(CamAccel + pos.z, 0, 1440);
		Transform.Position = pos;

		//Leaderboards.Board
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( selectedFloors.Count != 0 )
		{
			foreach ( var floor in selectedFloors )
			{
				Gizmo.Draw.Color = Color.FromRgba( 0xff7777aa );
				Gizmo.Draw.LineBBox( floor.modelRenderer.Bounds );
				Gizmo.Draw.Color = Color.FromRgba( 0xffffffff );
			}
		}

		if ( Begin != End )
			Gizmo.Draw.Arrow( Begin, End, 24, 16 );


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
		var floor = selectedIsland.Floors[selectedFloors.Count];

		//Add floor to list if there's only one floor left on island
		//or below floor has the same level
		if ( selectedFloors.Count == 0 || selectedFloors[0].Level == floor.Level )
		{
			var sound = Sound.Play( "floor-pick", Transform.World.Position );
			sound.Pitch = 2.0f;
			sound.Volume = 5.0f;

			floor.PlaySound = false;

			selectedFloors.Add( floor );
			var point = new Vector4( floor.FinalPoint, 0.1f );
			point.z += zOff;
			floor.MovePoint = new List<Vector4> { point };
		}
		else if (selectedFloors[0].Level != floor.Level )
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
			PlayMoveSound();

			float height = 0f;
			for (int i = selectedFloors.Count - 1; i >= 0; i-- )
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
				height += selectedFloors[i].Height;

				selectedFloors[i].PlaySound = true;
				selectedFloors[i].MovePoint = vec4;
			}

			PostFloorPlacement( newIsland );
		}

		//Return to pos if the target island's floor is smaller than selected floor
		else if ( newIsland.Floors[0].Level < selectedFloors[0].Level )
			ReturnFloorToPlace();

		//Place floor on a larger floor
		else
		{
			PlayMoveSound();

			float height = 0f;
			for ( int i = selectedFloors.Count - 1; i >= 0; i-- )
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
				height += selectedFloors[i].Height;

				selectedFloors[i].PlaySound = true;
				selectedFloors[i].MovePoint = vec4;
			}

			PostFloorPlacement( newIsland );
		}
	}

	void ReturnFloorToPlace()
	{
		PlayMoveSound();

		foreach (var floor in selectedFloors)
		{
			floor.PlaySound = true;

			var point = new Vector4( floor.FinalPoint, 0.1f );
			point.z -= zOff;
			floor.MovePoint = new List<Vector4> { point };
		}
		selectedIsland = null;
		selectedFloors = new List<Floor>();
	}

	void PostFloorPlacement( Island newIsland )
	{
		Score = Math.Max(0, Score - 1);

		//Exchange floors
		for ( int i = selectedFloors.Count - 1; i >= 0; i-- )
		{
			selectedIsland.Floors.RemoveAt( i );

			selectedFloors[i].Island = newIsland;
			newIsland.Floors.Insert( 0, selectedFloors[i] );
		}

		//Remove newIsland from EmptyIsland
		if ( EmptyIslands.Count != 0 )
		{
			int ii = EmptyIslands.IndexOf( newIsland );
			if ( ii != -1 )
				EmptyIslands.RemoveAt( ii );
		}

		//Check highest
		if ( newIsland.Floors.Count > Highest )
			Highest = newIsland.Floors.Count;

		//Check if there's a match-3 floors
		CheckFloor( newIsland );

		if ( selectedIsland.Floors.Count == 0 )
		{
			EmptyIslands.Add( selectedIsland );
			//Spawn new floor on a random empty island
			if ( EmptyIslands.Count >= Islands.Count - 1 )
				SpawnFloor( EmptyIslands[Random.Shared.Next( 0, EmptyIslands.Count - 1 )] );
		}

		selectedIsland = null;
		selectedFloors = new List<Floor>();
	}

	void SpawnFloor(Island island)
	{
		Score += 5;
		Log.Info( $"Reward player with 5 scores" );

		GameObject gObj = new GameObject();
		gObj.Transform.Position = island.Transform.Position + island.OriginPoint + new Vector3( 0, 0, zOff * 8 );
		Floor floor = Random.Shared.Next( 0, 4 ) switch
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

	void CheckFloor( Island island )
	{
		for ( var i = 0; i < island.Floors.Count; i++ )
		{
			var type = island.Floors[i].GetType();
			var match = 1;
			while (
				i + match < island.Floors.Count &&
				type == island.Floors[i + match].GetType()
			)
			{
				type = island.Floors[i + match].GetType();
				match++;
			}

			if ( match >= 3 )
			{
				matchedFloorsAbove = island.Floors.GetRange( 0, i + match - 1 );

				for ( int j = 0; j < match; j++ )
				{
					matchedFloors.Add( island.Floors[i] );
					island.Floors.RemoveAt( i );
				}
				i--;
			}
		}
	}

	void ClearMatchedFloor()
	{
		Score += matchedFloors.Count * 10;
		Log.Info( $"Reward player with {matchedFloors.Count * 10} scores" );

		float height = 0f;

		var island = matchedFloors[0].Island;
		for ( int i = 0; i < matchedFloors.Count; i++ )
		{
			height += matchedFloors[i].Height;
			matchedFloors[i].GameObject.Destroy();
		}

		matchedFloors = new List<Floor>();

		if ( island.Floors.Count == 0 )
		{
			EmptyIslands.Add( island );
			if ( EmptyIslands.Count >= Islands.Count - 1 )
				SpawnFloor( EmptyIslands[Random.Shared.Next( 0, EmptyIslands.Count - 1 )] );
		}
		else
		{
			foreach ( var floor in matchedFloorsAbove )
			{
				var point = new Vector4( floor.FinalPoint, 0.1f );
				point.z -= height;
				floor.MovePoint = new List<Vector4> { point };
			}
			matchedFloorsAbove = new List<Floor>();
		}
	}

	void PlayMoveSound()
	{
		var sound = Sound.Play( "floor-move", Transform.World.Position );
		sound.Pitch = 1.3f;
		sound.Volume = 5.0f;
	}
}
