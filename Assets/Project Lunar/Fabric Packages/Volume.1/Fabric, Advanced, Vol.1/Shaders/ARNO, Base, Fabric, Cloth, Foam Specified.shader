// COPYRIGHTS JEREMY YU GONG, SHADER SMART, 2018
// 21 February 2018
// Shader Smart | Project Lana
// Jeremy Y. Gong 

//----	Project Lana: Fabric, Cloth, and Foram Specified, ARNO Base Version. 1.00 ----

// Introducing: 
// 1) Albedo (A)
// 2) Roughness (R)
// 3) Normal (N)
// 4) Ambient Occlusion (O)
// 5) Dirt Pattern is introduces Bias Value to Base Color (Albedo), Specular, Roughness, Normal, and Ambient Occlusion
// 6) Alpha Mask (Transparency) is introduced, a normalization function is attached along with the fine-tuning options
// 7) A secondary Normal map is introduced for Macros 
// 8) Secondary Normal fall off at far field
// 9) Base Color would reduce to a mono color at far field
// 10) Normalization is applied to both Dirt pattern and Alpha Mask
// 11) Desaturation is introduced to the Dirt Color Bias for the Base Color term


Shader "Shader Smart/Fabric | Cloth | Foam/Base/ARNO" {
	
	Properties {
		
		//----- Albedo and Texture Coordinates  ----
		
		_MainTex ("Albedo: Map", 2D) = "white" {}
		_Rotation ("Texture Rotation", Range(0, 360)) = 0.0
		_Color ("Albedo: Color Bias", Color) = (1,1,1,1)
		_MainCon ("Albedo: Contrast", Range(0,4)) = 1.0
		_MainBgt ("Albedo: Brightness", Range(0,4)) = 1.0

		//---- Dirt Pattern ----
		
		//Dirt UV is individually registered
		_DirtTex ("Surface Dirt: Map", 2D) = "white" {}
		_DirtGray ("Surface Dirt: Gray Level", Range(0,4)) = 1.0 
		_Dirt1 ("Surface Dirt: White Channel", Range(-1,1)) = 1.0
		_Dirt0 ("Surface Dirt: Black Channel", Range(-1,1)) = 0.0
		//Macro Adjustment
		_DirtPre ("Surface Dirt: Color Desaturation Precentage", Range(0,100)) = 20.0
		_DirtCol ("Surface Dirt: Dirt Color Bias", Color) = (1,1,1,1)
		_DirtSpec ("Surface Dirt: Specular Bias", Range(-1,1)) = 0.1
		_DirtNorm ("Surface Dirt: Normal Bias", Range(-1,1)) = 0.1
		_DirtOcc ("Surface Dirt: Ambient Occlusion Bias", Range(0,1)) = 0.75

		//---- Alpha Mask ----

		//Transparency UV is individually registered
		_TransTex ("Transparency: Mask", 2D) = "white" {}
		_TransGray ("Transparency: Gray Level", Range(0,4)) = 1.0
		_Trans1 ("Transparency: White Channel", Range(0,1)) = 1.0
		_Trans0 ("Transparency: Black Channel", Range(0,1)) = 0.0
		_TransThre ("Transparency: Threshold Value", Range(0,1)) = 0.5

		//----- Roughness -----

		[NoScaleOffset] _RghTex ("Roughness: Map", 2D) = "black" {}
		_RghGray ("Roughness: Gray Level Contrast", Range(0,4)) = 1.0
		_Rgh1 ("Roughness: White Channel Strength", Range(-1,1)) = 1.0
		_Rgh0 ("Roughness: Black Channel Strength", Range(-1,1)) = 0.0
		_RghAmp ("Roughness: Strength", Range(-4,4)) = 1.0

		//----- Normal -----

		//Primary Normal
		[NoScaleOffset] _NormTex ("Normal: Map", 2D) = "bump" {}
		_Norm ("Normal: Strength", Range(-2,2)) = 1.0
		_NormBas ("Normal: Bias", Range(-1, 1)) = 0.0
		//Secondary Normal, and its UV is individually registered
		_NormTex2 ("Normal: Secpndary Map", 2D) = "bump" {}
		_Norm2 ("Normal: Secondary Strength", Range(-2,2)) = 0.0
		_NormRati ("Normal: Secondary to Primary Bias Ratio", Range(0,2)) = 0.0

		//---- Far Field Fall Off ----

		_FarDec ("Far Field: Decay Distance", Range(0, 500)) = 5.0
		_FarVan ("Far Field: Vanish Distance", Range(0, 2000)) = 45.0
		_FarCol ("Far Field: Base Color Reduction", Color) = (1,1,1,1)

		//---- Ambient Occlusion ----

		[NoScaleOffset] _OccTex ("Occlusion: Map", 2D) = "white" {}
		_OccCon ("Occlusion: Gray Level Strength", Range(0,4)) = 1.0
		_OccMin ("Occlusion: Base", Range(0, 1)) = 0.0

		//---- Additions ----

	}

	SubShader {
		Tags { "Queue"="Transparent" "RenderType"="Transparent" }
		LOD 300

		CGPROGRAM

		#pragma surface surf StandardSpecular alpha addshadow fullforwardshadows vertex:vert
		#pragma target 3.0

		//---- Texture Varibles ----

		sampler2D _MainTex;
		sampler2D _DirtTex;
		sampler2D _TransTex;
		sampler2D _RghTex;
		sampler2D _NormTex;
		sampler2D _NormTex2;
		sampler2D _OccTex;

		//---- Varibles ----

		//Albedo 
		fixed4 _Color;
		half _MainCon;
		half _MainBgt;
		//Surface Dirt
		half _DirtGray;
		half _Dirt1; 
		half _Dirt0;
		//Surface Dirt: Detail Control
		half _DirtPre;
		fixed4 _DirtCol;
		half _DirtRgh; 
		half _DirtNorm; 
		half _DirtOcc; 
		//Transparency
		half _TransGray;
		half _Trans1;
		half _Trans0;
		half _TransThre;
		//Roughness
		half _Rgh1;
		half _Rgh0;
		half _RghAmp;
		half _RghGray;
		//Normal
		half _Norm;
		half _NormBas;
		half _Norm2;
		half _NormRati;
		//Far Field Fall off
		half _FarDec;
		half _FarVan;
		fixed4 _FarCol;
		//(Ambient) Occlusion
		half _OccCon;
		half _OccMin;
		//UV Texture Coordinates
		half _Rotation;

		struct Input {

			half2 uv_MainTex;
			half2 uv_DirtTex;
			half2 uv_TransTex;
			half2 uv_NormTex2;
			half3 worldPos;
			half3 WorldSpaceCameraPos;
		
		};

	//	UNITY_INSTANCING_BUFFER_START(Props)
	//	UNITY_INSTANCING_BUFFER_END(Props)

		void vert (inout appdata_full v) {

		    v.texcoord.xy -= 0.5;
            
			half ang = _Rotation * 0.0174;
		    half sinX = sin (ang);
            half cosX = cos (ang);
            half sinY = sin (ang);
            half2x2 rotationMatrix = half2x2( cosX, -sinX, sinY, cosX);
            v.texcoord.xy = mul ( v.texcoord.xy, rotationMatrix );
            v.texcoord.xy += 0.5;

        }

		void surf (Input IN, inout SurfaceOutputStandardSpecular o) {

			//---- Falloff Distance and Lerp Base ----

			half f0 = distance (IN.worldPos, _WorldSpaceCameraPos);
			half f1 = ( f0 - _FarDec ) / ( _FarVan - _FarDec );
			half f = clamp ( f1, 0, 1);

			//---- Surface Dirt Evaluation ----

			half d0 = tex2D (_DirtTex, IN.uv_DirtTex).r;
			half d1 = pow (d0, _DirtGray);
			half dd = lerp ( _Dirt0, _Dirt1, d1 );
	
			//Dirt Reduction at Far Field
			half d = lerp ( dd, 1, f ); //Use d for built-in Far Field pattern

			//---- Transparency Mask

			half t0 = tex2D (_TransTex, IN.uv_TransTex).r;
			half t1 = pow (t0, _TransGray);
			half t2 = lerp ( _Trans0, _Trans1, t1 );
			half t = ceil (t2 - _TransThre);

			o.Alpha = t;

			//---- Albedo Evaluation ----
			
			//Base Color Assignment
			fixed3 c0 = tex2D (_MainTex, IN.uv_MainTex);
			fixed3 c1 = pow (c0, _MainCon);
			fixed3 c2 = c1 * _Color.rgb;
			fixed3 c3 = c2 * _MainBgt;
			//Applying Dirt Pattern to Base Color 
			fixed3 cd0 = c3;
			fixed3 cd1 = _DirtPre/100 * ( dot(half3(1,1,1), cd0) )/sqrt(3.0) * _DirtCol.rgb + (1 - _DirtPre/100) * cd0;
			fixed3 c4 = lerp ( cd1, c3, dd );
			fixed3 c = lerp ( c4, c1 * _FarCol * _MainBgt, f );

			o.Albedo = c;

			//---- Roughness Evaluation ----

			half r0 = tex2D (_RghTex, IN.uv_MainTex).r;
			half r1 = pow (r0, _RghGray);
			half r2 = lerp (_Rgh0, _Rgh1, r1);
			half r3 = (1 - r2) * _RghAmp; 
			//Applying Dirt Pattern to Roughness
			half r = lerp ( r3 + (1-_DirtRgh), r3, dd );

			o.Smoothness = r;

			//---- Normal Evaluation ----

			//Primary Evaluation
			half3 n10 = UnpackNormal (tex2D (_NormTex, IN.uv_MainTex));
			half3 n11 = lerp (n10, half3 (0,0,1), (1 - _Norm));
			//Secondaru Evaluation 
			half3 n20 = UnpackNormal (tex2D (_NormTex2, IN.uv_NormTex2));
			half3 n21 = lerp (n20, half3 (0,0,1), (1 - _Norm2));
			//Evantual Evaluation 
			half3 n1 = n11 + _NormRati * n21 + _NormBas * half3 (0,0,1);
			//Applying Dirt Pattern to Normal with built-in Far Field Fall off
			half3 n0 = lerp (n1 + _DirtNorm * half3 (0,0,1), n1, d);
			//Finalized Normal
			half3 n =  normalize (n0);

			o.Normal =  n;

			//---- Ambient Occlusion Evaluation ----

			half ao0 = tex2D (_OccTex, IN.uv_MainTex);
			half ao1 = pow (ao0, _OccCon);
			half ao3 = lerp (_OccMin, 1, ao1);
			//Applying Dirt Pattern to Ambient Occlusion
			half ao = lerp ( _DirtOcc, ao3, d );

			o.Occlusion = ao;

			//---- Additional Evaluation ----

		}
		ENDCG
	}
	FallBack "Diffuse"
}
