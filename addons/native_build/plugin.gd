@tool
extends EditorPlugin

const NATIVE_DIR := "./native"

func _build() -> bool:
	var project_dir := ProjectSettings.globalize_path("res://")
	var native_path := project_dir.path_join(NATIVE_DIR).simplify_path()

	var output: Array = []
	var code := OS.execute("cmd", [
		"/c", "scons",
		"-C", native_path,
		"platform=windows",
		"target=template_debug",
	], output, true)

	if code != 0:
		push_error("scons failed (exit %d):\n%s" % [code, "\n".join(output)])
		return false

	return true