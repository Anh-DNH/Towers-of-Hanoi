public class Floor2 : Floor
{
	public static int HighestLevel = 1;

	protected override void OnAwake()
	{
		Height = 58;
		//Log.Info( "Spawned towerdesign2" );
		modelRenderer.Model = Model.Load( @"models\towerdesign2.vmdl" );
		HighestLevel++;
		Level = HighestLevel;

		base.OnAwake();
	}
}
