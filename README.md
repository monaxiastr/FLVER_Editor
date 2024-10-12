# FLVER_Editor

A probably multi-functional editor to edit Fromsoftware game's FLVER model file (Sekiro, Dark Souls, Bloodborne etc.)
Users can view and edit models, materials, dummy points inside the FLVER file and can also import external models into the FLVER file.

## Functionality

- Texture loading functionality: the tpf file's name must be the same as flver file name.

- Silence vertex deletion functionality: ctrl + right click in 3d model viewing software to enter such mode, then press alt + right click to quick delete vertex.

- mesh->M. Reset functionality to help you port DS2 .flv file and make it compatible with new P[ARSN] material.

- "Material->Xml Edit" functionalty, editor can auto set texture description file depacked from .tpf file.

- "Import model->Auto set texture path" functionality, when importing fbx files, user can choose auto set texture path insead typing them manually. (Can auto read and set diffuse, specular and normal channels' first textures.)

- "Mesh->N.Flip" button near the scale textfields. Scale the normals according to the values you typed in the scale textfields.

- "Mesh->Rotate in degrees" check box. By checking this you can rotate meshes in degrees instead of radiant.

- Finding parent bone functionality. If a vertex is bind to a bone that does not exist in original flver file. It will try to find if its parent
  bone is bind to a existing bone. If its parent or grandgrand...parent bone exists in flver scene, it will automatically bind to that bone instead of bind to the root bone.

- Support blender's "CATS" plugin's bone names.(https://github.com/michaeldegroot/cats-blender-plugin/releases)
  Once you click "Fix model" in that blender plugin, it will automatically rename every bones， and FLVER editor can automatically recongize these renamed bones' names and convert them to Sekiro/DS3 style bone names.

- "Mesh->Delete faceset only" functionality. By checking this option, you can delete .hkx file related meshes without disable the .hkx file's cloth physics functionality.

- boneConvertion.ini file can automaticly convert bone names from mixamo rig to sekiro/ds3 rig.

- Mesh->Rev.Mesh and Mesh->Rev.Normal functionalities help you import models that have wrong normals or mirrored models.

## Basic tutorial

1.Double click the MySFformat to start the program, choose the flver file you want to edit.
(Alternate way: drag the flver to the .exe file to auotmatic open the file, or you can set the flver file open method to this .exe file.)

2.You will see two windows. One is FLVER viewer and another is FLVER Bones.

[FLVER viewer]

A window to help you check the model and see the changes you made.

Basic operation:

    -Mouse press and move: rotate your camera

    -mouse middle button: camera panning functionality

    -Mouse scroll: move forward/backward

    -Numpad 2 and 8: Move up/down your camera

    -F1: Render mode: line

    -F2: Render mode: mesh

    -F3: Render mode: both

    -F: Refresh the scene

    -Mouse Right click: check the clicked vertex information.

    -Press B to toggle skeleton display and press M to toggle dummmy display

[FLVER Bones]

This is your main working bench. In this window you can edit some basic bone and header information.

Components in the left pannel:

    -A list of bones and their basic information.

    	Allowing you to edit bone names and their parent/child bone information.
    	Need to click the  "Modify" button in the right panel to save your change.


Components in the right panel:

    -Modify
    	Save your change at the bone list part.

    -Material
    	Open the [material] window.

    -Mesh
    	Open the [mesh] window. So that you can transform meshes of the flver model.

    -Dummy
    	Open the [dummy] window. If you want to fix the sword trail and weapon art issue, click [dummy]->[SekiroFix]
    	This program will automatically fix the issue.(You still need to manually change the dummy points position though.)

    -BufferLayout
    	Check your current FLVER file's buffer layout information. It contains how will the program write things to the flver file.

    -Import Model
    	Import external model files into the flver.
    	After import, you need to click [Modify] to save your result.
    	You may also need to rotate the mesh to fix the axis problem.

