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

	public override void _Process(double delta) {

		FrontCam.GlobalTransform = FrontCamPos.GlobalTransform;
		BottomCam.GlobalTransform = BottomCamPos.GlobalTransform;

		if(is_armed){
			ApplyForce(GlobalTransform.Basis.X * y * SP);

			ApplyForce(GlobalTransform.Basis.Y * z * SP);

			ApplyForce(GlobalTransform.Basis.Z * x * SP);

			ApplyForce(GlobalTransform.Basis.X * r * SP, GlobalTransform.Basis.Z + GlobalTransform.Basis.X);
		}
	}

	public void HareketEt(int x = 0, int y = 0, int z = 500, int r = 0){
        this.x = Math.Min(Math.Max(x, -1000), 1000) * 25;
        this.y = Math.Min(Math.Max(y, -1000), 1000) * 25;
        this.r = Math.Min(Math.Max(r, -1000), 1000) * 2;

		this.z = (z - 500) * 2;
        this.z = Math.Min(Math.Max(this.z, -1000), 1000) * 10;
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
