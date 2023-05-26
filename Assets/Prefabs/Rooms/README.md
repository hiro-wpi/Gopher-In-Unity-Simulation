## Modular Design in Unity

This package provides modular walls, doors, windows, etc., for quickly building and customizing you own testing environment. 

However, Unity's baked light mode does not support modular walls well. One common issue is color difference and visible seams between modular walls. There seems to be no direct solutions to this problem (refer to [thread1](https://forum.unity.com/threads/plm-seams-in-modular-walls.804270/) and [thread2](https://forum.unity.com/threads/unity-baked-gi-bleeding-artefact-with-modular-scene.1289039/)). Here is two recommended workaround:

- Instead of baking lights for the walls, use real-time lighting. Real-time lighting works fine with modular components with one major limitation, i.e., computational cost. This workaround is recommended when the environment scale is small.
- Use other 3D modeling software (e.g. Blender) to build the rooms by merging the modular walls. Or use Unity ProGrid (as mentioned in [thread2](https://forum.unity.com/threads/unity-baked-gi-bleeding-artefact-with-modular-scene.1289039/)) to merge the game objects. The key idea is to combine all the modular walls into one single mesh.

In this package, we use workaround 1 for the **Test Small** scene and workaround 2 for the large-scale **Hospital** scene.