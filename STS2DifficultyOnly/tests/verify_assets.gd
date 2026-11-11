extends SceneTree
var checks := 0
var failures := 0

func check(condition: bool, message: String) -> void:
    checks += 1
    if not condition:
        failures += 1
        push_error(message)

func _initialize() -> void:
    var args := OS.get_cmdline_user_args()
    if args.size() != 1:
        push_error("Pass the built PCK path after --")
        quit(1)
        return
    check(ProjectSettings.load_resource_pack(args[0], false), "Could not mount PCK")
    var path := "res://STS2DifficultyOnly/asset_manifest.json"
    check(FileAccess.file_exists(path), "Asset manifest missing from pack")
    var manifest: Dictionary = JSON.parse_string(FileAccess.get_file_as_string(path))
    for entry in manifest.files:
        var resource_path: String = "res://" + entry
        check(FileAccess.file_exists(resource_path), "Missing packed file: " + resource_path)
        if resource_path.ends_with(".tres"):
            var texture := load(resource_path) as Texture2D
            check(texture != null, "Texture could not load: " + resource_path)
            if texture != null:
                check(texture.get_width() > 0 and texture.get_height() > 0, "Texture has zero size")
        elif resource_path.ends_with(".json"):
            var parsed: Variant = JSON.parse_string(FileAccess.get_file_as_string(resource_path))
            check(parsed is Dictionary, "Invalid localization JSON: " + resource_path)
    for name in manifest.scenes:
        var scene_path: String = "res://STS2DifficultyOnly/scenes/encounters/" + name + ".tscn"
        var packed := load(scene_path) as PackedScene
        check(packed != null, "Scene failed to load: " + scene_path)
        if packed == null:
            continue
        var scene := packed.instantiate()
        check(scene is Control, "Encounter root must be Control")
        var slots: Dictionary = manifest.scenes[name]
        check(scene.get_child_count() == slots.size(), "Incorrect slot count")
        for slot in slots:
            var marker := scene.get_node_or_null(NodePath(slot)) as Marker2D
            check(marker != null, "Required Marker2D slot missing: " + slot)
            if marker != null:
                var expected: Array = slots[slot]
                check(marker.position == Vector2(expected[0], expected[1]), "Marker coordinate changed")
                check(marker.get_parent() == scene, "Slots must be direct children for BaseLib")
        scene.free()
    print("ASSET TESTS: ", checks, " checks; ", failures, " failures. Tested in Godot ", Engine.get_version_info().string)
    quit(0 if failures == 0 else 1)
