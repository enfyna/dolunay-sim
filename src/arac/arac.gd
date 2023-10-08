class_name Arac extends RigidBody3D

@export var FrontCam : Marker3D
@export var BottomCam : Marker3D

const SP = 100 #SERVO_POWER

func _integrate_forces(_state: PhysicsDirectBodyState3D) -> void:

	var x : int = 0
	var y : int = 0
	var z : int = 0
	var r : int = 0

	x = int(Input.is_action_pressed("ui_left")) - int(Input.is_action_pressed("ui_right")) 
	
	apply_force(global_transform.basis.x * x * SP)

	y = int(Input.is_action_pressed("throttle")) - int(Input.is_action_pressed("brake"))
	
	apply_force(global_transform.basis.y * y * SP)

	z = int(Input.is_action_pressed("ui_up")) - int(Input.is_action_pressed("ui_down"))
	
	apply_force(global_transform.basis.z * z * SP)

	r = int(Input.is_action_pressed("r_left")) - int(Input.is_action_pressed("r_right"))
	
	apply_force(global_transform.basis.x * r * SP / 10, global_transform.basis.z + global_transform.basis.x)