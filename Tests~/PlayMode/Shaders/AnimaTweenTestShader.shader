Shader "Hidden/AnimaTweenTestShader" 
{
    Properties 
    {
        _Color ("Color", Color) = (1,1,1,1)
        _Float ("Float", Float) = 0
        _Int ("Int", Int) = 0
        _Vector ("Vector", Vector) = (0,0,0,0)
    }
    SubShader 
    {
        Pass 
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct appdata {
                float4 vertex : POSITION;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v) 
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }
            
            fixed4 _Color;

            fixed4 frag (v2f i) : SV_Target 
            {
                return _Color;
            }
            ENDCG
        }
    }
}