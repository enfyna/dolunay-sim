using Godot;
using System;

public partial class Menu : Control
{
	private Button missionStart;
	private Label missionInfo;

	private Main missionScene;

	private int selectedMission = 1;

	public override void _Ready()
	{
		missionStart = GetNode<Button>("%StartMission");
		missionStart.Hide();

		missionInfo = GetNode<Label>("%MissionInfo");
		missionInfo.Hide();

		missionScene = (Main)ResourceLoader.Load<PackedScene>("res://src/Sim.tscn").Instantiate();
	}

	public void _on_mission_select(int mission){
		selectedMission = mission;

		switch (mission)
		{
			case 1:
				missionInfo.Text = "Mission 1\n\nFind the red circle in the pool and land the vehicle on it.\n\n";
				missionStart.Disabled = false;
				break;
			case 2:
				missionInfo.Text = "Mission 2\n\nPass inside all the yellow objects without crashing the vehicle.\n(Not implemented yet)\n";
				missionStart.Disabled = true;
				break;
			case 3:
				missionInfo.Text = "Mission 3\n\nFind the pinger and crash the vehicle to it.\n(Not implemented yet)\n\n";
				missionStart.Disabled = true;
				break;
		}

		missionInfo.Show();

		missionStart.Show();
	}

	public void _on_start_mission_pressed(){
		missionScene.SelectedMission = selectedMission;

		GetTree().Root.AddChild(missionScene);

		QueueFree();
	}
}