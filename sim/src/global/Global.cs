using Godot;

public partial class Global : Node
{
	public string ip = "127.0.0.1";
	public ushort port = 12345;

	public ushort CameraResolution = 300;

	public ushort SelectedMission = 0;
	public ushort SelectedYear = 2024;

	public float fog_density = 0.1f;

    public bool RandomYRot = false;
}
