using Godot;
using System.Threading.Tasks;

public partial class Main : Node3D
{
    [Export]
    private Dolunay Arac;

    [Export]
    private SubViewport viewport_1;

    [Export]
    private SubViewport viewport_2;

    [Export]
    private bool save_img = false;

    private readonly Camera3D cam_1 = new();
    private readonly Camera3D cam_2 = new();

    public override void _Ready() {

        viewport_1.AddChild(cam_1);
        viewport_2.AddChild(cam_2);
    }

    private int frame = 0;
    public override void _Process(double delta) {

        cam_1.GlobalTransform = Arac.FrontCam.GlobalTransform;
        cam_2.GlobalTransform = Arac.BottomCam.GlobalTransform;

        frame += 1;

        if (save_img) {
            Task.Run(async () => {
                await ToSignal(GetTree(), "process_frame");
                viewport_1.GetTexture().GetImage().SaveJpg($"res://frames/cam_1_{frame}.jpg");
                viewport_2.GetTexture().GetImage().SaveJpg($"res://frames/cam_2_{frame}.jpg");
            });
        }
    }
}
