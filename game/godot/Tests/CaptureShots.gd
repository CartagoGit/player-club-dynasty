extends SceneTree

## Renders the shell and saves a PNG per route, so a change of chrome can be
## reviewed by looking at it instead of by describing it.
##
##   godot --path game/godot --script res://Tests/CaptureShots.gd
##
## Output goes to user://shots/ unless PCDYNASTY_SHOTS_DIR says otherwise.

const MAIN_SCENE := preload("res://Scenes/Main.tscn")


func _initialize() -> void:
	call_deferred("_run")


func _run() -> void:
	var out_dir := OS.get_environment("PCDYNASTY_SHOTS_DIR")
	if out_dir.is_empty():
		out_dir = ProjectSettings.globalize_path("user://shots")
	DirAccess.make_dir_recursive_absolute(out_dir)

	var scene := MAIN_SCENE.instantiate()
	root.add_child(scene)
	await _settle(90)
	await _shot(out_dir, "01-main-menu")

	var options := scene.get_node("MainMenu/MenuFrame/MenuContent/Actions/Options") as Button
	options.emit_signal("pressed")
	await _settle(30)
	await _shot(out_dir, "02-settings")

	var back := _find(scene, "Back")
	if back != null:
		back.emit_signal("pressed")
	await _settle(20)

	var new_career := scene.get_node("MainMenu/MenuFrame/MenuContent/Actions/NewCareer") as Button
	new_career.emit_signal("pressed")
	await _settle(60)
	await _shot(out_dir, "03-career-setup")

	var club_table := _find(scene, "ClubTable")
	var club_rows := _find(club_table, "Rows") if club_table != null else null
	if club_rows != null and club_rows.get_child_count() > 0:
		var choose := _find(club_rows.get_child(0), "Choose") as Button
		choose.emit_signal("pressed")
		await _settle(10)
		(scene.get_node("MainMenu/CareerSetup/SetupContent/SetupActions/StartCareer") as Button).emit_signal("pressed")
		await _settle(90)
		await _shot(out_dir, "04-workspace-overview")
		var squad := scene.get_node("Workspace/Frame/Layout/Sidebar/SidebarMargin/SidebarContent/Squad") as Button
		squad.emit_signal("pressed")
		await _settle(40)
		await _shot(out_dir, "05-workspace-squad")
		var finances := scene.get_node("Workspace/Frame/Layout/Sidebar/SidebarMargin/SidebarContent/Finances") as Button
		finances.emit_signal("pressed")
		await _settle(40)
		await _shot(out_dir, "06-workspace-finances")

	print("capture-shots: wrote PNGs to ", out_dir)
	quit(0)


func _settle(frames: int) -> void:
	for _index in frames:
		await process_frame


func _shot(out_dir: String, name: String) -> void:
	await RenderingServer.frame_post_draw
	var image := get_root().get_texture().get_image()
	var path := out_dir.path_join("%s.png" % name)
	var error := image.save_png(path)
	if error != OK:
		push_error("capture-shots: could not write %s (%d)" % [path, error])
	else:
		print("capture-shots: ", path)


func _find(node: Node, node_name: String) -> Node:
	if node.name == node_name:
		return node
	for child in node.get_children():
		var found := _find(child, node_name)
		if found != null:
			return found
	return null
