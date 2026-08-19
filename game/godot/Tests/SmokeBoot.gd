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

	# Data source: load the shipped world through the external path and put it back.
	var options_button := scene.get_node("MainMenu/MenuFrame/MenuContent/Actions/Options") as Button
	options_button.emit_signal("pressed")
	await process_frame
	var settings := _find_node(scene, "OptionsBoard")
	if settings == null:
		_fail("Settings did not open as a configuration board.")
		return
	var groups := _find_node(settings, "GroupsFlow")
	_expect(groups.get_child_count() >= 4, "Settings must show every group of preferences at once.")
	# Groups are addressed by node name: their titles are translated.
	for group_name in ["SectionLanguage", "SectionTheme", "SectionDisplay", "SectionData"]:
		var group := _find_node(settings, group_name)
		if group == null:
			_fail("The settings board has no %s group." % group_name)
			return
		_expect(group.visible, "Every settings group must be visible at once: %s was hidden." % group_name)

	# The language and the theme pickers must actually accept a click. They are
	# tiles, not buttons: a Button with Control children swallows its own input.
	_check_picker_is_clickable(settings, "SectionLanguage", "the language picker")
	_check_picker_is_clickable(settings, "SectionTheme", "the theme picker")
	_expect(not _contains_label_text(settings, "OPTIONS_THEME_HEADER"), "The theme group is rendering a raw translation key.")

	var source_node := _find_node(settings, "ContentSource")
	if source_node == null:
		_fail("The data group did not show the content source controls.")
		return
	var source_path := String(scene.get_path_to(source_node))
	if not await _wait_until(func() -> bool: return scene.has_node(source_path)):
		_fail("The data area did not show the content source controls.")
		return
	var source_status := scene.get_node(source_path + "/ContentSourceStatus") as Label
	_expect(source_status.text.begins_with("Integrado"), "The shipped world must be reported as the integrated source.")
	var integrated_hash := source_status.text
	var external_path := scene.get_node(source_path + "/ExternalPath") as LineEdit
	external_path.text = ProjectSettings.globalize_path("res://../content/starter-world")
	(scene.get_node(source_path + "/ContentSourceActions/LoadExternalSource") as Button).emit_signal("pressed")
	if not await _wait_until(func() -> bool: return scene.has_node(source_path) and (scene.get_node(source_path + "/ContentSourceStatus") as Label).text.begins_with("Externo")):
		_fail("The external world was not loaded.")
		return
	source_status = scene.get_node(source_path + "/ContentSourceStatus") as Label
	_expect(source_status.text.split(" · ")[-1] == integrated_hash.split(" · ")[-1], "The same content read as an external source must keep its hash.")
	(scene.get_node(source_path + "/ContentSourceActions/RestoreIntegratedSource") as Button).emit_signal("pressed")
	if not await _wait_until(func() -> bool: return (scene.get_node(source_path + "/ContentSourceStatus") as Label).text.begins_with("Integrado")):
		_fail("Restoring did not return to the integrated world.")
		return
	source_status = scene.get_node(source_path + "/ContentSourceStatus") as Label
	# One command bar, one way out.
	var settings_back := _find_node(settings, "Back") as Button
	settings_back.emit_signal("pressed")
	await process_frame
	_expect(main_menu.visible, "Leaving settings must return to the main menu.")
	_check_nothing_overflows(main_menu)

	var new_career := scene.get_node("MainMenu/MenuFrame/MenuContent/Actions/NewCareer") as Button
	new_career.emit_signal("pressed")
	await process_frame

	var club_list := scene.get_node("MainMenu/CareerSetup/SetupContent/ClubScroll/ClubList") as VBoxContainer
	if not await _wait_until(func() -> bool: return _row_count(club_list, "ClubTable") > 0):
		_fail("The career setup did not render starter-world clubs.")
		return
	# A table whose rows lay out at zero pixels is in the tree and invisible:
	# assert the first row has real height before trusting the count.
	var first_row := _row_at(club_list, "ClubTable", 0) as Control
	_expect(first_row.size.y > 0.0, "The club table rendered rows with no height.")
	var choose_first := _find_node(first_row, "Choose") as Button
	choose_first.emit_signal("pressed")
	await process_frame

	var start_career := scene.get_node("MainMenu/CareerSetup/SetupContent/SetupActions/StartCareer") as Button
	_expect(not start_career.disabled, "Selecting a club must enable career start.")
	start_career.emit_signal("pressed")
	await process_frame

	_expect(not main_menu.visible, "The main menu must close after starting a career.")
	_expect(workspace.visible, "The manager workspace must become visible after starting a career.")
	_expect(scene.has_node("WorkspaceController"), "The wired workspace controller is missing.")

	var finances := scene.get_node("Workspace/Frame/Layout/Sidebar/SidebarMargin/SidebarContent/Finances") as Button
	_expect(not finances.disabled, "The finance office must be enabled.")
	finances.emit_signal("pressed")
	await process_frame
	var cash_path := "Workspace/Frame/Layout/ContentMargin/ContentScroll/Content/FinancesOffice/FinanceMetrics/Cash/Content/Value"
	var transfer_budget_path := "Workspace/Frame/Layout/ContentMargin/ContentScroll/Content/FinancesOffice/FinanceMetrics/TransferBudget/Content/Value"
	if not await _wait_until(func() -> bool: return scene.has_node(cash_path)):
		_fail("The persistent finance office did not open.")
		return
	var opening_cash := _label_text(scene, cash_path)
	var opening_transfer_budget := _label_text(scene, transfer_budget_path)
	_expect(opening_cash.contains("EUR"), "The finance office must render meaningful currency values.")

	var market_button := scene.get_node("Workspace/Frame/Layout/Sidebar/SidebarMargin/SidebarContent/Market") as Button
	_expect(not market_button.disabled, "The transfer market must be enabled.")
	market_button.emit_signal("pressed")
	await process_frame
	var market_path := "Workspace/Frame/Layout/ContentMargin/ContentScroll/Content/TransferMarket"
	var market_scope_path := market_path + "/Header/ClubScope"
	if not await _wait_until(func() -> bool: return scene.has_node(market_path)):
		_fail("The playable transfer market did not open.")
		return
	var managed_market_scope := _label_text(scene, market_scope_path)
	var candidate_count := _row_count(_content(scene), "CandidateRows")
	_expect(candidate_count > 0, "The transfer market must expose deterministic candidates.")
	var first_candidate := _row_at(_content(scene), "CandidateRows", 0)
	var signed_player := _first_cell_text(first_candidate)
	_expect(not signed_player.begins_with("MARKET_"), "The market must render sentences, not translation keys.")
	var sign_player := _find_node(first_candidate, "Sign") as Button
	var market_notice_path := market_path + "/Notice"
	# Language-independent: the notice must change from its idle sentence and
	# the signed player must leave the list. Asserting Spanish copy here made
	# the smoke fail on an English fallback.
	var idle_notice := _label_text(scene, market_notice_path) if scene.has_node(market_notice_path) else ""
	sign_player.emit_signal("pressed")
	if not await _wait_until(func() -> bool: return scene.has_node(market_notice_path) and _label_text(scene, market_notice_path) != idle_notice):
		_fail("An affordable one-click transfer was not settled. Notice still reads: " + (_label_text(scene, market_notice_path) if scene.has_node(market_notice_path) else "<no notice node>"))
		return
	_expect(not _label_text(scene, market_notice_path).begins_with("MARKET_"), "The settlement notice must be a sentence, not a translation key.")
	_expect(_row_count(_content(scene), "CandidateRows") == candidate_count - 1, "The acquired player must leave the transfer candidate list.")

	finances.emit_signal("pressed")
	if not await _wait_until(func() -> bool: return scene.has_node(cash_path)):
		_fail("Finances did not refresh after the transfer.")
		return
	var cash_after_transfer := _label_text(scene, cash_path)
	var budget_after_transfer := _label_text(scene, transfer_budget_path)
	_expect(cash_after_transfer != opening_cash, "A completed transfer must reduce cash.")
	_expect(budget_after_transfer != opening_transfer_budget, "A completed transfer must reduce the transfer budget.")

	var squad_button := scene.get_node("Workspace/Frame/Layout/Sidebar/SidebarMargin/SidebarContent/Squad") as Button
	squad_button.emit_signal("pressed")
	if not await _wait_until(func() -> bool: return _find_node(_content(scene), "SquadList") != null):
		_fail("The managed squad did not open after the transfer.")
		return
	_expect(_contains_label_text(_find_node(_content(scene), "SquadList"), signed_player), "The acquired player must appear in the managed squad.")
	var rendered_squad_size := _row_count(_content(scene), "SquadList")
	_expect(rendered_squad_size > 0, "The managed squad must render its players.")
	# The title is the static section name; the club and the head-count live in
	# the summary line under it.
	_expect(not _panel_text(_content(scene), "SquadHeaderPanel", "Title").begins_with("SQUAD_"), "The squad must render sentences, not translation keys.")
	var managed_squad_summary := _panel_text(_content(scene), "SquadHeaderPanel", "Summary")
	_expect(managed_squad_summary.contains(str(rendered_squad_size)), "The squad summary must count the players it renders. Summary reads: '" + managed_squad_summary + "' rows=" + str(rendered_squad_size))
	# The persistent header always names the managed club, whatever is browsed.
	var managed_club_name := _panel_text(scene, "HeaderRow", "Club").split(" · ")[0]
	_expect(managed_club_name.length() > 0, "The workspace header must name the managed club.")

	var overview := scene.get_node("Workspace/Frame/Layout/Sidebar/SidebarMargin/SidebarContent/Overview") as Button
	overview.emit_signal("pressed")
	await process_frame
	var change_club_path := "Workspace/Frame/Layout/ContentMargin/ContentScroll/Content/ClubOverview/ClubHeader/ChangeClub"
	var change_club := scene.get_node(change_club_path) as Button
	change_club.emit_signal("pressed")
	await process_frame
	if not await _wait_until(func() -> bool: return _row_count(_content(scene), "ClubPickerRows") > 1):
		_fail("The club browser did not list the world's clubs.")
		return
	# Re-resolve the rows every time: switching club re-renders the screen, so
	# a container captured earlier can already be queued for deletion.
	var chosen := false
	for index in range(_row_count(_content(scene), "ClubPickerRows")):
		var candidate_row := _row_at(_content(scene), "ClubPickerRows", index)
		if candidate_row == null:
			continue
		var choose := _find_node(candidate_row, "Choose") as Button
		if choose != null and not choose.disabled:
			choose.emit_signal("pressed")
			chosen = true
			break
	if not chosen:
		_fail("The club browser must offer a club other than the managed one.")
		return
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
	_expect(not _contains_label_text(_find_node(_content(scene), "CandidateRows"), signed_player), "The signed player must stay unavailable after club browsing.")

	squad_button.emit_signal("pressed")
	if not await _wait_until(func() -> bool: return _find_node(_content(scene), "SquadList") != null):
		_fail("The squad of a browsed rival did not open.")
		return
	var rival_squad_size := _row_count(_content(scene), "SquadList")
	_expect(rival_squad_size > 0, "A browsed rival must expose its own players.")
	_expect(_panel_text(_content(scene), "SquadHeaderPanel", "Summary") != managed_squad_summary, "Browsing a rival must show the rival squad, not the managed one.")

	var tactics := scene.get_node("Workspace/Frame/Layout/Sidebar/SidebarMargin/SidebarContent/Tactics") as Button
	_expect(not tactics.disabled, "The tactics route must be enabled.")
	tactics.emit_signal("pressed")
	await process_frame

	var lineup_path := "Workspace/Frame/Layout/ContentMargin/ContentScroll/Content/LineupSelection"
	if not await _wait_until(func() -> bool: return scene.has_node(lineup_path)):
		_fail("The persisted lineup selector did not open.")
		return

	var lineup := scene.get_node(lineup_path) as Control
	var lineup_title := (lineup.get_node("Margin/Content/Title") as Label).text
	_expect(lineup_title.begins_with(managed_club_name), "A match plan must belong to the managed club even while browsing a rival.")
	var candidates := lineup.get_node("Margin/Content/CandidateScroll/CandidateRows") as VBoxContainer
	_expect(candidates.get_child_count() > 0, "The lineup selector must show the active squad.")
	var submit := lineup.get_node("Margin/Content/ActionRow/Submit") as Button
	submit.emit_signal("pressed")
	await process_frame
	var lineup_status := lineup.get_node("Margin/Content/ActionRow/Status") as Label
	_expect(lineup_status.text == "Selección enviada.", "The lineup plan was not accepted and persisted.")

	var main_menu_button := scene.get_node("Workspace/Frame/Layout/Sidebar/SidebarMargin/SidebarContent/MainMenu") as Button
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

	finances = scene.get_node("Workspace/Frame/Layout/Sidebar/SidebarMargin/SidebarContent/Finances") as Button
	finances.emit_signal("pressed")
	if not await _wait_until(func() -> bool: return scene.has_node(cash_path)):
		_fail("The finance office did not reopen after continuing the career.")
		return
	_expect(_label_text(scene, cash_path) == cash_after_transfer, "Continuing a career must restore its post-transfer finance state.")
	_expect(_label_text(scene, transfer_budget_path) == budget_after_transfer, "Continuing a career must restore its transfer budget.")

	squad_button = scene.get_node("Workspace/Frame/Layout/Sidebar/SidebarMargin/SidebarContent/Squad") as Button
	squad_button.emit_signal("pressed")
	if not await _wait_until(func() -> bool: return _find_node(_content(scene), "SquadList") != null):
		_fail("The squad did not reopen after continuing the career.")
		return
	_expect(_contains_label_text(_find_node(_content(scene), "SquadList"), signed_player), "Continuing a career must restore the acquired player.")

	market_button = scene.get_node("Workspace/Frame/Layout/Sidebar/SidebarMargin/SidebarContent/Market") as Button
	market_button.emit_signal("pressed")
	if not await _wait_until(func() -> bool: return scene.has_node(market_path)):
		_fail("Market did not reopen after continuing the career.")
		return
	_expect(_label_text(scene, market_scope_path) == managed_market_scope, "Continuing must restore the managed market scope.")
	_expect(not _contains_label_text(scene.get_node(market_path + "/CandidateRows"), signed_player), "Continuing must keep an acquired player out of the market.")

	main_menu_button = scene.get_node("Workspace/Frame/Layout/Sidebar/SidebarMargin/SidebarContent/MainMenu") as Button
	main_menu_button.emit_signal("pressed")
	if not await _wait_until(func() -> bool: return main_menu.visible and not workspace.visible and not scene.has_node("WorkspaceController")):
		_fail("Returning before a second new career did not clean up the workspace.")
		return

	new_career.emit_signal("pressed")
	await process_frame
	club_list = scene.get_node("MainMenu/CareerSetup/SetupContent/ClubScroll/ClubList") as VBoxContainer
	if not await _wait_until(func() -> bool: return _row_count(club_list, "ClubTable") > 0):
		_fail("The second new-career setup did not render clubs.")
		return
	first_row = _row_at(club_list, "ClubTable", 0) as Control
	var choose_again := _find_node(first_row, "Choose") as Button
	choose_again.emit_signal("pressed")
	await process_frame
	start_career = scene.get_node("MainMenu/CareerSetup/SetupContent/SetupActions/StartCareer") as Button
	start_career.emit_signal("pressed")
	if not await _wait_until(func() -> bool: return workspace.visible and scene.has_node("WorkspaceController")):
		_fail("The second new career did not open a fresh workspace.")
		return

	market_button = scene.get_node("Workspace/Frame/Layout/Sidebar/SidebarMargin/SidebarContent/Market") as Button
	market_button.emit_signal("pressed")
	if not await _wait_until(func() -> bool: return scene.has_node(market_path)):
		_fail("Market did not open in the second new career.")
		return
	_expect(_contains_label_text(scene.get_node(market_path + "/CandidateRows"), signed_player), "A new career must clear prior transfers and restore the player to the market.")

	finances = scene.get_node("Workspace/Frame/Layout/Sidebar/SidebarMargin/SidebarContent/Finances") as Button
	finances.emit_signal("pressed")
	if not await _wait_until(func() -> bool: return scene.has_node(cash_path)):
		_fail("Finances did not open in the second new career.")
		return
	_expect(_label_text(scene, cash_path) == opening_cash, "A new career must restore opening cash.")
	_expect(_label_text(scene, transfer_budget_path) == opening_transfer_budget, "A new career must restore its opening transfer budget.")

	squad_button = scene.get_node("Workspace/Frame/Layout/Sidebar/SidebarMargin/SidebarContent/Squad") as Button
	squad_button.emit_signal("pressed")
	if not await _wait_until(func() -> bool: return _find_node(_content(scene), "SquadList") != null):
		_fail("Squad did not open in the second new career.")
		return
	_expect(not _contains_label_text(_find_node(_content(scene), "SquadList"), signed_player), "A new career must clear the acquired squad projection.")

	quit(1 if _failed else 0)


