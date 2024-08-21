using System;

public class Floor3 : Floor
{
	new public int Type = 2;

	protected override void OnAwake()
	{
		Height = 48;
		//Log.Info( "Spawned towerdesign3" );
		modelRenderer.Model = Model.Load( @"models\towerdesign3.vmdl" );
		FloorCtrl.HighestLevel[Type]++;

		base.OnAwake();
	}
}
