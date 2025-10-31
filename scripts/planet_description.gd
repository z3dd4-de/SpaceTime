extends SubViewportContainer
@onready var description_label: RichTextLabel = $SubViewport/DescriptionLabel


func _ready() -> void:
	description_label.visible = false


func _on_mouse_entered() -> void:
	description_label.visible = true


func _on_mouse_exited() -> void:
	description_label.visible = false
