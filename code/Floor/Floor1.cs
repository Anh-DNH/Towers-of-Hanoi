using System;

public class Floor1 : Floor
{
	new public int Type = 0;

	// public static int HighestTower = 0;
	// public static int HeightGoal = 0;

	protected override void OnAwake()
	{
		Height = 42;
		//Log.Info( "Spawned towerdesign1" );
		modelRenderer.Model = Model.Load( @"models\towerdesign1.vmdl" );
		FloorCtrl.HighestLevel[Type]++;


		base.OnAwake();
	}
}
