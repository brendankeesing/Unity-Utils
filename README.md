# Unity Utils

These are a bunch of generic unity tools that I have accumulated over the years. Feel free to use anything here however you want.

## Collision Events

By default, Unity has no way to be notified when a Collider or trigger has been hit unless your component is on the same GameObject as the Collider. The CollisionEvents component will intercept all collision events and pass it on to any listeners.

## Decals

Projects an image onto a surface by generating a mesh. This does not require deferred rendering and can be set to affect only certain layers. It's also slow to generate, so only use for static meshes.

## Icon Generator

Select a Camera object in the scene, specify the out resolution, click a button. Done.

## Line Path

Generates a high-performance loop-able path made up of lines. There is an option to smooth out the line to give it spline-like properties.
LinePathMesh will generate a mesh along the path. This is handy for making pipes or billboards.
LinePathObjectPlacer will place prefabs along the path. Useful for creating tree lines
PathUtils contains a bunch of functions for animating objects along a path.

## Mesh Placer

Scatters prefabs across the surface of a mesh with somewhat even distribution.

## Object Painter

An editor tool that lets you paint prefabs onto surfaces. Useful for making grass.

## Shader Replacer

Replaces all instances of a shader with another shader, for all materials in the project. This was super handy when switching between Legacy/URP/HDRP.

## Simple Gradient

Unity's Gradients have a limitation of 8 colors. If this is too limiting, SimpleGradient let's you have unlimited colors. It's a lot more clunky to use though.

## UI

A bunch of UI mesh generators:
* ApertureUI: A very customizable camera aperture.
* BarChartUI: Given an array of values, it will generate a basic bar chart of the values.
* BorderUI: Draws a box outline around the RectTransform.
* MeshUI: Takes a 3D mesh and renders it in UI space.
* RingUI: A circle with a smaller circle cut out in the center.
* RotateUVs: Add this to the same GameObject with a Graphic component and you can rotate the texture coordinates on the mesh.
* SemiCircleUI: Generates only part of a circle.

## Utils

Too many goodies here to list, so here are some highlights:
* CompressionUtils: Easy functions for compressing/decompressing bytes and strings.
* MeshBuilder: A procedural mesh friendly mesh generation class.
* ObjectPool: Simple object pool that doesn't try to do everything under the sun. 
* OBJExporter: For when you want to get a procedural mesh out of the project. Works at runtime too.
* TweenUtils: Simple tween functions that doesn't try to do everything under the sun. 
