using UnityEngine;

namespace Jake.RenderTextureExtensions
{
	public static class RenderTextureExtensions
	{
		public static Texture2D ToTexture2D(this RenderTexture renderTexture)
		{
			// swap render textures
			var prevActive = RenderTexture.active;
			RenderTexture.active = renderTexture;

			// read render texture
			var texture2d = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false);
			texture2d.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);

			// restore render texture
			RenderTexture.active = prevActive;

			return texture2d;
		}
	}
}
