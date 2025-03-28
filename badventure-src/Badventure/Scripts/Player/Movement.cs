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
    private Label _screenLabel;
    private CanvasLayer _canvasLayer;

    public override void _Ready()
    {
        // Настройка камеры  
        _camera = GetNode<Camera3D>("Camera3D");
        Input.MouseMode = Input.MouseModeEnum.Captured;

        // Отложенная настройка UI  
        CallDeferred(nameof(SetupDebugLabel));
    }

    private void SetupDebugLabel()
    {
        _canvasLayer = new CanvasLayer();
        _canvasLayer.Layer = 100;
        GetTree().Root.AddChild(_canvasLayer);

        _screenLabel = new Label();
        _screenLabel.Position = new Vector2(10, 10);
        _screenLabel.AddThemeFontSizeOverride("font_size", 20);

        _canvasLayer.AddChild(_screenLabel);
    }

    public override void _Input(InputEvent @event)
    {
        bool debugging = false;
        // Поворот камеры мышью  
        if (@event is InputEventMouseMotion mouseMotion)
        {
            _mouseDelta += mouseMotion.Relative * MouseSensitivity;
            _mouseDelta.Y = Mathf.Clamp(_mouseDelta.Y, -89.9f, 89.9f);

            _camera.RotationDegrees = new Vector3(-_mouseDelta.Y, 0, 0);
            RotationDegrees = new Vector3(0, -_mouseDelta.X, 0);
        }
        // Экстренный выход из захвата курсора  
        if (@event.IsActionPressed("ui_cancel") && !debugging)
        {
            Input.MouseMode = Input.MouseModeEnum.Visible;
            debugging = true;
        }
        else if (debugging && @event.IsActionPressed("ui_cancel")) 
        {
            debugging = false;
            Input.MouseMode = Input.MouseModeEnum.Captured;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        // Получаем входные данные для движения  
        Vector2 input = Input.GetVector("move_left", "move_right", "move_forward", "move_back");

        // Вычисляем направление движения  
        Vector3 direction = Transform.Basis * new Vector3(input.X, 0, input.Y);
        direction = direction.Normalized();

        // Устанавливаем горизонтальную скорость  
        _velocity.X = direction.X * Speed;
        _velocity.Z = direction.Z * Speed;

        // Гравитация  
        if (!IsOnFloor())
        {
            _velocity.Y += Gravity * (float)delta;
        }
        else
        {
            _velocity.Y = 0;
        }

        // Прыжок  
        if (Input.IsActionJustPressed("jump") && IsOnFloor())
        {
            _velocity.Y = JumpForce;
        }

        if (!IsOnFloor() && Position.Y < -20)
        {
            Translate(new Vector3(Position.X, Position.Y + 60, Position.Z));
        }
        // Обновляем скорость и перемещаем персонажа  
        Velocity = _velocity;
        MoveAndSlide();

        // Обновляем отладочную информацию  
        UpdatePositionLabel();
    }

    private void UpdatePositionLabel()
    {
        if (_screenLabel != null)
        {
            _screenLabel.Text = $"""  
                Позиция:   
                X: {GlobalPosition.X:F2}  
                Y: {GlobalPosition.Y:F2}  
                Z: {GlobalPosition.Z:F2}  

                Скорость:  
                X: {Velocity.X:F2}  
                Y: {Velocity.Y:F2}  
                Z: {Velocity.Z:F2}  
                """;
        }
    }
}