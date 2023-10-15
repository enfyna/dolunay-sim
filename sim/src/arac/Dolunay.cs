using Godot;
using System;

public partial class Dolunay : RigidBody3D
{
	[Export]
    public SubViewport FrontView;

    [Export]
    public SubViewport BottomView;

	[Export]
    private Marker3D FrontCamPos;

    [Export]
    private Marker3D BottomCamPos;

	[Export]
    private Camera3D FrontCam;

    [Export]
    private Camera3D BottomCam;

	private const float SP = 0.01f;

	public float x = 0;
	public float y = 0;
	public float z = 0;
	public float r = 0;

	public override void _Process(double delta) {
		FrontCam.GlobalTransform = FrontCamPos.GlobalTransform;
		BottomCam.GlobalTransform = BottomCamPos.GlobalTransform;

		ApplyForce(GlobalTransform.Basis.X * x *SP);

		ApplyForce(GlobalTransform.Basis.Y * y * SP);

		ApplyForce(GlobalTransform.Basis.Z * z * SP);

		ApplyForce(GlobalTransform.Basis.X * r * SP, GlobalTransform.Basis.Z + GlobalTransform.Basis.X);
	}

	public void HareketEt(int x = 0, int y = 0, int z = 500, int r = 0){
		this.x = x;
		this.y = y;
		this.r = r;

        Math.Min(Math.Max(this.x, -1000), 1000);
        Math.Min(Math.Max(this.y, -1000), 1000);
        Math.Min(Math.Max(this.r, -1000), 1000);

		this.z = z - 500;
        Math.Min(Math.Max(this.z, -500), 500);
	}
}
