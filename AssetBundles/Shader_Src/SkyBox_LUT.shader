Shader "Skybox/SkyBox_LUT"
{
    Properties
    {
        exposure ("exposure", Range(0, 20)) = 4.0
        ground_refract ("ground refract", Range(0, 1)) = 1.0
        ground_light ("ground light", Range(0, 1)) = 0.0
        mie_amount ("mie amount", Range(0, 10)) = 3.996
        mie_absorb ("mie absorb", Range(0, 10)) = 1.11
        minh ("planet ground radius", Float) = 63.71393
        maxh ("planet sky radius", float) = 64.71393
        H_Reayleigh ("reayleigh factor scale", float) = 0.08
        H_Mie ("min factor scale", float) = 0.02
        H_OZone ("ozone height", float) = 0.25
        D_OZone ("ozone radius", float) = 0.15
        SunColor ("SunColor", Color) = (1,1,1,1)
        mie_eccentricity ("mie eccentricity", Color) = (0.618,0.618,0.618,1)
        scatterLUT_Size ("scatterLUT_Size", Vector) = (0,0,0,0)
        reayleighScatterFactor ("Reayleigh Scatter Factor", Vector) = (0.47293,1.22733,2.09377,0)
        OZoneAbsorbFactor ("OZone Absorb Factor", Vector) = (0.21195,0.20962,0.01686,0)
        translucentLUT ("translucent LUT", 2D) = "white"{}
        scatterLUT ("scatter LUT", 2D) = "black"{}
    }
    SubShader
    {
        Tags { "Queue"="Transparent-1" "RenderType"="Transparent" "PreviewType"="Skybox" }
        ZWrite Off
        ZTest Off
        Cull Off
        LOD 0

        GrabPass 
        {
            "_GrabTexture"
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Lighting.cginc"
			#include "UnityShaderVariables.cginc"
            #include "./PBSky.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 position : TEXCOORD0;
                float3 screen : TEXCOORD1;
                float3 screenDir : TEXCOORD2;
            };

            sampler2D _GrabTexture;
            float4 SunColor;
            
            float ground_refract;
            float ground_light;
            sampler2D _CameraDepthTexture;
            // const float s = float(6.6315851227221438037423488874623);
            // #define ingCount 6
            // #define ingLightCount 8
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.position =  mul(unity_ObjectToWorld, v.vertex).xyz;
                o.screen = o.vertex.xyw;
                o.screenDir = mul(UNITY_MATRIX_MV,v.vertex).xyz;
                return o;
            }
            // float3 AFref(float3 dir)
            // {
            // 	float3 result = float3(0.0,0.0,0.0);
            // 	float strong = strong(dot(dir,LIGHT0_DIRECTION));
                
            // 	//return dir;
                
            // 	//float3 y = dir * (infos[5].x-infos[0].x) + infos[0].h;
            // 	//float cosl = dot(normalize(y),LIGHT0_DIRECTION);
            // 	//float sinl = sqrt(1.0-cosl*cosl);
            // 	//float3 srcl = (LIGHT0_COLOR-SunBlight(infos[4].h,float3(sinl,cosl,0))[5].reduce)*strong*(infos[5].reduce-infos[4].reduce);
            // 	//result = srcl;
            // 	//return normalize(result);
            // 	return max(float3(0.0,0.0,0.0),min(result,float3(1)));
            // }
            
            
            void sky(in float3 LIGHT0_COLOR, in float3 LIGHT0_DIRECTION, in float3 EYEDIR, in float3 POSITION, in float2 SCREEN, in float DEPTH, out float3 COLOR) {
                // POSITION /= 100000.0;
                // POSITION.y += h0 + 0.00005;
                
                float3 scatter = float3(0.0,0.0,0.0);
                float3 backGround = tex2D(_GrabTexture,float2(1.0 + SCREEN.x,1.0 - SCREEN.y) / 2.0).xyz;
                // float3 rgb = max(0.0,ceil(dot(LIGHT0_DIRECTION,EYEDIR) - cos(0.004652439059837008))) * LIGHT0_COLOR + backGround;
                float3 rgb = backGround;
                IngAirFogPropInfo infos = getIngAirFogPropInfoByRelPos(POSITION,EYEDIR,LIGHT0_DIRECTION,DEPTH);

                float3 trans;
                scatter = LightScatter(infos, LIGHT0_COLOR, backGround * ground_refract, backGround * ground_light, trans);
                float3 p = infos.depth * infos.viewDir + float3(0.0,infos.h,0.0);
                if(infos.h <= maxh) rgb *= trans * (1.0-step(length(p), maxh));
                scatter = max(float3(0.0,0.0,0.0),scatter);
                scatter = hdr(scatter);
                scatter = ACESTonemap(scatter);
                
                rgb = max(float3(0.0,0.0,0.0),rgb)+scatter;
                // rgb = float3(1.0,1.0,1.0)-exp(-rgb); 
                // rgb = ACESTonemap(rgb);
                // rgb = (float3(1.0,1.0,1.0)-exp(-rgb))/(1.0 - exp(-1.0));
                // rgb = sqrt(rgb);
                COLOR = rgb;
            }

            float4 frag (v2f i) : SV_Target
            {
                // const float3 OZoneAbsorbFactor = float3(0.21195,0.20962,0.01686);
                // const float3 OZoneAbsorbFactor = float3(0.065,0.1881,0.0085);
                float3 color = SunColor;
                float3 sun = normalize(_WorldSpaceLightPos0.xyz);
                float3 eye = normalize(i.position - _WorldSpaceCameraPos.xyz);
                float3 pos = _WorldSpaceCameraPos.xyz - mul(unity_ObjectToWorld, float4(0.0,0.0,0.0,1.0));
                float2 scr = i.screen.xy/i.screen.z;

                float scrDis = length(normalize(i.screenDir).xy);
                float depthValue = tex2D(_CameraDepthTexture, float2(1.0 + scr.x,1.0 - scr.y) / 2.0).x;
                depthValue = LinearEyeDepth(depthValue);
                depthValue /= sqrt(1.0 - scrDis*scrDis);
                sky(color,sun,eye,pos,scr,depthValue,color);


                // // float2 SCREEN = i.screen.xy/i.screen.z;
                // // return tex2D(_CameraDepthTexture,float2(1.0 + SCREEN.x,1.0 - SCREEN.y) / 2.0);
                // color = abs(IngOZoneDensity(h0+1.0,pos.y * length(eye.xz)) - IngOZoneDensity(pos.y*eye.y,pos.y * length(eye.xz))) * (float3(1.0,1.0,1.0)-OZoneAbsorbFactor);
                
                // return float4(scrDir.x,scrDir.y,scrDir.z,1.0);
                return float4(color.x,color.y,color.z,1.0);
                // // return fixed4(SCREEN.x,SCREEN.y,0.0,1.0);

            }

            ENDCG
        }
    }
}
