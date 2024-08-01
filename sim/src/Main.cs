using Godot;
using Godot.Collections;
using System.Threading.Tasks;

public partial class Main : Node3D
{
	[Export] private Dolunay Arac;
	[Export] private ColorRect fade_effect;

	private TcpServer server = new();
	private StreamPeerTcp connection;
	private string ip = "127.0.0.1";
	private ushort port = 12345;
	private Label connectionInfo;

	public override void _Ready(){
		fade_effect.Modulate = Colors.Black;
		
		Global Globals = GetNode<Global>("/root/Global");

		if(Globals.RandomYRot){
			Arac.randomizeRotationY();		
		}

		ip = Globals.ip;
		port = Globals.port;

		WorldEnvironment we = GetNode<WorldEnvironment>("WorldEnvironment"); 
		we.Environment.FogEnabled = Globals.fog_density > 0;
		we.Environment.FogDensity = Globals.fog_density;

		connectionInfo = GetNode<Label>("%ConnectionInfo");

		Node mp = GetNode<Node>("%Missions");
		for(int i = 0; i < mp.GetChildCount(); i++){
			Node3D elm = mp.GetChild<Node3D>(i);
            string[] m_date = elm.Name.ToString().Split("_");
            ushort m_year = (ushort)m_date[0].ToInt();
            ushort m_id = (ushort)m_date[1].ToInt();

			if(Globals.SelectedYear == m_year && Globals.SelectedMission == m_id - 1){
				elm.Show();
			}
            else{
                elm.Hide();
                elm.QueueFree();
            }
		}
		Tween tw = CreateTween();
		tw.TweenProperty(fade_effect, "modulate", Colors.Transparent, 0.5);
	}

	private int Connect(){
		if(!server.IsListening()){
			server.Listen(port, ip);
			connectionInfo.Text = "Waiting for connection on " + ip + ":" + port;
		}
		if(!server.IsConnectionAvailable()){
			return 1;
		}
		connection = server.TakeConnection();
		server.Stop();

		if (connection.GetStatus() == StreamPeerTcp.Status.Connected){
			connectionInfo.Text = "Successfully connected";
            return 0;
		}
		else if(connection.GetStatus() == StreamPeerTcp.Status.Connecting){
			connectionInfo.Text = "Trying to connect to " + ip + " : " + port;
            return 2;
		}
		else{
			connectionInfo.Text = "Error connecting to " + ip + " : " + port;
            return 3;
		}
	}

	private double time_passed_since_last_msg = 0;
	private const double message_dt = 0.1d;
	private bool sending_data = false;
	private async Task SendData(){
		byte[] bytes = await Arac.GetData();
		connection.PutData(bytes);
	}

	public override async void _Process(double delta)
	{
		if(connection is null || connection.GetStatus() != StreamPeerTcp.Status.Connected){
			Arac.HareketEt();
			Connect();
			return;
		}

		time_passed_since_last_msg += delta;
		if(time_passed_since_last_msg > message_dt){
			time_passed_since_last_msg = 0;
			await SendData();
            RecvData();
		}
	}

	private void RecvData(){
		int byte_count = connection.GetAvailableBytes();
		if (byte_count <= 0) {
			return;
		}
		// GD.Print("Byte Count: ", byte_count);

		string str = connection.GetString(byte_count);
		// GD.Print("Received Data: ", str);

		int startidx = str.IndexOf('{');
		if(startidx == -1) return;
		int endidx = str.IndexOf('}', startidx);
		if(endidx == -1) return;

		str = str.Substring(startidx, endidx - startidx + 1);

		// GD.Print("Split Data: ", str);
		Dictionary data = (Dictionary)Json.ParseString(str);

		if(data.ContainsKey("set_arm")){
			int set_arm = (int)data["set_arm"];
			Arac.SetArm(set_arm == 1);
		}

		if(data.ContainsKey("inputs")){
			int[] inputs = (int[])data["inputs"];
			connectionInfo.Text = inputs.Stringify();

			Arac.HareketEt(inputs[0], inputs[1], inputs[2], inputs[3]);
		}
	}

	public void _on_exit_pressed(){
		Menu menu = (Menu)ResourceLoader.Load<PackedScene>("res://src/menu/menu.tscn").Instantiate();

		GetTree().Root.AddChild(menu);

		QueueFree();
	}

	public void _on_reset_pressed(){
		Main main = (Main)ResourceLoader.Load<PackedScene>("res://src/Sim.tscn").Instantiate();

		GetTree().Root.AddChild(main);

		QueueFree();
	}

	public override void _ExitTree() {
		if(connection != null){
			connection.DisconnectFromHost();
			connection.Dispose();
		}
		server.Stop();
		server.Dispose();
	}
}