const CONTENT_PATH := "Workspace/Frame/Layout/ContentMargin/ContentScroll/Content"


# Screens are re-rendered on every navigation, and a freed node can still be in
# the tree for a frame. Searching from the live content root instead of the
# whole scene avoids matching the corpse of the previous screen.
func _content(scene: Node) -> Node:
	return scene.get_node(CONTENT_PATH) if scene.has_node(CONTENT_PATH) else scene


func _find_node(root: Node, node_name: String) -> Node:
	if root.name == node_name:
		return root
	for child in root.get_children():
		var found := _find_node(child, node_name)
		if found != null:
			return found
	return null


func _rows_of(scene: Node, section_name: String) -> VBoxContainer:
	var section := _find_node(scene, section_name)
	if section == null:
		return null
	return _find_node(section, "Rows") as VBoxContainer


func _row_count(scene: Node, section_name: String) -> int:
	var rows := _rows_of(scene, section_name)
	return 0 if rows == null else rows.get_child_count()


func _row_at(scene: Node, section_name: String, index: int) -> Node:
	var rows := _rows_of(scene, section_name)
	if rows == null or rows.get_child_count() <= index:
		return null
	return rows.get_child(index)


func _first_cell_text(row: Node) -> String:
	var cells := _find_node(row, "Cells")
	if cells == null or cells.get_child_count() == 0:
		return ""
	var first := cells.get_child(0)
	return (first as Label).text if first is Label else ""


