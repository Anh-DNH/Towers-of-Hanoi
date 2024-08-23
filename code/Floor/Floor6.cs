using System;

public class Floor6 : Floor
{
	new public int Type = 3;

	protected override void OnAwake()
	{
		Height = 58;
		//Log.Info( "Spawned towerdesign4" );
		modelRenderer.Model = Model.Load( @"models\towerdesign6.vmdl" );
		FloorCtrl.HighestLevel[Type]++;

		base.OnAwake();
	}
}
