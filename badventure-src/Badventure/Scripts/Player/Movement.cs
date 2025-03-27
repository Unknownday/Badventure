using Godot;

public partial class Movement : CharacterBody3D
{
	[Export] public float Speed = 5.0f;
	[Export] public float JumpForce = 6.5f;
	[Export] public float MouseSensitivity = 0.2f; 

	private Vector2 _mouseDelta;
	private Camera3D _camera;
	private const float Gravity = -9.8f;
	private Vector3 _velocity = Vector3.Zero;
	private Node3D _player;
	
	public override void _Ready()
	{
		_camera = GetNode<Camera3D>("Camera3D");
 
		Input.MouseMode = Input.MouseModeEnum.Captured;
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseMotion mouseMotion)
		{
			_mouseDelta += mouseMotion.Relative * MouseSensitivity;
			_mouseDelta.Y = Mathf.Clamp(_mouseDelta.Y, -89.9f, 89.9f);

			_camera.RotationDegrees = new Vector3(-_mouseDelta.Y, 0, 0);
			RotationDegrees = new Vector3(0, -_mouseDelta.X, 0);
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		

		Vector2 input = Input.GetVector("move_left", "move_right", "move_forward", "move_back");
		Vector3 direction = Transform.Basis * new Vector3(input.X, 0, input.Y);
		direction = direction.Normalized();
		_velocity.X = direction.X * Speed;
		_velocity.Z = direction.Z * Speed;

		if (!IsOnFloor())
		{
			_velocity.Y += Gravity * (float)delta;
			
			if(GlobalPosition.Y < -10) GlobalPosition = new Vector3(0, 5, 0);
		}
		else
		{
			_velocity.Y = 0;
		}

		if (Input.IsActionJustPressed("jump") && IsOnFloor())
		{
			_velocity.Y = JumpForce;
		}
		
		Velocity = _velocity;
		MoveAndSlide();
	}
}
