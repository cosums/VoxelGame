extends Node3D

func _ready() -> void:
	var gen := ChunkGenerator.new()
	gen.set_seed(1234)
	var result: Dictionary = gen.generate_chunk(Vector3(0, 0, 0), 1, false)
	print("verts: ", result["vertices"].size())
	print("mats: ", result["material_indices"].keys())