func _panel_text(scene: Node, panel_name: String, label_name: String) -> String:
	var panel := _find_node(scene, panel_name)
	if panel == null:
		return ""
	var label := _find_node(panel, label_name)
	return "" if label == null else (label as Label).text


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


## Presses the first tile of a picker and asserts the selection actually moved.
## A control that cannot be clicked reports the same state before and after.
func _check_picker_is_clickable(settings: Node, group_name: String, description: String) -> void:
	var group := _find_node(settings, group_name)
	if group == null:
		_fail("There is no %s." % description)
		return
	var tiles: Array[Node] = []
	_collect_tiles(group, tiles)
	if tiles.size() < 2:
		_fail("%s offers fewer than two choices, so a click cannot be proven." % description)
		return
	var originally_selected: Node = null
	var unselected: Node = null
	for tile in tiles:
		if tile.get("Selected"):
			originally_selected = tile
		elif unselected == null:
			unselected = tile
	if unselected == null:
		_fail("%s has every choice selected at once." % description)
		return
	unselected.call("Press")
	_expect(unselected.get("Selected"), "%s did not react to a click." % description)
	# The press goes through the real path, preferences included, so put the
	# player's choice back rather than leaving the test's pick behind.
	if originally_selected != null:
		originally_selected.call("Press")


func _collect_tiles(node: Node, into: Array[Node]) -> void:
	if node.has_method("Press") and node.get("Selected") != null:
		into.append(node)
	for child in node.get_children():
		_collect_tiles(child, into)


## No panel may leave the window. Absolute offsets sized for one resolution are
## how the shell used to spill outside its frame.
func _check_nothing_overflows(root: Control) -> void:
	var frame := Rect2(Vector2.ZERO, root.size)
	for child in root.get_children():
		if child is not Control:
			continue
		var control := child as Control
		if not control.visible:
			continue
		var box := Rect2(control.position, control.size)
		var fits := box.position.x >= -1.0 and box.position.y >= -1.0 \
			and box.end.x <= frame.end.x + 1.0 and box.end.y <= frame.end.y + 1.0
		_expect(fits, "%s leaves the window: %s is outside %s." % [control.name, box, frame])
