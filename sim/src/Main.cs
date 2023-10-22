using System;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;

public partial class Main : Node3D
{
	[Export]
	private Dolunay Arac;

	public int SelectedMission;
	private Label connectionInfo;

	private TcpServer server = new();
	private StreamPeerTcp connection;
	private const string ip = "127.0.0.1";
	private const int port = 12345;

	public override void _Ready(){
		connectionInfo = GetNode<Label>("%ConnectionInfo");

		switch (SelectedMission)
		{
			case 1:
				Node3D redCircle = GetNode<Node3D>("%RedCircle");
				redCircle.Show();
				break;
			case 2:
				break;
			case 3:
				break;
		}
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
		}
		else if(connection.GetStatus() == StreamPeerTcp.Status.Connecting){
			connectionInfo.Text = "Trying to connect to " + ip + " : " + port;
		}
		else{
			connectionInfo.Text = "Error connecting to " + ip + " : " + port;
		}
		return 0;
	}

	private bool sending_data = false;
	private async Task SendData(){
		sending_data = true;

		byte[] bytes = await Arac.GetData();

		connection.PutData(bytes);
	}
	
    public override async void _Process(double delta)
    {
		if(connection is null || connection.GetStatus() == StreamPeerTcp.Status.None){
			Connect();
			return;
		}

		if(!sending_data){
			await SendData();
		}
		
		Dictionary data = RecvData();

		if(data != null && (int)data["is_armed"] == 1){
			int[] inputs = (int[])data["inputs"];

			connectionInfo.Text = inputs.Stringify();

			Arac.HareketEt(inputs[0], inputs[1], inputs[2], inputs[3]);
		}
		else{
			Arac.HareketEt();
		}
		// GD.Print("Success!");
		sending_data = false;
    }

	private Dictionary RecvData(){
		int byte_count = connection.GetAvailableBytes();
		if (byte_count <= 0) {
			return null;
		}
		// GD.Print("Byte Count: ", byte_count);

		string str = connection.GetString(byte_count);
		// GD.Print("Received Data: ", str);
		try{
			str = $"{{{str.Split('{')[1].Split('}')[0]}}}";
		}
		catch (Exception){
			return null;
		}
		// GD.Print("Split Data: ", str);
		return (Dictionary)Json.ParseString(str);
	}

	public void _on_exit_pressed(){
		Menu menu = (Menu)ResourceLoader.Load<PackedScene>("res://src/menu/menu.tscn").Instantiate();

		GetTree().Root.AddChild(menu);

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