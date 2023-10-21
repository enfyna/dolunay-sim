using System;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;

public partial class Main : Node3D
{
	[Export]
	private Dolunay Arac;

	private TcpServer server = new();
	private StreamPeerTcp connection;
	private const string ip = "127.0.0.1";
	private const int port = 12345;

	public override void _Ready(){
		server.Listen(port, ip);
	}

	private int Connect(){
		if(!server.IsListening()){
			server.Listen(port, ip);
		}
		if(!server.IsConnectionAvailable()){
			return 1;
		}
		connection = server.TakeConnection();
		server.Stop();

		if (connection.GetStatus() == StreamPeerTcp.Status.Connected){
			GD.Print("Successfully connected to the server");
		}
		else if(connection.GetStatus() == StreamPeerTcp.Status.Connecting){
			GD.Print("Trying to connect to " + ip + " : " + port);
		}
		else{
			GD.Print("Error connecting to " + ip + " : " + port);
		}
		return 0;
	}

	private bool sending_data = false;
	private async Task SendData(){
		sending_data = true;

		byte[] bytes = await Arac.GetData();

		GD.Print(connection.PutData(bytes));
		
	}
	
    public override async void _Process(double delta)
    {
		if(connection is null){
			Connect();
			return;
		}
		GD.Print(connection.Poll());

		if(connection.GetStatus() == StreamPeerTcp.Status.None){
			Connect();
			return;
		}
		if(!sending_data){
			await SendData();
		}
		int byte_count = connection.GetAvailableBytes();
		if (byte_count <= 0) {
			return;
		}
		GD.Print("Byte Count: ", byte_count);

		string str = connection.GetString(byte_count);
		GD.Print("Received Data: ", str);
		try{
			str = $"{{{str.Split('{')[1].Split('}')[0]}}}";
		}
		catch (Exception){
			return;
		}
		GD.Print("Split Data: ", str);

		Dictionary data = (Dictionary)Json.ParseString(str);
		if (data is null){
			return;
		}
		if((int)data["is_armed"] == 1){
			int[] inputs = (int[])data["inputs"];
			Arac.HareketEt(inputs[0], inputs[1], inputs[2], inputs[3]);
		}
		else{
			Arac.HareketEt();
		}
		GD.Print("Success!");
		sending_data = false;
    }

    public override void _ExitTree() {
		connection.DisconnectFromHost();
		connection.Dispose();
		server.Stop();
		server.Dispose();
	}
}