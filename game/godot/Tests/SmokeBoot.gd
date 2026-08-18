extends SceneTree

const MAIN_SCENE := preload("res://Scenes/Main.tscn")

var _failed := false

func _initialize() -> void:
	call_deferred("_run")


func _run() -> void:
	var scene := MAIN_SCENE.instantiate()
	root.add_child(scene)

	if not await _wait_until(func() -> bool: return scene.has_node("MainMenuController")):
		_fail("The main menu did not become available.")
		return

	var main_menu := scene.get_node("MainMenu") as Control
	var workspace := scene.get_node("Workspace") as Control
	_expect(main_menu.visible, "The main menu must be the first visible route.")
	_expect(not workspace.visible, "The manager workspace must stay hidden before career selection.")
	_expect(_label_text(scene, "MainMenu/MenuFrame/MenuContent/Eyebrow") == "PC DYNASTY · DIRECCIÓN DEPORTIVA", "The original PC Dynasty identity is missing.")

	var new_career := scene.get_node("MainMenu/MenuFrame/MenuContent/Actions/NewCareer") as Button
	new_career.emit_signal("pressed")
	await process_frame

	var club_list := scene.get_node("MainMenu/CareerSetup/SetupContent/ClubScroll/ClubList") as VBoxContainer
	if not await _wait_until(func() -> bool: return club_list.get_child_count() > 0):
		_fail("The career setup did not render starter-world clubs.")
		return

	var first_club := club_list.get_child(0) as Button
	first_club.emit_signal("pressed")
	await process_frame

	var start_career := scene.get_node("MainMenu/CareerSetup/SetupContent/SetupActions/StartCareer") as Button
	_expect(not start_career.disabled, "Selecting a club must enable career start.")
	start_career.emit_signal("pressed")
	await process_frame

	_expect(not main_menu.visible, "The main menu must close after starting a career.")
	_expect(workspace.visible, "The manager workspace must become visible after starting a career.")
	_expect(scene.has_node("WorkspaceController"), "The wired workspace controller is missing.")

	var tactics := scene.get_node("Workspace/Layout/Sidebar/SidebarMargin/SidebarContent/Tactics") as Button
	_expect(not tactics.disabled, "The tactics route must be enabled.")
	tactics.emit_signal("pressed")
	await process_frame

	var lineup_path := "Workspace/Layout/ContentMargin/ContentScroll/Content/LineupSelection"
	if not await _wait_until(func() -> bool: return scene.has_node(lineup_path)):
		_fail("The persisted lineup selector did not open.")
		return

	var lineup := scene.get_node(lineup_path) as Control
	var candidates := lineup.get_node("Margin/Content/CandidateScroll/CandidateRows") as VBoxContainer
	_expect(candidates.get_child_count() > 0, "The lineup selector must show the active squad.")
	var submit := lineup.get_node("Margin/Content/ActionRow/Submit") as Button
	submit.emit_signal("pressed")
	await process_frame
	var lineup_status := lineup.get_node("Margin/Content/ActionRow/Status") as Label
	_expect(lineup_status.text == "Selección enviada.", "The lineup plan was not accepted and persisted.")

	quit(1 if _failed else 0)


func _wait_until(predicate: Callable, max_frames: int = 300) -> bool:
	for _frame in max_frames:
		if predicate.call():
			return true
		await process_frame
	return false


func _label_text(parent: Node, path: String) -> String:
	var label := parent.get_node(path) as Label
	return label.text


func _expect(condition: bool, message: String) -> void:
	if not condition:
		_failed = true
		push_error(message)


func _fail(message: String) -> void:
	push_error(message)
	quit(1)
