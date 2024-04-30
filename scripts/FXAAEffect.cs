using System.Collections;
using UnityEngine;

namespace The_Legend_of_Bum_bo_Windfall
{
	public class FXAAEffect : MonoBehaviour
	{
		const int luminancePass = 0;
		const int fxaaPass = 1;

		public enum LuminanceMode { Alpha, Green, Calculate }

		public LuminanceMode luminanceSource;

		[Range(0.0312f, 0.0833f)]
		public float contrastThreshold = 0.0312f;

		[Range(0.063f, 0.333f)]
		public float relativeThreshold = 0.063f;

		[Range(0f, 1f)]
		public float subpixelBlending = 1f;

		[HideInInspector]
		public Shader fxaaShader;

		public bool lowQuality;

		public bool gammaBlending;

		Material fxaaMaterial;

		void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			if (fxaaMaterial == null)
			{
				fxaaMaterial = new Material(fxaaShader);
				fxaaMaterial.hideFlags = HideFlags.HideAndDontSave;
			}

			fxaaMaterial.SetFloat("_ContrastThreshold", contrastThreshold);
			fxaaMaterial.SetFloat("_RelativeThreshold", relativeThreshold);
			fxaaMaterial.SetFloat("_SubpixelBlending", subpixelBlending);

			if (lowQuality)
			{
				fxaaMaterial.EnableKeyword("LOW_QUALITY");
			}
			else
			{
				fxaaMaterial.DisableKeyword("LOW_QUALITY");
			}

			if (gammaBlending)
			{
				fxaaMaterial.EnableKeyword("GAMMA_BLENDING");
			}
			else
			{
				fxaaMaterial.DisableKeyword("GAMMA_BLENDING");
			}

			if (luminanceSource == LuminanceMode.Calculate)
			{
				fxaaMaterial.DisableKeyword("LUMINANCE_GREEN");
				RenderTexture luminanceTex = RenderTexture.GetTemporary(
					source.width, source.height, 0, source.format
				);
				Graphics.Blit(source, luminanceTex, fxaaMaterial, luminancePass);
				Graphics.Blit(luminanceTex, destination, fxaaMaterial, fxaaPass);
				RenderTexture.ReleaseTemporary(luminanceTex);
			}
			else
			{
				if (luminanceSource == LuminanceMode.Green)
				{
					fxaaMaterial.EnableKeyword("LUMINANCE_GREEN");
				}
				else
				{
					fxaaMaterial.DisableKeyword("LUMINANCE_GREEN");
				}
				Graphics.Blit(source, destination, fxaaMaterial, fxaaPass);
			}
		}
	}
}