using Godot;
using System;

public partial class TestBall : MeshInstance3D
{
	double x = 0;
	int direction = 1;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready(){
		x = Math.Abs(Transform.Origin.X);

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta){
		Transform3D pos = Transform;
		pos.Origin.X += (float)(delta * direction / 4);
		if(pos.Origin.X > x){
			direction = -1;
		}
		else if(pos.Origin.X < -x){
			direction = 1;
		}
		Transform = pos;
	}
}
