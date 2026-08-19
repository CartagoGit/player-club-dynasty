extends SceneTree

## Walks the running game through a plan of steps and photographs it.
##
## The point is to look at the app instead of describing it. `CaptureShots.gd`
## does that too, but its route is hard-coded, so exploring anywhere new means
## editing GDScript. Here the route is data: a JSON plan handed in through the
## environment, which means any route can be walked without touching the file.
##
##   PCDYNASTY_EXPLORE='[{"shot":"menu"},{"press":"NewCareer"},{"shot":"setup"}]' \
##     godot --path game/godot --script res://Tests/Explore.gd
##
## Steps:
##   {"shot": "name"}            save user://shots/name.png (or $PCDYNASTY_SHOTS_DIR)
##   {"press": "NodeName"}       activate a Button, a tile, or anything with Press()
##   {"wait": 30}                let N frames go by
##   {"dump": "NodeName"}        print the subtree under that node, with types
##   {"find": "NodeName"}        print whether the node exists, and where
##   {"text": "NodeName"}        print the node's text, if it has any
##   {"resize": [1280, 720]}     resize the window, to catch layout that overflows
##   {"theme": "high-contrast"}  not yet wired; reported as skipped
##
## Every step prints one line, so the transcript is the record of what happened.

const MAIN_SCENE := preload("res://Scenes/Main.tscn")

var _out_dir: String
var _scene: Node
var _failures: int = 0


func _initialize() -> void:
	call_deferred("_run")


func _run() -> void:
	_out_dir = OS.get_environment("PCDYNASTY_SHOTS_DIR")
	if _out_dir.is_empty():
		_out_dir = ProjectSettings.globalize_path("user://shots")
	DirAccess.make_dir_recursive_absolute(_out_dir)

	var plan := _read_plan()
	if plan.is_empty():
		push_error("explore: PCDYNASTY_EXPLORE is missing or is not a JSON array of steps.")
		quit(2)
		return

	_scene = MAIN_SCENE.instantiate()
	root.add_child(_scene)
	# The bootstrap loads the world asynchronously; nothing is worth doing
	# until the first route has settled.
	await _settle(90)

	for step in plan:
		await _apply(step)

	print("explore: %d step(s) run, %d failure(s), PNGs in %s" % [plan.size(), _failures, _out_dir])
	quit(1 if _failures > 0 else 0)


func _read_plan() -> Array:
	var raw := OS.get_environment("PCDYNASTY_EXPLORE")
	if raw.is_empty():
		return []
	var parsed = JSON.parse_string(raw)
	return parsed if parsed is Array else []


func _apply(step: Variant) -> void:
	if not step is Dictionary:
		_fail("step is not an object: %s" % str(step))
		return

	if step.has("wait"):
		await _settle(int(step["wait"]))
		print("explore: waited %d frame(s)" % int(step["wait"]))
		return

	if step.has("shot"):
		await _shot(str(step["shot"]))
		return

	if step.has("press"):
		await _press(str(step["press"]))
		return

	if step.has("dump"):
		_dump(str(step["dump"]))
		return

	if step.has("find"):
		_report_find(str(step["find"]))
		return

	if step.has("text"):
		_report_text(str(step["text"]))
		return

	if step.has("resize"):
		var size: Array = step["resize"]
		DisplayServer.window_set_size(Vector2i(int(size[0]), int(size[1])))
		await _settle(20)
		print("explore: resized to %dx%d" % [int(size[0]), int(size[1])])
		return

	if step.has("theme"):
		print("explore: theme switching is not wired into the explorer yet; skipped")
		return

	_fail("unknown step: %s" % str(step))


func _press(node_name: String) -> void:
	var node := _find(_scene, node_name)
	if node == null:
		_fail("press: no node named '%s'" % node_name)
		return

	# Three kinds of thing are pressable and they do not share an interface:
	# a Button has the signal, our tiles are PanelContainers with a Press()
	# method (a Button is not a container, so they cannot be buttons).
	if node.has_method("Press"):
		node.call("Press")
	elif node.has_signal("pressed"):
		node.emit_signal("pressed")
	else:
		_fail("press: '%s' (%s) is not pressable" % [node_name, node.get_class()])
		return

	await _settle(30)
	print("explore: pressed %s (%s)" % [node_name, node.get_class()])


func _dump(node_name: String) -> void:
	var node := _scene if node_name == "*" else _find(_scene, node_name)
	if node == null:
		_fail("dump: no node named '%s'" % node_name)
		return
	print("explore: tree under %s" % node_name)
	_print_tree(node, 0)


func _print_tree(node: Node, depth: int) -> void:
	var label := ""
	if node is Control and not (node as Control).visible:
		label = "  [hidden]"
	print("  %s%s (%s)%s" % ["  ".repeat(depth), node.name, node.get_class(), label])
	if depth > 6:
		return
	for child in node.get_children():
		_print_tree(child, depth + 1)


func _report_find(node_name: String) -> void:
	var node := _find(_scene, node_name)
	if node == null:
		print("explore: '%s' not found" % node_name)
		return
	print("explore: '%s' is %s at %s" % [node_name, node.get_class(), _scene.get_path_to(node)])


func _report_text(node_name: String) -> void:
	var node := _find(_scene, node_name)
	if node == null:
		_fail("text: no node named '%s'" % node_name)
		return
	if "text" in node:
		print("explore: %s.text = %s" % [node_name, node.get("text")])
	else:
		_fail("text: '%s' (%s) has no text" % [node_name, node.get_class()])


func _settle(frames: int) -> void:
	for _index in maxi(frames, 1):
		await process_frame


func _shot(name: String) -> void:
	await RenderingServer.frame_post_draw
	var image := get_root().get_texture().get_image()
	var path := _out_dir.path_join("%s.png" % name)
	var error := image.save_png(path)
	if error != OK:
		_fail("shot: could not write %s (%d)" % [path, error])
	else:
		print("explore: shot %s" % path)


func _find(node: Node, node_name: String) -> Node:
	if node.name == node_name:
		return node
	for child in node.get_children():
		var found := _find(child, node_name)
		if found != null:
			return found
	return null


func _fail(message: String) -> void:
	_failures += 1
	push_error("explore: " + message)
