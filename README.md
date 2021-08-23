# FPS-Controller-New-Input-System
Unity Rigidbody FPS Controller using the new Input System

**Setup :**

  - In the Unity's Package Manager, go in the Unity Registry and search for "Input System"
  - Import the package and follow the instructions
  - Download the unity package in the release section
  - In your Project "Assets" Folder, right click, select "Import Package" -> "Custom Package..."
  - Import all the assets
  - In the new "RigidbodyFPSController" Folder you can find the "Player Container" Prefab, place it in your scene
  - Create a new layer to represent the Ground (surfaces where you can move on)
  - Set your terrain to your Ground Layer
  - In the "Player Container" select "Player", in the "Player Movement" Script, set the "Ground Mask" to your Ground Layer

**Notes :**

  - In the "Scripts" Folder of the Package, you can find the "PlayerInputActions" Action Map to edit the keybinds
