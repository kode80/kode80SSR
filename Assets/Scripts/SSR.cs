//  Copyright (c) 2015, Ben Hopkins (kode80)
//  All rights reserved.
//  
//  Redistribution and use in source and binary forms, with or without modification, 
//  are permitted provided that the following conditions are met:
//  
//  1. Redistributions of source code must retain the above copyright notice, 
//     this list of conditions and the following disclaimer.
//  
//  2. Redistributions in binary form must reproduce the above copyright notice, 
//     this list of conditions and the following disclaimer in the documentation 
//     and/or other materials provided with the distribution.
//  
//  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY 
//  EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF 
//  MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL 
//  THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, 
//  SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT 
//  OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) 
//  HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT 
//  (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, 
//  EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.


using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]

public class SSR : MonoBehaviour 
{
	[Header("Downsample:")]
	[Range( 0.0f, 8.0f)]
	public int backfaceDepthDownsample = 0;
	[Range( 0.0f, 8.0f)]
	public int ssrDownsample = 0;
	[Range( 0.0f, 8.0f)]
	public int blurDownsample = 0;
	
	[Header("Raycast:")]
	[Range( 1.0f, 300.0f)]
	public int iterations = 20;
	[Range( 0.0f, 32.0f)]
	public int binarySearchIterations;
	[Range( 1.0f, 64.0f)]
	public int pixelStride = 1;
	public float pixelStrideZCutoff = 100.0f;
	public float pixelZSizeOffset = 0.1f;
	public float maxRayDistance = 10.0f;
	
	[Header("Reflection Fading:")]
	[Range( 0.0f, 1.0f)]
	public float screenEdgeFadeStart = 0.75f;
	[Range( 0.0f, 1.0f)]
	public float eyeFadeStart = 0.0f;
	[Range( 0.0f, 1.0f)]
	public float eyeFadeEnd = 1.0f;
	
	[Header("Roughness:")]
	[Range( 0.0f, 16.0f)]
	public int maxBlurRadius = 8;

	[Header("Blur Quality:")]
	[Range( 2.0f, 4.0f)]
	public int BlurQuality = 2;
	
	private Shader _backfaceDepthShader;
	private Material _ssrMaterial;
	private Material _blurMaterial;
	private Material _combinerMaterial;
	private RenderTexture _backFaceDepthTexture;
	private Camera _camera;
	private Camera _backFaceCamera;
	
	void OnEnable()
	{
		CreateMaterialsIfNeeded();
		_camera.depthTextureMode |= DepthTextureMode.Depth;
	}
	
	void OnDisable()
	{
		DestroyMaterials();
		
		if( _backFaceCamera)
		{
			DestroyImmediate( _backFaceCamera.gameObject);
			_backFaceCamera = null;
		}
	}

	void Reset()
	{
		_camera = GetComponent<Camera>();
	}
	
	void Start () 
	{
		CreateMaterialsIfNeeded();
	}

	void Awake()
	{
		_camera = GetComponent<Camera>();
	}
	
	void OnPreCull()
	{
		int downsampleBackFaceDepth = backfaceDepthDownsample + 1;
		int width = _camera.pixelWidth / downsampleBackFaceDepth;
		int height = _camera.pixelHeight / downsampleBackFaceDepth;
		_backFaceDepthTexture = RenderTexture.GetTemporary( width,  
		                                                   height,   
		                                                   16, 
		                                                   RenderTextureFormat.RFloat);
		
		if( _backFaceCamera == null)
		{
			GameObject cameraGameObject = new GameObject( "BackFaceDepthCamera");
			cameraGameObject.hideFlags = HideFlags.HideAndDontSave;
			_backFaceCamera = cameraGameObject.AddComponent<Camera>();
		}
		
		_backFaceCamera.CopyFrom( _camera);
		_backFaceCamera.renderingPath = RenderingPath.Forward;
		_backFaceCamera.enabled = false;
		_backFaceCamera.SetReplacementShader( Shader.Find( "kode80/BackFaceDepth"), null); 
		_backFaceCamera.backgroundColor = new Color( 1.0f, 1.0f, 1.0f, 1.0f);
		_backFaceCamera.clearFlags = CameraClearFlags.SolidColor;
		//_backFaceCamera.cullingMask = LayerMask.GetMask( "Everything");
		
		_backFaceCamera.targetTexture = _backFaceDepthTexture;
		_backFaceCamera.Render();
	}
	
