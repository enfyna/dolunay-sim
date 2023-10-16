using Godot;
using Godot.Collections;

public partial class Main : Node3D
{
	[Export]
	private Dolunay Arac;

	private StreamPeerTcp connection = new();
	private const string ip = "127.0.0.1";
	private const int port = 6060;

	public override void _Ready() {
		Connect();
	}

	private void Connect(){
		Error err = connection.ConnectToHost(ip, port);
		GD.Print(err);

		connection.Poll();

		if (connection.GetStatus() == StreamPeerTcp.Status.Connected){
			GD.Print("Successfully connected to the server");
		}
		else if(connection.GetStatus() == StreamPeerTcp.Status.Connecting){
			GD.Print("Trying to connect to " + ip + " : " + port);
		}
		else if (connection.GetStatus() == StreamPeerTcp.Status.None || connection.GetStatus() == StreamPeerTcp.Status.Error){
			GD.Print("Error connecting to " + ip + " : " + port);
		}
	}

	private bool sending_data = false;
	private async void SendData(){
		sending_data = true;

		byte[] bytes = await Arac.GetData();

		connection.PutData(bytes);
		
		sending_data = false;
	}

	string buffer = "";
    public override void _Process(double delta)
    {
		connection.Poll();

		if(connection.GetStatus() != StreamPeerTcp.Status.Connected){
			return;
		}
		if(!sending_data){
			SendData();
		}
		int byte_count = connection.GetAvailableBytes();
		if (byte_count == 0) {
			return;
		}
		GD.Print("Byte Count: ", byte_count);

		string str = connection.GetString(byte_count);
		GD.Print("Received Data: ", str);

		buffer += str;
		Dictionary data = (Dictionary)Json.ParseString(buffer);
		if (data is null){
			return;
		}
		buffer = "";
		if((int)data["is_armed"] == 1){
			int[] inputs = (int[])data["inputs"];
			Arac.HareketEt(inputs[0], inputs[1], inputs[2], inputs[3]);
		}
		else{
			Arac.HareketEt();
		}
		GD.Print("Success!");
    }

    public override void _ExitTree() {
		connection.DisconnectFromHost();
	}
}