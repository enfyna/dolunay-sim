extends Node3D

@export var arac : Arac

@export var viewport_1 : SubViewport
@export var viewport_2 : SubViewport

@export var save_img : bool = false

var cam_1 : Camera3D
var cam_2 : Camera3D

func _ready():
	cam_1 = Camera3D.new()
	cam_2 = Camera3D.new()

	viewport_1.add_child(cam_1)
	viewport_2.add_child(cam_2)


func _process(_delta):

	cam_1.global_transform = arac.FrontCam.global_transform
	cam_2.global_transform = arac.BottomCam.global_transform

	if save_img:
		await RenderingServer.frame_post_draw
		
		viewport_1.get_texture().get_image().save_jpg("res://cam_1.jpg")
		viewport_2.get_texture().get_image().save_jpg("res://cam_2.jpg")