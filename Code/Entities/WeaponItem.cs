using System;
using Sandbox.Enums;
using Sandbox.Utilities;

public abstract class WeaponItem : InventoryItem
{
	[Property] public GameObject ViewModelPrefab { get; set; }
	[Property] public GameObject WorldModelPrefab { get; set; }
	[Property] public HoldTypeEnum HoldType { get; set; }
	
	[Property] protected Vector3 _viewModelLocalPosition { get; set; }

	[Sync] protected GameObject _viewModel { get; set; }
	[Sync] protected GameObject _worldModel { get; set; }
	[Sync] public SkinnedModelRenderer ViewModelRenderer { get; set; }
	[Sync] public SkinnedModelRenderer WorldModelRenderer { get; set; }
	[Sync] protected SkinnedModelRenderer _playerModelRenderer { get; set; }

	public void SpawnViewModel()
	{
		if ( ViewModelPrefab == null ) return;

		// ViewModel needs to have the logic to shoot, reload, etc.
		_viewModel = ViewModelPrefab.Clone( new CloneConfig
		{
			StartEnabled = true,
			Parent = Game.ActiveScene.Camera.GameObject,
			Transform = Game.ActiveScene.Camera.WorldTransform
		} );

		_viewModel.LocalPosition = new Vector3( 20, 0, 0 );
		_viewModel.LocalRotation = Rotation.Identity;

		ViewModelRenderer = _viewModel.GetComponent<SkinnedModelRenderer>();
		ViewModelRenderer.RenderType = ModelRenderer.ShadowRenderType.Off;
	}

	public void DestroyViewModel()
	{
		_viewModel?.Destroy();
	}

	public void SpawnWorldModel( GameObject playerBody )
	{
		var holdBone = playerBody.Children.Where( go => go.Name == "hold_R" ).FirstOrDefault();

		if ( WorldModelPrefab == null || playerBody == null ) return;

		_worldModel = WorldModelPrefab.Clone( new CloneConfig
		{
			StartEnabled = true,
			Parent = holdBone,
			Transform = holdBone.WorldTransform
		} );

		WorldModelRenderer = _worldModel.GetComponent<SkinnedModelRenderer>();
		_playerModelRenderer = playerBody.GetComponent<SkinnedModelRenderer>();


		_worldModel.LocalPosition = new Vector3( -19, 11, 9 ); // TODO: put in property
		_worldModel.LocalRotation = Rotation.Identity;
		_worldModel.NetworkSpawn();
		
		ServerAnimationHelper.BroadcastAnimationSet( WorldModelRenderer, "b_deploy_skip", true );
		ServerAnimationHelper.BroadcastAnimationSet( _playerModelRenderer, "holdtype", (int)HoldType );
	}

	public void DestroyWorldModel()
	{
		GameObjectHelper.NetworkDestroy( _worldModel );
		ServerAnimationHelper.BroadcastAnimationSet( _playerModelRenderer, "holdtype", (int)HoldTypeEnum.None );
	}

	public abstract void Attack();
}
