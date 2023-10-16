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

		ApplyForce(GlobalTransform.Basis.X * x * SP);

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

		this.z = (z - 500) * 2;
        Math.Min(Math.Max(this.z, -1000), 1000);
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

		string dict_to_str = Json.Stringify(dict);
		byte[] bytes = Encoding.ASCII.GetBytes(dict_to_str);

		dict.Clear();

		return bytes;
	}
}
