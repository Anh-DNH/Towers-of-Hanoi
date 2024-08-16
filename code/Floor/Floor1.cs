public class Floor1 : Floor
{
	public static int HighestLevel = 1;

	protected override void OnAwake()
	{
		Log.Info( "Spawned towerdesign1" );
		modelRenderer.Model = Model.Load( @"models\towerdesign1.vmdl" );
		HighestLevel++;
		Level = HighestLevel;

		base.OnAwake();
	}
}
