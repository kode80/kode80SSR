# kode80SSR
An open source (FreeBSD license) screen space reflections implementation for Unity3D 5. Features GPU DDA for screen space ray casting (derived from [Morgan McGuire & Mike Mara's work](http://casual-effects.blogspot.com/2014/08/screen-space-ray-tracing.html)), backface depth buffer for per-pixel geometry thickness, distance attenuated pixel stride, rough/smooth surface reflection blurring, fully customizable for quality/speed and more.  

Read more on implementation: [http://www.kode80.com/blog/2015/03/11/screen-space-reflections-in-unity-5](http://www.kode80.com/blog/2015/03/11/screen-space-reflections-in-unity-5)

# Usage
Add the SSR component to a Camera, see properties section below for details. Requires deferred rendering path and a scene made up of Standard Shader materials.

# Status
Currently tested and optimized for OSX & Windows. Physically correct handling of specular is faked for now.

# Properties
### Downsample:
Each of the 3 main passes can be optionally downsampled to trade quality for performance.
* **Backface Depth:** The backface depth pass is used to calculate the 'thickness' of the current pixel by subtracting the frontface depth value. The frontface depth texture is always full resolution so downsampling the backface depth pass tends to only cause artifacts on object edges. Depending on scene depth range this can potentially be pushed pretty high with little affect on quality.
* **SSR:** Downsampling the SSR ray cast pass drastically reduces the number of rays but obviously results in lower resolution reflections. For scenes with high roughness/low specularity this may be acceptable.
* **Blur:** Downsampling the blur pass will increase the likelihood of halo artifacts but can be used to increase performance if needed.

### Raycast:
For every pixel in the SSR pass a ray is cast. For every step of a ray the frontface and backface depth textures are sampled and ray intersection tests performed. This is the most demanding pass of the shader and so minimizing the number of steps per ray will have the biggest positive impact on performance. Depending on the scene it is possible to get acceptable results with a surprisingly small number of steps (less than 10 steps in some cases).
* **Iterations:** The maximum allowed steps per ray. As this is a screen space ray cast, iterations can be thought of in pixels (iterations x pixel stride = maximum distance a ray can travel in pixels). Thus downsampling the SSR pass reduces the number of iterations required for a ray to travel the same distance in world space.
* **Binary Search Iterations:** The number of iterations of binary search refinement to conduct on the final step of a ray cast. If pixel stride is set to 1 this is ignored. For higher pixel strides this can generally be set to around 1-5 with good results.
* **Pixel Stride:** The number of pixels to jump each step of a ray cast. Higher values will result in less accurate reflections but allow rays to travel further for less iterations.
* **Pixel Stride Z Cuttoff:** Pixel stride is attenuated based on surface distance from camera. This value is the distance from the camera at which point pixel stride becomes 1. For scenes with wide depth range (such as long shots of terrain) this allows for a large pixel stride to be used without affecting distant reflections.
* **Pixel Z Size Offset:** This value is added to the calculated pixel "thickness" and can help mitigate artifacts caused by thin geometry or a downsampled backface depth pass. Generally this should be set to a low 0-1 range but depending on scene/settings may need to be set higher.
* **Max Ray Distance:** The maximum distance a ray can travel in world space.

### Reflection Fading:
Since SSR is a screen space effect, we rely on depth and normal passes for geometry information. This means that any geometry not rendered to the screen can not be reflected. To minimize the obviousness of this deficiency we can detect and smoothly fade these reflections.
* **Screen Edge Fade Start:** A value between 0-1 that controls when reflections who's rays reach the screen edges will start to fade. 0 = center of screen, 1 = edge of screen. Values closer to 1 will result in a more abrupt fade, values closer to 0 will result in smoother fading. Generally 0.75 is a good value.
* **Eye Fade Start:** The ray direction's Z value where reflections begin to fade. Reflections will be smoothly faded between this and the Z value set for Eye Fade End.
* **Eye Fade End:** The ray direction's Z value where reflections are completely transparent.

### Roughness:
Generally speaking, in PBR roughness/smoothness controls how sharp reflections appear. This is faked in the kode80SSR shader by using the roughness channel of the GBuffer to control the per-pixel radius of a bilateral blur pass.
* **Max Blur Radius:** This value is the maximum radius allowed by the blur pass and is multiplied by the per-pixel roughness value from the Unity GBuffer. Setting this to 0 means reflection blurring is disabled regardless of roughness values.
* **Blur Quality:** This value define the quality of the reflection blur. It depend from Max Blur Radius value.
