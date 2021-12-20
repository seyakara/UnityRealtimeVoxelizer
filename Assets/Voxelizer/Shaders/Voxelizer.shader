Shader "Voxelizer"
{
	Properties
	{
		[MainTexture] _BaseMap("Texture", 2D) = "white" {}
		[MainColor] _BaseColor("Color", Color) = (1, 1, 1, 1)
	}

	SubShader
	{
		Tags {"RenderType" = "Opaque" "IgnoreProjector" = "True" "RenderPipeline" = "UniversalPipeline" "ShaderModel" = "5.0"}
		LOD 100

		ZWrite Off
		Cull Off
		ZTest Always
		ColorMask 0

		Pass
		{
			Name "Unlit"

			HLSLPROGRAM
			#pragma target 5.0

			// -------------------------------------
			// Unity defined keywords
			#pragma multi_compile_instancing

			#pragma vertex vert
			#pragma require geometry
			#pragma geometry geom 
			#pragma fragment frag

			#include "Packages/com.unity.render-pipelines.universal/Shaders/UnlitInput.hlsl"

			RWStructuredBuffer<uint> _ColorBuffer : register(u1); 

			float _BlockSize;
			int _GridWidth;
			float _HeightScale;
			float3 _BasePos;

			struct VoxelAttributes
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float2 uv : TEXCOORD0;
			};

			struct VoxelVaryings
			{
				float4 positionCS : SV_POSITION;
				float2 uv		: TEXCOORD0;
				centroid float3 positionWS : TEXCOORD1;
			};

			VoxelAttributes vert(VoxelAttributes input)
			{
				VoxelAttributes output = input;
				output.vertex = mul(unity_ObjectToWorld, input.vertex);
				output.normal = mul((float3x3)UNITY_MATRIX_MV, input.normal);
				output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
				return output;
			}

			[maxvertexcount(3)]
			void geom(triangle VoxelAttributes input[3], inout TriangleStream<VoxelVaryings> outStream)
			{
				const float3	zAxis = float3(0.f, 0.f, 1.f);
				const float3	xAxis = float3(1.f, 0.f, 0.f);
				const float3	yAxis = float3(0.f, 1.f, 0.f);

				// 3つの頂点の法線からポリゴン面法線を求める
				float3 faceNrm = normalize(input[0].normal + input[1].normal + input[2].normal);

				// XYZ各軸との内積をとる
				float dotprduct0 = abs(dot(faceNrm, xAxis));
				float dotprduct1 = abs(dot(faceNrm, yAxis));
				float dotprduct2 = abs(dot(faceNrm, zAxis));

				float maximum = max(max(dotprduct0, dotprduct1), dotprduct2);
				// 0 : ZY
				// 1 : XZ
				// 2 : XY
				int axisIdx = 0;
				if (maximum == dotprduct0)
					axisIdx = 0;
				else if (maximum == dotprduct1)
					axisIdx = 1;
				else
					axisIdx = 2;


				[unroll]
				for (int i = 0; i < 3; i++)
				{
					// ワールド座標をクリッピング座標に変換
					float4 positionCS = TransformWorldToHClip(input[i].vertex.xyz);
					float w = positionCS.w;

					// 一旦正規化する
					positionCS.xyz /= w;

					// Zだけ0～1なので、-1～1にする
					if (UNITY_NEAR_CLIP_VALUE >= 0) {
						positionCS.z = positionCS.z * 2.0 - 1.0;
					}

					// 投影面に応じてxyzを入れ替える
					if (axisIdx == 0)
					{
						positionCS.xyz = positionCS.zyx;
					}
					else if (axisIdx == 1)
					{
						positionCS.xyz = positionCS.xzy;
					}

					// Zを0～1に戻す
					if (UNITY_NEAR_CLIP_VALUE >= 0) {
						positionCS.z = positionCS.z * 0.5 + 0.5;
					}
					positionCS.xyz *= w;

					positionCS.y /= _HeightScale;

					VoxelVaryings o;
					o.positionCS = positionCS;
					o.positionWS = input[i].vertex.xyz;
					o.uv = TRANSFORM_TEX(input[i].uv, _BaseMap);
					outStream.Append(o);
				}

				outStream.RestartStrip();
			}

			half4 frag(VoxelVaryings input) : SV_Target
			{
				// テクスチャカラー取得
				float4 color = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);
				if (color.a < 0.5) {
					discard;
				}
				color *= _BaseColor;

				// カラーのRGBを整数値にしてビットシフト後、uintのカラーマスクに格納
				uint3 iColor = uint3(color.xyz*255.0);
				uint colorMask = (iColor.r << 16u) | (iColor.g << 8u) | iColor.b;

				// RGBから輝度を求める。この輝度はカラーマスクの最上位8ビットに格納
				uint Y = (0.299*color.r + 0.587*color.g + 0.114*color.b) * 255.0;
				colorMask |= max(1, Y) << 24u;

				// ワールド座標からボクセルのグリッド上の位置を求める。
				// ボクセルの大きさの整数倍の位置にスナップさせるだけ。
				int3 grid = (input.positionWS - _BasePos) / float3(_BlockSize, _BlockSize*_HeightScale, _BlockSize);

				if (grid.x >= 0 && grid.x < _GridWidth && grid.y >= 0 && grid.y < _GridWidth && grid.z >= 0 && grid.z < _GridWidth) {
					// グリッド座標を1次元配列のインデックスに変換
					uint grididx = grid.z * _GridWidth * _GridWidth + grid.y * _GridWidth + grid.x;

					// カラー格納用のバッファにカラーマスクを格納
					InterlockedMax(_ColorBuffer[grididx], colorMask);
				}

				return color;
			}
			ENDHLSL
		}
	}
	FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
