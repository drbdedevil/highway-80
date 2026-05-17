using Godot;

public class CarObstacle
{
    public Godot.Vector3 World = new Godot.Vector3();
    public Texture2D Texture;
    public float Speed = 0;
    public int CurrentLane;
    public int TargetLane;

    public float LaneChangeProgress = 1f;

    public bool IsChangingLane = false;
}

public class ObstacleRenderData
{
	public CarObstacle Obstacle;
	public Rect2 Rect;
}