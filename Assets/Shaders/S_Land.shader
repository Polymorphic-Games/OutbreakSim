Shader "Landon/Land" {
    Properties {
        _MainTex ("Color (RGB) Lights (A)", 2D) = "gray" {}
		_Tex2 ("Color (RGB) Lights (A)", 2D) = "white" {}
		[Normal] _BumpMap ("Bumpmap", 2D) = "bump" {}

		[HDR]_GlowColor ("Glow Color", Color) = (1,1,1,1)
		[HDR]_FresColor ("Fresnel Color", Color) = (1,1,1,1)

		//[NoScaleOffset]
		_Data ("Data (rgba)", 2D) = "grey" {}

		[HDR] _C1 ("Data 1 channel r Color (rgb) Intensity (a)", Color) = (1,0,0,1)
		[HDR] _C2 ("Data 1 channel g Color (rgb) Intensity (a)", Color) = (0.5,0.5,0.5,0)
		[HDR] _C3 ("Data 1 channel b Color (rgb) Intensity (a)", Color) = (0.5,0.5,0.5,0)
		[HDR] _C4 ("Data 1 channel a Color (rgb) Intensity (a)", Color) = (0.5,0.5,0.5,0)
    }
    SubShader {
        Tags { "RenderType"="Opaque" }
		//Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Lambert vertex:vert addshadow //fullforwardshadows
        #pragma target 3.0

        sampler2D _MainTex, _Tex2, _BumpMap, _Data;

        struct Input {
            float2 uv_MainTex;
			float2 uv_Data;
            half3 viewDir;
			float3 localPos;
			float3 vertexNormal;
        };

		void vert (inout appdata_full v, out Input o) {
		   UNITY_INITIALIZE_OUTPUT(Input,o);
		   o.localPos = v.vertex.xyz;
		   o.vertexNormal = v.normal;
		}

        float4 _FresColor,_GlowColor;
		half4 _C1, _C2, _C3, _C4;


        void surf (Input IN, inout SurfaceOutput o) {
			float4 main = tex2D (_MainTex, IN.uv_MainTex);
			float4 tex2 = tex2D (_Tex2, IN.uv_MainTex);
			float4 data = tex2D (_Data, IN.uv_Data);

			//color mask areas
			fixed c1 = data.r * _C1.a;
			fixed c2 = data.g * _C2.a;
			fixed c3 = data.b * _C3.a;
			fixed c4 = data.a * _C4.a;
			fixed mask = saturate(c1 + c2 + c3 + c4);
			//add colors
			fixed3 sum = c1 * _C1.rgb + c2 * _C2.rgb + c3 * _C3.rgb + c4 * _C4.rgb;
			//apply colors
			o.Albedo = main.rgb * (1 - mask) + sum;
			o.Normal = UnpackNormal (tex2D (_BumpMap, IN.uv_MainTex));

			half rim = 1.0 - saturate(dot (normalize(IN.viewDir), o.Normal));
			//half rim = 1.0 - saturate(dot (normalize(IN.viewDir), normalize(IN.localPos)));
			half3 fres = _FresColor.rgb * pow (rim, 5 * _FresColor.a + 0.5);

			//half light = saturate(dot (_WorldSpaceLightPos0, IN.vertexNormal));

			half lightPos = 1 - dot (_WorldSpaceLightPos0, normalize(IN.localPos));
			half lightNorm = 1 - dot (_WorldSpaceLightPos0, IN.vertexNormal);
			
			//half light = saturate(lerp(lightPos,lightNorm,_LightsType)*_LightsSharpness-_LightsSharpness);
			half light = saturate((lightPos + lightNorm)*3-4);

			o.Emission = fres * light + light * (1 - main.a) * _GlowColor * (1-saturate(mask-sum.b));


			clip (0.3 - tex2.r);

            //o.Alpha = c.a;
			//multiply colors
			//fixed3 c1m = saturate(1-c1 + _C1.rgb);
        }
        ENDCG
    }
	FallBack "Legacy Shaders/Transparent/Cutout/Diffuse"
    //FallBack "Diffuse"
}