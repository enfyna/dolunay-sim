using Godot;

public partial class Global : Node
{
	public string ip = "127.0.0.1";
	public ushort port = 12345;

	public int SelectedMission = 0;

	public bool is_fog_enabled = true;
}