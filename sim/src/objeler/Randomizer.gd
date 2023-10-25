extends Node3D

func _ready() -> void:
	const MIN_DISTANCE_BETWEEN_OBJECTS = 3

	var occupied_positions = []

	for child in get_children():
		var new_position = Vector3()

		while true:
			new_position.x = randi_range(-5, 5)
			new_position.z = randi_range(-5, 5)
			new_position.y = global_position.y

			var is_occupied = false
			for occupied_pos in occupied_positions:
				if new_position.distance_to(occupied_pos) < MIN_DISTANCE_BETWEEN_OBJECTS:
					is_occupied = true
					break

			if not is_occupied:
				break

		occupied_positions.append(new_position)
		child.global_transform.origin = new_position
		child.global_rotation.y = randi_range(0, 180)