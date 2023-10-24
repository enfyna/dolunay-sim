using Godot;

public partial class RedCircle : MeshInstance3D
{
	public override void _Ready()
	{
		RandomNumberGenerator rng = new();
		Transform3D transform = GlobalTransform;

		transform.Origin.X += rng.Randf() * 10;
		transform.Origin.Z += rng.Randf() * 5;

		GlobalTransform = transform;
	}
}
