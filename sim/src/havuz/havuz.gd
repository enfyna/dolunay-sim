extends StaticBody3D

@export var water : MeshInstance3D
var mat : Material
var rx : float
var ry : float

func _ready() -> void:
	mat = water.get_active_material(0)
	rx = randf_range(-1, 1)
	ry = randf_range(-1, 1) 

func _process(delta: float) -> void:
	mat.uv1_offset.x += delta ** 2 * rx
	mat.uv1_offset.y += delta ** 2 * ry
