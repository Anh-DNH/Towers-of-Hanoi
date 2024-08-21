using System;

public class Floor2 : Floor
{
	new public int Type = 1;

	protected override void OnAwake()
	{
		Height = 58;
		//Log.Info( "Spawned towerdesign2" );
		modelRenderer.Model = Model.Load( @"models\towerdesign2.vmdl" );
		FloorCtrl.HighestLevel[Type]++;

		base.OnAwake();
	}
}
