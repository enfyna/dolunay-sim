using Godot;

public partial class Dolunay : RigidBody3D
{
	[Export]
    public Node3D FrontCam;

    [Export]
    public Node3D BottomCam;

	private const short SP = 100; // #SERVO_POWER

	public override void _Process(double delta) {

        float x = Input.IsActionPressed("ui_left") ? 1 : Input.IsActionPressed("ui_right") ? -1 : 0;
		float y = Input.IsActionPressed("throttle") ? 1 : Input.IsActionPressed("brake") ? -1 : 0;
		float z = Input.IsActionPressed("ui_up") ? 1 : Input.IsActionPressed("ui_down") ? -1 : 0;
		float r = Input.IsActionPressed("r_left") ? 1 : Input.IsActionPressed("r_right") ? -1 : 0;
		
		ApplyForce(GlobalTransform.Basis.X * x *SP);

		ApplyForce(GlobalTransform.Basis.Y * y * SP);

		ApplyForce(GlobalTransform.Basis.Z * z * SP);

		ApplyForce(GlobalTransform.Basis.X * r * SP / 10, GlobalTransform.Basis.Z + GlobalTransform.Basis.X);
	}
}
