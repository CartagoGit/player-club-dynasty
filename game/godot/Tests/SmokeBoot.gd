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

	var finances := scene.get_node("Workspace/Layout/Sidebar/SidebarMargin/SidebarContent/Finances") as Button
	_expect(not finances.disabled, "The finance office must be enabled.")
	finances.emit_signal("pressed")
	await process_frame
	var cash_path := "Workspace/Layout/ContentMargin/ContentScroll/Content/FinancesOffice/FinanceMetrics/Cash/Content/Value"
	if not await _wait_until(func() -> bool: return scene.has_node(cash_path)):
		_fail("The persistent finance office did not open.")
		return
	var opening_cash := _label_text(scene, cash_path)
	_expect(opening_cash.contains("EUR"), "The finance office must render meaningful currency values.")

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

	var main_menu_button := scene.get_node("Workspace/Layout/Sidebar/SidebarMargin/SidebarContent/MainMenu") as Button
	main_menu_button.emit_signal("pressed")
	if not await _wait_until(func() -> bool: return main_menu.visible and not workspace.visible and not scene.has_node("WorkspaceController")):
		_fail("Returning from a career did not restore the main menu cleanly.")
		return

	var continue_career := scene.get_node("MainMenu/MenuFrame/MenuContent/Actions/ContinueCareer") as Button
	_expect(not continue_career.disabled, "The career created in this session must be immediately resumable.")
	continue_career.emit_signal("pressed")
	if not await _wait_until(func() -> bool: return workspace.visible and scene.has_node("WorkspaceController")):
		_fail("Continuing a career did not create a fresh manager workspace.")
		return

	finances = scene.get_node("Workspace/Layout/Sidebar/SidebarMargin/SidebarContent/Finances") as Button
	finances.emit_signal("pressed")
	if not await _wait_until(func() -> bool: return scene.has_node(cash_path)):
		_fail("The finance office did not reopen after continuing the career.")
		return
	_expect(_label_text(scene, cash_path) == opening_cash, "Continuing a career must restore its persisted finance state.")

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
