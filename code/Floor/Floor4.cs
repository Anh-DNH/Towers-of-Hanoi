public class Floor4 : Floor
{
	public static int HighestLevel = 1;

	protected override void OnAwake()
	{
		//Log.Info( "Spawned towerdesign4" );
		modelRenderer.Model = Model.Load( @"models\towerdesign4.vmdl" );
		HighestLevel++;
		Level = HighestLevel;

		base.OnAwake();
	}
}
