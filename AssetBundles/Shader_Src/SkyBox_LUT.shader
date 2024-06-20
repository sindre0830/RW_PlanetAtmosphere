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
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 uv : TEXCOORD0;
                float3 screen : TEXCOORD1;
            };

            sampler2D _GrabTexture;

            float4 groundColor;
            float4 mie_eccentricity;
            float4 SunColor;
            
            float ground_refract;
            float ground_light;
            float exposure;
            // sampler2D _CameraDepthTexture;
            // const float s = float(6.6315851227221438037423488874623);
            // #define ingCount 6
            // #define ingLightCount 8
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv =  mul(unity_ObjectToWorld, v.vertex).xyz - _WorldSpaceCameraPos.xyz;
                o.screen = o.vertex.xyw;
                return o;
            }

            struct IngAirFogPropInfo
            {
                float h;
                float3 viewDir;
                float3 lightDir;
            };

            float3x3 Crossfloat3x3_W2L(float3 d1, float3 d2) //d1,d2 on x-y plane, x axis lock on d1
            {
                d1 = normalize(d1);
                d2 = normalize(d2);
                float3 d3 = normalize(cross(d1,d2));
                d2 = normalize(cross(d3,d1));
                
                return float3x3(d1,d2,d3);
            }
            
            void transToHeightAndDir(in float3 relPos, in float3 dir, out float3 transedDir, out float h)
            {
                float3 d1 = abs(relPos);
                if (d1.y < d1.x)
                {
                    if (d1.z < d1.y) d1 = float3(0.0,0.0,1.0);
                    else d1 = float3(0.0,1.0,0.0);
                }
                else
                {
                    if (d1.z < d1.x) d1 = float3(0.0,0.0,1.0);
                    else d1 = float3(1.0,0.0,0.0);
                }
                float3x3 proj = Crossfloat3x3_W2L(relPos,d1);
                h = length(relPos);
                transedDir = normalize(mul(proj,dir)).yxz;
            }

            IngAirFogPropInfo getIngAirFogPropInfoByRelPos(float3 relPos, float3 viewDir, float3 lightDir)
            {
                float3 d1 = abs(relPos);
                if (d1.y < d1.x)
                {
                    if (d1.z < d1.y) d1 = float3(0.0,0.0,1.0);
                    else d1 = float3(0.0,1.0,0.0);
                }
                else
                {
                    if (d1.z < d1.x) d1 = float3(0.0,0.0,1.0);
                    else d1 = float3(1.0,0.0,0.0);
                }
                float3x3 proj = Crossfloat3x3_W2L(relPos,d1);
                IngAirFogPropInfo result;
                result.h = length(relPos);
                result.viewDir = normalize(mul(proj,viewDir)).yxz;
                result.lightDir = normalize(mul(proj,lightDir)).yxz;
                
                float h0 = result.h * length(result.viewDir.xz);
                float x0 = result.h * result.viewDir.y;
                float mh = 0.0;
                if(h0 < maxh)
                {
                    mh = sqrt(maxh*maxh-h0*h0);
                }
                if(x0 < -mh)
                {
                    relPos = float3(0.0,result.h,0.0) - (x0 + mh) * result.viewDir;
                    d1 = abs(relPos);
                    if (d1.y < d1.x)
                    {
                        if (d1.z < d1.y) d1 = float3(0.0,0.0,1.0);
                        else d1 = float3(0.0,1.0,0.0);
                    }
                    else
                    {
                        if (d1.z < d1.x) d1 = float3(0.0,0.0,1.0);
                        else d1 = float3(1.0,0.0,0.0);
                    }
                    proj = Crossfloat3x3_W2L(relPos,d1);
                    result.h = length(relPos);
                    result.viewDir = normalize(mul(proj,result.viewDir)).yxz;
                    result.lightDir = normalize(mul(proj,result.lightDir)).yxz;
                }
                return result;
            }
            
            float reayleighStrong(float cosw)
            {
                return 0.05968310365946075091333141126469*(1.0+cosw*cosw);
            }
            
            float3 mieStrong(float cosw)
            {
                float3 mie = mie_eccentricity.xyz * 2.0 - float3(1.0,1.0,1.0);
                float3 g = mie * mie;
                return 0.07957747154594766788444188168626*(float3(1.0,1.0,1.0)-g)*(1.0+cosw*cosw)/((float3(2.0,2.0,2.0)+g)*pow(float3(1.0,1.0,1.0)+g-2.0*mie*cosw,float3(1.5,1.5,1.5)));
            }
            
            float3 getScatterInfo(float3 viewDir, float3 lightDir, float3 lightColor, float h)
            {
                // const float3 reayleighScatterFactor = float3(0.58,1.35,3.31);
                // const float3 OZoneAbsorbFactor = float3(0.21195,0.20962,0.01686);
                // const float3 OZoneAbsorbFactor = float3(0.065,0.1881,0.0085);
                float3 mieScatterFactor = mie_amount.xxx;
                float3 mieAbsorbFactor = (mie_absorb * mie_amount).xxx;
                // float3 offset = float3(0.0);
                h = abs(h);
                float w = acos(clamp(viewDir.y,-1.0,1.0));
                float l = acos(clamp(lightDir.y,-1.0,1.0));
                // float h0 = h * length(viewDir.xz);
                if (h < minh) return float3(0.0,0.0,0.0);

                lightColor = abs(lightColor);
                float cosw = dot(viewDir,lightDir);
                // return float4(w,h,l,acos(dot(normalize(viewDir.xz),normalize(lightDir.xz)))).zzz / PI;
                // float3 scatterFromZeroW = scatterFromLUT(float4(w,h,l,0.5));
                float3 all = scatterFromLUT(float4(w,h,l,acos(clamp(dot(normalize(viewDir.xz),normalize(lightDir.xz)),-1.0,1.0)))) * lightColor;
                // float2 ah = AH2Map(float2(w,h));
                // float bottom = pow(ah.x,8.0);
                // if(bottom > 0.0625)
                // {
                //     float3 scatterFromZeroW = scatterFromLUT_avg(float3(w,h,l));
                    // all = lerp(all,scatterFromZeroW,bottom);
                // }
                // w /= PI;
                // // w *= w * w;
                // w *= w;
                // w *= w;
                // // w *= w;
                // float3 all = scatterFromLUT(float4(w,h,l,PI * 0.6));
                // return all;
                // float3 all = scatterFromLUT(float4(w,h,l,0.0)) * lightColor;
                float3 r = all * reayleighScatterFactor / (reayleighScatterFactor + mieScatterFactor);
                float3 m = all * mieScatterFactor / (reayleighScatterFactor + mieScatterFactor);
                
                float reayleigh = reayleighStrong(cosw);
                // float reayleigh = 0.620350490899400016668;
                float3 mie = mieStrong(cosw);
                // float3 mie = (mie_eccentricity * 2.0 - float3(1.0)) * 0.310175245449700008334;
                r *= reayleigh * reayleighScatterFactor;
                m *= mie * mieScatterFactor;
                return r + m;
            }
            
            //blocked light
            float3 LightScatter(IngAirFogPropInfo infos, float3 lightColor, float3 surfaceColor, float3 surfaceLight)
            {
                // const float3 reayleighScatterFactor = float3(0.58,1.35,3.31);
                // const float3 OZoneAbsorbFactor = float3(0.21195,0.20962,0.01686);
                // const float3 OZoneAbsorbFactor = float3(0.065,0.1881,0.0085);
                float3 mieScatterFactor = mie_amount.xxx;
                float3 mieAbsorbFactor = (mie_absorb * mie_amount).xxx;

                float3 result = float3(0.0,0.0,0.0);

                float h0 = infos.h * length(infos.viewDir.xz);
                float x0 = infos.h * infos.viewDir.y;
                float mh = 0.0;
                if(h0 < maxh)
                {
                    mh = sqrt(maxh*maxh-h0*h0);
                }
                float ml = -mh;
                if(h0 < minh)
                {
                    ml = sqrt(minh*minh-h0*h0);
                }
                if(x0 <= -ml || ml <= x0)
                {
                    if(x0 < -ml && h0 < minh)
                    {
                        float3 p = -(ml + x0) * infos.viewDir + float3(0.0,infos.h,0.0);
                        float lightAng = acos(dot(infos.lightDir,p / minh));
                        float h = infos.h;
                        x0 = max(ml,x0);
                        x0 = min(mh,x0);
                        transToHeightAndDir(p,infos.lightDir,p,h);
                        surfaceLight += surfaceColor * lightColor * translucentFromLUT(float2(lightAng,minh));
                        // surfaceLight += surfaceColor * getScatterInfo(float3(0.0,1.0,0.0),p,lightColor,minh);
                        result = surfaceLight * translucentFromLUT_BlockByGround(float2(atan2(h0,x0),sqrt(h0*h0+x0*x0)));
                        // result = translucentFromLUT_BlockByGround(float2(atan2(h0,x0),sqrt(h0*h0+x0*x0)));
                        result = max(result,0.0);
                    }
                    result += getScatterInfo(infos.viewDir,infos.lightDir,lightColor,infos.h);
                }

                return max(float3(0.0,0.0,0.0),result);
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
            
            
            float3 hdr(float3 L) 
            {
                L = L * exposure;
                L.r = L.r < 1.413 ? pow(L.r * 0.38317, 1.0 / 2.2) : 1.0 - exp(-L.r);
                L.g = L.g < 1.413 ? pow(L.g * 0.38317, 1.0 / 2.2) : 1.0 - exp(-L.g);
                L.b = L.b < 1.413 ? pow(L.b * 0.38317, 1.0 / 2.2) : 1.0 - exp(-L.b);
                return L;
            }
            float3 ACESTonemap(float3 color){
                const float a = 2.51;  // Default: 2.51f
                const float b = 0.03;  // Default: 0.03f
                const float c = 2.43;  // Default: 2.43f
                const float d = 0.59;  // Default: 0.59f
                const float e = 0.14;  // Default: 0.14f
                const float p = 1.3;
                const float overlap = 0.2;
                
                const float rgOverlap = 0.1 * 0.2;
                const float rbOverlap = 0.01 * 0.2;
                const float gbOverlap = 0.04 * 0.2;
                
                const float3x3 coneOverlap = 	float3x3(
                                                float3(1.0          , 0.1 * 0.2 , 0.01 * 0.2),
                                                float3(0.1 * 0.2    , 1.0       , 0.04 * 0.2),
                                                float3(0.01 * 0.2   , 0.1 * 0.2 , 1.0       )
                                                );
                
                const float3x3 coneOverlapInverse = float3x3(
                                                float3(	1.0 + (0.1 * 0.2 + 0.01 * 0.2)  , 					    -0.1 * 0.2  ,	-0.01 * 0.2			            ),
                                                float3(	-0.1 * 0.2					    ,   1.0 + (0.1 * 0.2 + 0.04 * 0.2)  ,	-0.04 * 0.2					    ),
                                                float3(	-0.01 * 0.2					    , 					    -0.1 * 0.2  ,	1.0 + (0.01 * 0.2 + 0.1 * 0.2)  )
                                                );
                
                color = mul(coneOverlap,color);
                color = pow(color, float3(p,p,p));
                color = (color * (a * color + b)) / (color * (c * color + d) + e);
                color = pow(color, float3(1.0,1.0,1.0)/p);
                color = mul(coneOverlapInverse,color);
                color = clamp(color,float3(0.0,0.0,0.0),float3(1.0,1.0,1.0));
                return color;
            }
            
            void sky(in float3 LIGHT0_COLOR, in float3 LIGHT0_DIRECTION, in float3 EYEDIR, in float3 POSITION, in float2 SCREEN, out float3 COLOR) {
                // POSITION /= 100000.0;
                // POSITION.y += h0 + 0.00005;
                
                float3 scatter = float3(0.0,0.0,0.0);
                float3 backGround = tex2D(_GrabTexture,float2(1.0 + SCREEN.x,1.0 - SCREEN.y) / 2.0).xyz;
                float3 rgb = max(0.0,ceil(dot(LIGHT0_DIRECTION,EYEDIR) - cos(0.004652439059837008))) * LIGHT0_COLOR + backGround;
                IngAirFogPropInfo infos = getIngAirFogPropInfoByRelPos(POSITION,EYEDIR,LIGHT0_DIRECTION);
                // if (infos.h <= maxh+0.0001)
                // {
                    // backGround *= groundColor;
                    scatter = LightScatter(infos,LIGHT0_COLOR,backGround * ground_refract,backGround * ground_light);
                    // scatter = LightScatter(infos,LIGHT0_COLOR,groundColor,float3(0.0,0.0,0.0));
                    // float h0 = infos.h * length(infos.viewDir.xz);
                    // float x0 = infos.h * infos.viewDir.y;
                    rgb *= translucentFromLUT(float2(acos(infos.viewDir.y),infos.h));
                    // if (infos.dirInfos.e < 0.0) rgb = float3(0.0,0.0,0.0);
                    // COLOR = translucentFromLUTBlockByGround(float2(atan2(h0,x0),infos.h));
                    // COLOR = scatterFromLUT(float3(atan2(h0,x0),infos.h,0.0));
                    // COLOR = getScatterInfo(infos.viewDir,infos.lightDir,LIGHT0_COLOR,infos.h);
                    // return;
                    scatter = max(float3(0.0,0.0,0.0),scatter);
                    // scatter = scatter*exp(scatter);
                    // scatter = (float3(1.0,1.0,1.0)-exp(-scatter))/(1.0 - exp(-1.0));
                    // scatter = sqrt(scatter);
                    // scatter = log(scatter + float3(1.0,1.0,1.0))/log(1.2);
                    // scatter = float3(1.0,1.0,1.0)-exp(-scatter);
                    scatter = hdr(scatter);
                    scatter = ACESTonemap(scatter);

                // }
                rgb = max(float3(0.0,0.0,0.0),rgb)+scatter;
                // rgb = float3(1.0,1.0,1.0)-exp(-rgb); 
                // rgb = ACESTonemap(rgb);
                // rgb = (float3(1.0,1.0,1.0)-exp(-rgb))/(1.0 - exp(-1.0));
                // rgb = sqrt(rgb);
                COLOR = rgb;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // const float3 OZoneAbsorbFactor = float3(0.21195,0.20962,0.01686);
                // const float3 OZoneAbsorbFactor = float3(0.065,0.1881,0.0085);
                float3 color = SunColor;
                float3 sun = normalize(_WorldSpaceLightPos0.xyz);
                float3 eye = normalize(i.uv);
                float3 pos = _WorldSpaceCameraPos.xyz - mul(unity_ObjectToWorld, float4(0.0,0.0,0.0,1.0));
                sky(color,sun,eye,pos,i.screen.xy/i.screen.z,color);
                // // float2 SCREEN = i.screen.xy/i.screen.z;
                // // return tex2D(_CameraDepthTexture,float2(1.0 + SCREEN.x,1.0 - SCREEN.y) / 2.0);
                // color = abs(IngOZoneDensity(h0+1.0,pos.y * length(eye.xz)) - IngOZoneDensity(pos.y*eye.y,pos.y * length(eye.xz))) * (float3(1.0,1.0,1.0)-OZoneAbsorbFactor);

                return fixed4(color.x,color.y,color.z,1.0);
                // // return fixed4(SCREEN.x,SCREEN.y,0.0,1.0);

            }

            ENDCG
        }
    }
}
