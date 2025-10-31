extends Camera3D
var mouse = Vector2()
const DIST = 1000



func _input(event: InputEvent) -> void:
	if event is InputEventMouseMotion:
		mouse = event.position
	if event is InputEventMouseButton:
		if event.pressed == false and event.button_index == MOUSE_BUTTON_LEFT:
			get_mouse_world_pos(mouse)    # get_viewport().get_mouse_position()


func get_mouse_world_pos(pos: Vector2) -> void:
	var space = get_world_3d().direct_space_state
	var start = get_viewport().get_camera_3d().project_ray_origin(pos)
	var end = get_viewport().get_camera_3d().project_position(pos, DIST)
	var params = PhysicsRayQueryParameters3D.new()
	params.from = start
	params.to = end
	
	var result = space.intersect_ray(params)
	if result.is_empty() == false:
		print(result)
