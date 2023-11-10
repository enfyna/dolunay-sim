using Godot;

public partial class Global : Node
{
	public string ip = "127.0.0.1";
	public ushort port = 12345;

	public int CameraResolution = 300;

	public int SelectedMission = 0;

	public float fog_density = 0.2f;
}