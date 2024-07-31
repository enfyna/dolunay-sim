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

	private ushort selectedMission = 1;
	private ushort selectedYear = 2024;

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
        _bind_mission_buttons();

		randomizeYRot.ButtonPressed = Globals.RandomYRot;
		Fog.Value = Globals.fog_density;
		IPinput.Text = Globals.ip + ":" + Globals.port;
		CameraResolution.Text = Globals.CameraResolution.ToString();

		// load mission scene
		missionScene = (Main)ResourceLoader.Load<PackedScene>("res://src/Sim.tscn").Instantiate();

		// hide mission settings
		missionInfo.GetParent<VBoxContainer>().Modulate = Colors.Transparent;
	}

    private void _bind_mission_buttons(){
        TabContainer missionContainer = GetNode<TabContainer>("%TabContainer");
        foreach (Node year in missionContainer.GetChildren()){
            foreach (Button mission in year.GetChildren()){
                mission.Pressed += () => {
                    _on_mission_select(
                        Convert.ToUInt16(year.Name),
                        Convert.ToUInt16(mission.Name)
                    );
                };
            }
        }
        missionContainer.TabChanged += (long tab) => {
            _on_mission_select(
                (ushort)(2023 + tab),
                1 // show the first mission of the selected year on tab change
            );
        };
    }

	private void _on_mission_select(ushort year, ushort mission){
		string[] info = { "", "", "" };
		missionInfo.GetParent<VBoxContainer>().Modulate = Colors.White;

        mission--;

		missionStart.Disabled = mission >= 3;

		selectedMission = mission;
		selectedYear = year;

        if(year == 2023){
            info[0] = "Mission 1\n\nFind the red circle in the pool and land the vehicle on it.\n\n";
            info[1] = "Mission 2\n\nPass inside all the yellow objects without crashing the vehicle.\n\n";
            info[2] = "Mission 3\n\nFind the pinger and crash the vehicle to it.\n(Not implemented)\n\n";
            if(mission == 2) missionStart.Disabled = true; // Not implemented
            missionInfo.Text = info[mission];
        }
        else if(year == 2024){
            info[0] = "Mission 1\n\n";
            info[1] = "Mission 2\n\n";
            info[2] = "Mission 3\n\n";
            missionStart.Disabled = true; // Not implemented
            missionInfo.Text = info[mission];
        }
        else{
            missionStart.Disabled = true;
            missionInfo.GetParent<VBoxContainer>().Modulate = Colors.Transparent;
        }
	}

	private void _on_start_mission_pressed(){

		Global Globals = GetNode<Global>("/root/Global");

		Globals.RandomYRot = randomizeYRot.ButtonPressed;

        string[] addr = IPinput.Text.Split(":");
		Globals.ip = addr[0];
		Globals.port = Convert.ToUInt16(addr[1]);

		Globals.fog_density = (float)Fog.Value;

		ushort res = UInt16.Parse(CameraResolution.Text);
		Globals.CameraResolution = res;

		Globals.SelectedMission = selectedMission;
        Globals.SelectedYear = selectedYear;
		
		Tween tw = CreateTween();
		tw.TweenProperty(this, "modulate", Colors.Black, 0.5);
		tw.TweenCallback(Callable.From(() => {
			GetTree().Root.AddChild(missionScene);
			QueueFree();
		})).SetDelay(0.5f);
	}
}
