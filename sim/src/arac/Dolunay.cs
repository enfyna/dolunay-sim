using Godot;
using Godot.Collections;
using System;
using System.Text;
using System.Threading.Tasks;

public partial class Dolunay : RigidBody3D
{
	[Export]
	public SubViewport FrontView;

	[Export]
	public SubViewport BottomView;

	[Export]
	public RayCast3D DepthSensor;

	[Export]
	public RayCast3D RightDistance;

	[Export]
	public RayCast3D LeftDistance;

	[Export]
	private Marker3D FrontCamPos;

	[Export]
	private Marker3D BottomCamPos;

	[Export]
	private Camera3D FrontCam;

	[Export]
	private Camera3D BottomCam;

	private bool is_armed = false;

	private const float SP = 0.01f;

	private float x = 0;
	private float y = 0;
	private float z = 0;
	private float r = 0;

	private float x_s = 0;
	private float y_s = 0;
	private float z_s = 0;
	private float r_s = 0;

	public override void _Process(double delta) {

		FrontCam.GlobalTransform = FrontCamPos.GlobalTransform;
		BottomCam.GlobalTransform = BottomCamPos.GlobalTransform;

		x_s = Mathf.Lerp(x_s, x, 0.7f);
		y_s = Mathf.Lerp(y_s, y, 0.7f);
		r_s = Mathf.Lerp(r_s, r, 0.7f);
		z_s = Mathf.Lerp(z_s, z, 0.7f);

		ApplyForce(GlobalTransform.Basis.X * y_s);
		ApplyForce(GlobalTransform.Basis.Y * z_s);
		ApplyForce(GlobalTransform.Basis.Z * x_s);

		ApplyForce(GlobalTransform.Basis.X * r_s, GlobalTransform.Basis.Z + GlobalTransform.Basis.X);
	}

	public void HareketEt(int x = 0, int y = 0, int z = 500, int r = 0){
		if(is_armed){
			this.x = Math.Min(Math.Max(x, -1000), 1000) * 25 * SP;
			this.y = Math.Min(Math.Max(-y, -1000), 1000) * 25 * SP;
			this.r = Math.Min(Math.Max(-r, -1000), 1000) * 2 * SP;

			this.z = Math.Min(Math.Max((z - 500) * 2, -1000), 1000) * 5 * SP;
		}
		else{
			this.x = 0;
			this.y = 0;
			this.z = 10; // try to fake vehicle going up when motors are disarmed
			this.r = 0;
		}
	}

	public void SetArm(bool arm){
		this.is_armed = arm;
	}

	private Dictionary<string, string> dict = new();
	public async Task<byte[]> GetData(){
		await ToSignal(GetTree(), "process_frame");

		Image cam1 = FrontView.GetTexture().GetImage();
		Image cam2 = BottomView.GetTexture().GetImage();

		byte[] imageData = cam1.SavePngToBuffer();
		byte[] image2Data = cam2.SavePngToBuffer();

		string imageDataBase64 = Convert.ToBase64String(imageData);
		string image2DataBase64 = Convert.ToBase64String(image2Data);

		dict.Add("cam_1", imageDataBase64);
		dict.Add("cam_2", image2DataBase64);

		Vector3 origin = GlobalTransform.Origin;

		Vector3 right_point = RightDistance.GetCollisionPoint();
		Vector3 left_point = LeftDistance.GetCollisionPoint();
		Vector3 top_point = DepthSensor.GetCollisionPoint();

		double right_distance = origin.DistanceTo(right_point);
		double left_distance = origin.DistanceTo(left_point);
		double depth = origin.DistanceTo(top_point);

		dict.Add("right_distance", Math.Round(right_distance, 5).ToString());
		dict.Add("left_distance", Math.Round(left_distance, 5).ToString());
		dict.Add("depth", Math.Round(depth, 5).ToString());

		dict.Add("pitch", Math.Round(GlobalRotation.X, 5).ToString());
		dict.Add("yaw", Math.Round(GlobalRotation.Y, 5).ToString());
		dict.Add("roll", Math.Round(GlobalRotation.Z, 5).ToString());

		dict.Add("is_armed", is_armed ? "1" : "0");

		string dict_to_str = Json.Stringify(dict);
		byte[] bytes = Encoding.ASCII.GetBytes(dict_to_str);

		dict.Clear();

		return bytes;
	}
}
