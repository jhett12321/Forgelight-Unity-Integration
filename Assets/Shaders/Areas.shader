Shader "Custom/Areas" {
    Properties{
        _Color("Color", Color) = (1,1,1,0.7)
    }
        SubShader{
        Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
#pragma surface surf Standard alpha

    struct Input {
        float2 uv_MainTex;
    };

    fixed4 _Color;

    void surf(Input IN, inout SurfaceOutputStandard o)
    {
        fixed4 c = _Color;

        //Diffuse
        o.Albedo = c.rgb;
        o.Alpha = c.a;
    }
    ENDCG
    }
        FallBack "Diffuse"
}
