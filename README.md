# Texture.com-packer
Simple tool to pack Textures.com (or other source) grayscale PBR textures into a Unity-friendly format.

![image](https://user-images.githubusercontent.com/114744494/216590169-edbbf31f-95f2-457f-b96d-63eb356e8458.png)

The format is:
 - _AlbedoTransparency: RGB = base color, A = transparency
 - _MetallicOcclusionSmoothness: R = metallic, G = occlusion, B = 0, A = smoothness (1 - roughness)
 - _Normal: RGB = normals (no change made, the texture just gets moved and renamed with the others for convenience)

Since Unity's standard shaders use the green channel of the Occlusion map, the MetallicOcclusionSmoothness texture can be used with the standard shader as both the Metallic map (with smoothness from alpha) and the Occlusion map.

Where a texture is not provided, it will be replaced with R = 0, G = 0, B = 0, A = 1 - i.e. opaque black. This means that you can, for example, create a completely metallic material by not choosing a metallic texture and selecting the A channel. Note that this means that if you do not provide an occlusion texture you should always set the channel to A.

## Limitations

Due to the limitations of the use of Texture2D.LoadImage():
 - May not handle sRGB correctly - in particular the smoothness channel, since it is inverted from the input roughness, may not be the ground truth.
 - Only accepts .PNG or .JPEG as input.
 
Due to the limitations of me making this very quickly and keeping it extremely simple:
 - All textures which will be packed into a single texture (i.e. base color/transparency, metallic/occlusion/roughness) must be matching dimensions.
