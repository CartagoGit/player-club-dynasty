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
	var transfer_budget_path := "Workspace/Layout/ContentMargin/ContentScroll/Content/FinancesOffice/FinanceMetrics/TransferBudget/Content/Value"
	if not await _wait_until(func() -> bool: return scene.has_node(cash_path)):
		_fail("The persistent finance office did not open.")
		return
	var opening_cash := _label_text(scene, cash_path)
	var opening_transfer_budget := _label_text(scene, transfer_budget_path)
	_expect(opening_cash.contains("EUR"), "The finance office must render meaningful currency values.")

	var market_button := scene.get_node("Workspace/Layout/Sidebar/SidebarMargin/SidebarContent/Market") as Button
	_expect(not market_button.disabled, "The transfer market must be enabled.")
	market_button.emit_signal("pressed")
	await process_frame
	var market_path := "Workspace/Layout/ContentMargin/ContentScroll/Content/TransferMarket"
	var market_scope_path := market_path + "/Header/ClubScope"
	if not await _wait_until(func() -> bool: return scene.has_node(market_path)):
		_fail("The playable transfer market did not open.")
		return
	var market_rows := scene.get_node(market_path + "/CandidateRows") as VBoxContainer
	var managed_market_scope := _label_text(scene, market_scope_path)
	var candidate_count := market_rows.get_child_count()
	_expect(candidate_count > 0, "The transfer market must expose deterministic candidates.")
	var first_candidate := market_rows.get_child(0) as HBoxContainer
	var signed_player := (first_candidate.get_child(0) as Label).text
	var sign_player := first_candidate.get_node("Sign") as Button
	sign_player.emit_signal("pressed")
	var market_notice_path := market_path + "/Notice"
	if not await _wait_until(func() -> bool: return scene.has_node(market_notice_path) and _label_text(scene, market_notice_path).contains("FICHAJE CERRADO")):
		_fail("An affordable one-click transfer was not settled.")
		return
	market_rows = scene.get_node(market_path + "/CandidateRows") as VBoxContainer
	_expect(market_rows.get_child_count() == candidate_count - 1, "The acquired player must leave the transfer candidate list.")

	finances.emit_signal("pressed")
	if not await _wait_until(func() -> bool: return scene.has_node(cash_path)):
		_fail("Finances did not refresh after the transfer.")
		return
	var cash_after_transfer := _label_text(scene, cash_path)
	var budget_after_transfer := _label_text(scene, transfer_budget_path)
	_expect(cash_after_transfer != opening_cash, "A completed transfer must reduce cash.")
	_expect(budget_after_transfer != opening_transfer_budget, "A completed transfer must reduce the transfer budget.")

	var squad_button := scene.get_node("Workspace/Layout/Sidebar/SidebarMargin/SidebarContent/Squad") as Button
	squad_button.emit_signal("pressed")
	var squad_path := "Workspace/Layout/ContentMargin/ContentScroll/Content/SquadList"
	if not await _wait_until(func() -> bool: return scene.has_node(squad_path)):
		_fail("The managed squad did not open after the transfer.")
		return
	_expect(_contains_label_text(scene.get_node(squad_path), signed_player), "The acquired player must appear in the managed squad.")
	var squad := scene.get_node(squad_path) as VBoxContainer
	var rendered_squad_size := squad.get_child_count() - 3
	_expect(_label_text(squad, "SquadHeader").contains(str(rendered_squad_size) + " JUGADORES"), "The squad header must count acquired players.")

	var overview := scene.get_node("Workspace/Layout/Sidebar/SidebarMargin/SidebarContent/Overview") as Button
	overview.emit_signal("pressed")
	await process_frame
	var change_club_path := "Workspace/Layout/ContentMargin/ContentScroll/Content/ClubOverview/ClubHeader/ChangeClub"
	var change_club := scene.get_node(change_club_path) as Button
	change_club.emit_signal("pressed")
	await process_frame
	var content := scene.get_node("Workspace/Layout/ContentMargin/ContentScroll/Content") as Control
	var picker_layout := content.get_child(0) as VBoxContainer
	_expect(picker_layout.get_child_count() > 4, "The club browser must expose another fictional club.")
	var second_club_row := picker_layout.get_child(4) as HBoxContainer
	var second_club_action := second_club_row.get_child(6) as HBoxContainer
	var choose_second_club := second_club_action.get_child(0) as Button
	choose_second_club.emit_signal("pressed")
	await process_frame
	finances.emit_signal("pressed")
	if not await _wait_until(func() -> bool: return scene.has_node(cash_path)):
		_fail("Finances failed after browsing a non-managed club.")
		return
	_expect(_label_text(scene, cash_path) == cash_after_transfer, "Finances must remain scoped to the managed career club.")
	market_button.emit_signal("pressed")
	if not await _wait_until(func() -> bool: return scene.has_node(market_path)):
		_fail("Market failed after browsing a non-managed club.")
		return
	_expect(_label_text(scene, market_scope_path) == managed_market_scope, "Market must remain scoped to the managed career club.")
	_expect(not _contains_label_text(scene.get_node(market_path + "/CandidateRows"), signed_player), "The signed player must stay unavailable after club browsing.")

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
	_expect(_label_text(scene, cash_path) == cash_after_transfer, "Continuing a career must restore its post-transfer finance state.")
	_expect(_label_text(scene, transfer_budget_path) == budget_after_transfer, "Continuing a career must restore its transfer budget.")

	squad_button = scene.get_node("Workspace/Layout/Sidebar/SidebarMargin/SidebarContent/Squad") as Button
	squad_button.emit_signal("pressed")
	if not await _wait_until(func() -> bool: return scene.has_node(squad_path)):
		_fail("The squad did not reopen after continuing the career.")
		return
	_expect(_contains_label_text(scene.get_node(squad_path), signed_player), "Continuing a career must restore the acquired player.")

	market_button = scene.get_node("Workspace/Layout/Sidebar/SidebarMargin/SidebarContent/Market") as Button
	market_button.emit_signal("pressed")
	if not await _wait_until(func() -> bool: return scene.has_node(market_path)):
		_fail("Market did not reopen after continuing the career.")
		return
	_expect(_label_text(scene, market_scope_path) == managed_market_scope, "Continuing must restore the managed market scope.")
	_expect(not _contains_label_text(scene.get_node(market_path + "/CandidateRows"), signed_player), "Continuing must keep an acquired player out of the market.")

	main_menu_button = scene.get_node("Workspace/Layout/Sidebar/SidebarMargin/SidebarContent/MainMenu") as Button
	main_menu_button.emit_signal("pressed")
	if not await _wait_until(func() -> bool: return main_menu.visible and not workspace.visible and not scene.has_node("WorkspaceController")):
		_fail("Returning before a second new career did not clean up the workspace.")
		return

	new_career.emit_signal("pressed")
	await process_frame
	club_list = scene.get_node("MainMenu/CareerSetup/SetupContent/ClubScroll/ClubList") as VBoxContainer
	if not await _wait_until(func() -> bool: return club_list.get_child_count() > 0):
		_fail("The second new-career setup did not render clubs.")
		return
	first_club = club_list.get_child(0) as Button
	first_club.emit_signal("pressed")
	await process_frame
	start_career = scene.get_node("MainMenu/CareerSetup/SetupContent/SetupActions/StartCareer") as Button
	start_career.emit_signal("pressed")
	if not await _wait_until(func() -> bool: return workspace.visible and scene.has_node("WorkspaceController")):
		_fail("The second new career did not open a fresh workspace.")
		return

	market_button = scene.get_node("Workspace/Layout/Sidebar/SidebarMargin/SidebarContent/Market") as Button
	market_button.emit_signal("pressed")
	if not await _wait_until(func() -> bool: return scene.has_node(market_path)):
		_fail("Market did not open in the second new career.")
		return
	_expect(_contains_label_text(scene.get_node(market_path + "/CandidateRows"), signed_player), "A new career must clear prior transfers and restore the player to the market.")

	finances = scene.get_node("Workspace/Layout/Sidebar/SidebarMargin/SidebarContent/Finances") as Button
	finances.emit_signal("pressed")
	if not await _wait_until(func() -> bool: return scene.has_node(cash_path)):
		_fail("Finances did not open in the second new career.")
		return
	_expect(_label_text(scene, cash_path) == opening_cash, "A new career must restore opening cash.")
	_expect(_label_text(scene, transfer_budget_path) == opening_transfer_budget, "A new career must restore its opening transfer budget.")

	squad_button = scene.get_node("Workspace/Layout/Sidebar/SidebarMargin/SidebarContent/Squad") as Button
	squad_button.emit_signal("pressed")
	if not await _wait_until(func() -> bool: return scene.has_node(squad_path)):
		_fail("Squad did not open in the second new career.")
		return
	_expect(not _contains_label_text(scene.get_node(squad_path), signed_player), "A new career must clear the acquired squad projection.")

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


func _contains_label_text(parent: Node, text: String) -> bool:
	if parent is Label and (parent as Label).text == text:
		return true
	for child in parent.get_children():
		if _contains_label_text(child, text):
			return true
	return false


func _expect(condition: bool, message: String) -> void:
	if not condition:
		_failed = true
		push_error(message)


func _fail(message: String) -> void:
	push_error(message)
	quit(1)
