using UnityEngine;

namespace Jake.CameraExtensions
{
	using RenderTextureExtensions;

	public static class CameraExtensions
	{
		/// <summary>
		/// Instantly render camera and return contents as a Texture2D
		/// </summary>
		public static Texture2D Screenshot(this Camera camera)
		{
			// swap target texture
			var prevTarget = camera.targetTexture;
			camera.targetTexture = new RenderTexture(camera.pixelWidth, camera.pixelHeight, 32);

			// render to texture2d
			camera.Render();
			var screenshot = camera.targetTexture.ToTexture2D();

			// restore target texture
			Object.Destroy(camera.targetTexture);
			camera.targetTexture = prevTarget;

			return screenshot;
		}
	}
}
