# 3D-Render-engine-for-CSharp-winforms
An incomplete project attempting to simulate 3D in a winforms application, I plan to highly optimise it in the future.
# This Build has **NOT** been tested.<br><br><br>
## <ins>Contents</ins>:
* **[Added features](#added-features)**
* **[Features to be added](#features-to-be-added)**
* **[Unfinished features](#unfinished-features)**
* **[Removed features](#removed-features)**
* **[Tested?](#has-this-feature-been-tested)**
<br><br><br>

## <ins>Added features:</ins>
* **[Head back?](#contents)**
- ### Rendering of 3-Dimensional meshes:
    As is in the name of this repository, this program can render 3-Dimensional mesh objects, 
    these meshes are rendered on a Windows Form and viewed through a Camera.
* ### Key press event subscribing:
    A custom event system where functions can be subscribed to specific keys.
    * ### Joystick support:
        Joystick support that functions similarly to the **[Keypress event subscribing](#key-press-event-subscribing)**
* ### Texture loading:
    Textures can be loaded onto meshes, this occurs in the **[Texturer class](Framework/Components.cs)** with support from some **[custom Databases](/Framework/Databases.cs)** and **[Writeable Bitmap](/Framework/WriteableBitmap.cs)**, textures are stretched to fit on a per-Polygon basis.
* ### Basic Physics engine:
    A basic Physics engine, not fully implemented not good at all.
* ### Components:
    Game objects can have components that will add to them <ins>e.g Texturing, Physics e.t.c</ins>
* ### Deadlock prevention:
    A class **[BackdoorJob](/Framework/Miscallaneous.cs)** has been implemented to prevent deadlocks by stopping themselves after a certain amount of time,
    it builds off of **Task.Run(), Reflection** and other libraries to do this.
<br><br><br>

## <ins>Unfinished features:</ins>
* **[Head back?](#contents)**
* Extension loading, will be re-added, hopefully.
<br><br><br>

## <ins>Features to be added:</ins>
* **[Head back?](#contents)**
[N] Extension loading.

## <ins>Has this feature been tested?</ins>
* **[Head back?](#contents)**
- [ ] **[3-Dimensional meshes rendering](#rendering-of-3-dimensional-meshes).**

- [ ] **[Key pressing events](#key-press-event-subscribing).**

- [ ] **[Key pressing events/Joystick support](#joystick-support).**

- [ ] **[Texture Loading](#texture-loading).**

- [X] **[Components](#components).**

- [X] **[Deadlock prevention](#deadlock-prevention).**