	[ImageEffectOpaque]
	void OnRenderImage( RenderTexture source, RenderTexture destination)
	{
		CreateMaterialsIfNeeded();
		UpdateMaterialsPublicProperties();
		
		int downsampleSSR = ssrDownsample + 1;
		int dsSSRWidth = source.width / downsampleSSR;
		int dsSSRHeight = source.height / downsampleSSR;
		
		bool debugDepth = false;
		if( debugDepth)
		{
			Graphics.Blit( _backFaceDepthTexture, destination);
			RenderTexture.ReleaseTemporary( _backFaceDepthTexture);
			
			return;
		}
		
		FilterMode filterMode = FilterMode.Trilinear;
		
		RenderTexture rtSSR;
		if( _camera.hdr)
		{
			rtSSR = RenderTexture.GetTemporary( dsSSRWidth, dsSSRHeight, 0, RenderTextureFormat.DefaultHDR);
		}
		else
		{
			rtSSR = RenderTexture.GetTemporary( dsSSRWidth, dsSSRHeight, 0, RenderTextureFormat.Default);
		}
		
		rtSSR.filterMode = filterMode;
		
		if( _backFaceDepthTexture)
		{
			_ssrMaterial.SetTexture( "_BackFaceDepthTex", _backFaceDepthTexture);
		}
		Graphics.Blit( source, rtSSR, _ssrMaterial);
		
		if( maxBlurRadius > 0)
		{
			int downsampleBlur = blurDownsample + 1;
			int dsBlurWidth = source.width / downsampleBlur;
			int dsBlurHeight = source.height / downsampleBlur;
			
			RenderTexture rtBlurX = null;
			if( _camera.hdr)
			{
				rtBlurX = RenderTexture.GetTemporary( dsBlurWidth, dsBlurHeight, 0, RenderTextureFormat.DefaultHDR);
			}
			else
			{
				rtBlurX = RenderTexture.GetTemporary( dsBlurWidth, dsBlurHeight, 0, RenderTextureFormat.Default);
			}
			
			rtBlurX.filterMode = filterMode;
			_blurMaterial.SetVector( "_TexelOffsetScale",
			                        new Vector4 ((float)maxBlurRadius / source.width, 0,0,0));
			Graphics.Blit( rtSSR, rtBlurX, _blurMaterial);
			
			RenderTexture rtBlurY;
			if( _camera.hdr)
			{
				rtBlurY = RenderTexture.GetTemporary( dsBlurWidth, dsBlurHeight, 0, RenderTextureFormat.DefaultHDR);
			}
			else
			{
				rtBlurY = RenderTexture.GetTemporary( dsBlurWidth, dsBlurHeight, 0, RenderTextureFormat.Default);
			}
			
			rtBlurY.filterMode = filterMode;
			_blurMaterial.SetVector( "_TexelOffsetScale",
			                        new Vector4( 0, (float)maxBlurRadius/source.height, 0,0));
			Graphics.Blit( rtBlurX, rtBlurY, _blurMaterial);
			
			RenderTexture.ReleaseTemporary( rtBlurX);
			RenderTexture.ReleaseTemporary( rtSSR);
			
			rtSSR = rtBlurY;
		}
		
		_combinerMaterial.SetTexture( "_BlurTex", rtSSR);
		Graphics.Blit( source, destination, _combinerMaterial);
		
		RenderTexture.ReleaseTemporary( rtSSR);
		RenderTexture.ReleaseTemporary( _backFaceDepthTexture);
	}
	
	private void CreateMaterialsIfNeeded()
	{
		if( _ssrMaterial == null)
		{
			_ssrMaterial = new Material( Shader.Find( "kode80/SSR"));
			_ssrMaterial.hideFlags = HideFlags.HideAndDontSave;
		}
		
		if( _blurMaterial == null)
		{
			_blurMaterial = new Material( Shader.Find( "kode80/BilateralBlur"));
			_blurMaterial.hideFlags = HideFlags.HideAndDontSave;
		}
		
		if( _combinerMaterial == null)
		{
			_combinerMaterial = new Material( Shader.Find( "kode80/SSRBlurCombiner"));
			_combinerMaterial.hideFlags = HideFlags.HideAndDontSave;
		}
	}
	
	private void DestroyMaterials()
	{
		DestroyImmediate( _ssrMaterial);
		_ssrMaterial = null;
		
		DestroyImmediate( _blurMaterial);
		_blurMaterial = null;
		
		DestroyImmediate( _combinerMaterial);
		_combinerMaterial = null;
	}
	
	private void UpdateMaterialsPublicProperties()
	{
		if( _ssrMaterial)
		{
			_ssrMaterial.SetFloat( "_PixelStride", pixelStride);
			_ssrMaterial.SetFloat( "_PixelStrideZCuttoff", pixelStrideZCutoff);
			_ssrMaterial.SetFloat( "_PixelZSize", pixelZSizeOffset);
			
			_ssrMaterial.SetFloat( "_Iterations", iterations);
			_ssrMaterial.SetFloat( "_BinarySearchIterations", binarySearchIterations);
			_ssrMaterial.SetFloat( "_MaxRayDistance", maxRayDistance);
			_blurMaterial.SetFloat( "_BlurQuality", BlurQuality);
			_ssrMaterial.SetFloat( "_ScreenEdgeFadeStart", screenEdgeFadeStart);
			_ssrMaterial.SetFloat( "_EyeFadeStart", eyeFadeStart);
			_ssrMaterial.SetFloat( "_EyeFadeEnd", eyeFadeEnd);
			
			int downsampleSSR = ssrDownsample + 1;
			int width = _camera.pixelWidth / downsampleSSR;
			int height = _camera.pixelHeight / downsampleSSR;
			Matrix4x4 trs = Matrix4x4.TRS( new Vector3( 0.5f, 0.5f, 0.0f), Quaternion.identity, new Vector3( 0.5f, 0.5f, 1.0f));
			Matrix4x4 scrScale = Matrix4x4.Scale( new Vector3( width, height, 1.0f));
			Matrix4x4 projection = _camera.projectionMatrix;
			
			Matrix4x4 m = scrScale * trs * projection;
			
			_ssrMaterial.SetVector( "_RenderBufferSize", new Vector4( width, height, 0.0f, 0.0f));
			_ssrMaterial.SetVector( "_OneDividedByRenderBufferSize", new Vector4( 1.0f / width, 1.0f / height, 0.0f, 0.0f));
			_ssrMaterial.SetMatrix( "_CameraProjectionMatrix", m);
			_ssrMaterial.SetMatrix( "_CameraInverseProjectionMatrix", projection.inverse);
			_ssrMaterial.SetMatrix( "_NormalMatrix", _camera.worldToCameraMatrix);
		}
		
		if( _blurMaterial)
		{
			_blurMaterial.SetFloat( "_DepthBias", 0.305f);
			_blurMaterial.SetFloat( "_NormalBias", 0.29f);
		}
	}
}
