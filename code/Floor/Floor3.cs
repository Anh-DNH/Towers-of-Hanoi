public class Floor3 : Floor
{
	public static int HighestLevel = 1;

	protected override void OnAwake()
	{
		//Log.Info( "Spawned towerdesign3" );
		modelRenderer.Model = Model.Load( @"models\towerdesign3.vmdl" );
		HighestLevel++;
		Level = HighestLevel;

		base.OnAwake();
	}
}
