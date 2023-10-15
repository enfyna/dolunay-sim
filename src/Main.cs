using Godot;
using Godot.Collections;
using System;
using System.Text;

public partial class Main : Node3D
{
	[Export]
	private Dolunay Arac;

	private SubViewport cam_1;
	private SubViewport cam_2;

	private StreamPeerTcp connection = new();
	private const string ip = "127.0.0.1";
	private const int port = 6060;

	public override void _Ready() {
		cam_1 = Arac.FrontView;
		cam_2 = Arac.BottomView;

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
	private Dictionary<string, string> dict = new();

	private async void SendData(){
		sending_data = true;

		await ToSignal(GetTree(), "process_frame");

		Image cam1 = cam_1.GetTexture().GetImage();
		Image cam2 = cam_2.GetTexture().GetImage();

		byte[] imageData = cam1.SavePngToBuffer();
		byte[] image2Data = cam2.SavePngToBuffer();
		
		string imageDataBase64 = Convert.ToBase64String(imageData);
		string image2DataBase64 = Convert.ToBase64String(image2Data);

		dict.Add("cam_1", imageDataBase64);
		dict.Add("cam_2", image2DataBase64);

		string dict_to_str = Json.Stringify(dict);
		byte[] bytes = Encoding.ASCII.GetBytes(dict_to_str);
		connection.PutData(bytes);

		dict.Clear();
	}

    public override void _Process(double delta)
    {
		connection.Poll();

		if (connection.GetStatus() == StreamPeerTcp.Status.Connected) {
			int byte_count = connection.GetAvailableBytes();
			if (byte_count > 0) {
				GD.Print("Byte Count: ", byte_count);
				string str = connection.GetString(byte_count);
				GD.Print("Received Data: ", str);
				Dictionary data = (Dictionary)Json.ParseString(str);
				if (data is null){
					return;
				}
				if((int)data["state"] == 0){
					int[] inputs = (int[])data["inputs"];
					Arac.HareketEt(inputs[0], inputs[1], inputs[2], inputs[3]);
					sending_data = false;
				}
				GD.Print("Success!");
			}
			else{
				if(!sending_data){
					SendData();
				}
			}
		}
    }

    public override void _ExitTree() {
		connection.DisconnectFromHost();
	}
}