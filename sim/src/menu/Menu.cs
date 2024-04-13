using Godot;
using System;

public partial class Menu : Control
{
	private Button missionStart;
	private Label missionInfo;
	private LineEdit IPinput;
	private LineEdit CameraResolution;
	private HSlider Fog;
    private CheckBox randomizeYRot;

	private Main missionScene;

	private int selectedMission = 1;

	public override void _Ready()
	{
        // load globals
        Global Globals = GetNode<Global>("/root/Global");

        // init nodes
		missionStart = GetNode<Button>("%StartMission");
		missionInfo = GetNode<Label>("%MissionInfo");
        randomizeYRot = GetNode<CheckBox>("%RandomY");
        Fog = GetNode<HSlider>("%Fog");
        IPinput = GetNode<LineEdit>("%IP");
        CameraResolution = GetNode<LineEdit>("%CameraResolution");

        randomizeYRot.ButtonPressed = Globals.RandomYRot;
        Fog.Value = Globals.fog_density;
        IPinput.Text = Globals.ip + ":" + Globals.port;
        CameraResolution.Text = Globals.CameraResolution.ToString();

        // load mission scene
        missionScene = (Main)ResourceLoader.Load<PackedScene>("res://src/Sim.tscn").Instantiate();

        // hide mission settings
		missionInfo.GetParent<VBoxContainer>().Modulate = Colors.Transparent;
	}

	public void _on_mission_select(int mission){
		selectedMission = mission;

        string[] info = {
            // 2023
            "Mission 1\n\nFind the red circle in the pool and land the vehicle on it.\n\n",
            "Mission 2\n\nPass inside all the yellow objects without crashing the vehicle.\n\n",
            "Mission 3\n\nFind the pinger and crash the vehicle to it.\n(Not implemented)\n\n",
            // 2024
            "Mission 1\n\n",
            "Mission 2\n\n",
            "Mission 3\n\n",
        };

        missionInfo.Text = info[mission - 1];
        missionStart.Disabled = mission >= 3;

		missionInfo.GetParent<VBoxContainer>().Modulate = Colors.White;
	}

	public void _on_start_mission_pressed(){

		Global Globals = GetNode<Global>("/root/Global");

        Globals.RandomYRot = randomizeYRot.ButtonPressed;

		Globals.ip = IPinput.Text.Split(":")[0];
		Globals.port = Convert.ToUInt16(IPinput.Text.Split(":")[1]);

		Globals.fog_density = (float)Fog.Value;

		int res = Int16.Parse(CameraResolution.Text);
		Globals.CameraResolution = res;

		Globals.SelectedMission = selectedMission;
		
		Tween tw = CreateTween();
		tw.TweenProperty(this, "modulate", Colors.Black, 0.5);
		tw.TweenCallback(Callable.From(() => {
			GetTree().Root.AddChild(missionScene);
			QueueFree();
		})).SetDelay(0.5f);
	}
}
