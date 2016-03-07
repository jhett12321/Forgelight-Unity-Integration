Shader "Custom/ForgelightModel" {
    Properties {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _BumpMap("Bump Map", 2D) = "bump" {}
        _PackedSpecular("Packed Specular", 2D) = "black" {}

        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0
    }
    SubShader {
        Tags {"Queue"="AlphaTest" "RenderType"="TransparentCutout" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows alphatest:_Cutoff addshadow

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _BumpMap;
        sampler2D _PackedSpecular;

        struct Input {
            float2 uv_MainTex;
        };

        fixed4 _Color;

        void surf (Input IN, inout SurfaceOutputStandard o) {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            fixed4 n = tex2D(_BumpMap, IN.uv_MainTex);
            fixed4 ps = tex2D(_PackedSpecular, IN.uv_MainTex);

            //Diffuse
            o.Albedo = c.rgb;

            //Normal Map
            o.Normal = UnpackNormal(n);

            //Packed Specular:
            //R = metallic, B = emission, G = gloss, A = smoothness
            o.Metallic = ps.r;

            //o.Emission = ps.b;

            if(ps.a > 0)
            {
                o.Smoothness = 1 - ps.a;
            }

            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
