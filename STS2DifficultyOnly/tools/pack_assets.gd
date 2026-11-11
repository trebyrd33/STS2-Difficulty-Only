extends SceneTree

func fail(message: String) -> void:
    push_error(message)
    quit(1)

func add_tree(packer: PCKPacker, directory: String) -> Error:
    var dir := DirAccess.open(directory)
    if dir == null:
        return ERR_CANT_OPEN
    for filename in dir.get_files():
        if filename.ends_with(".uid") or filename.ends_with(".import"):
            continue
        var resource_path := directory.path_join(filename)
        var result := packer.add_file(resource_path, resource_path)
        if result != OK:
            return result
    for child in dir.get_directories():
        var result := add_tree(packer, directory.path_join(child))
        if result != OK:
            return result
    return OK

func _initialize() -> void:
    var args := OS.get_cmdline_user_args()
    if args.size() != 1:
        fail("Pass the destination PCK path after --")
        return
    var packer := PCKPacker.new()
    if packer.pck_start(args[0]) != OK:
        fail("Could not create " + args[0])
        return
    if add_tree(packer, "res://STS2DifficultyOnly") != OK or packer.flush() != OK:
        fail("Failed packing Difficulty Only assets")
        return
    print("PACKED: ", args[0])
    quit(0)
