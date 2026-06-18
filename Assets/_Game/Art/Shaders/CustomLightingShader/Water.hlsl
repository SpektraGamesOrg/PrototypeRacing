#ifndef WATER_LIGHTING_INCLUDED
#define WATER_LIGHTING_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

#ifdef _SHADOW_SAMPLES_LOW
    #define SHADOW_ITERATIONS 1
    #define SHADOW_VOLUME
#elif _SHADOW_SAMPLES_MEDIUM
    #define SHADOW_ITERATIONS 2
    #define SHADOW_VOLUME
#elif _SHADOW_SAMPLES_HIGH
    #define SHADOW_ITERATIONS 4
    #define SHADOW_VOLUME
#else
    #define SHADOW_ITERATIONS 0
#endif


#ifdef _SSR_SAMPLES_LOW
    #define SSR_ITERATIONS 8
#elif _SSR_SAMPLES_MEDIUM
    #define SSR_ITERATIONS 16
#elif _SSR_SAMPLES_HIGH
    #define SSR_ITERATIONS 32
#else
    #define SSR_ITERATIONS 4
#endif

///////////////////////////////////////////////////////////////////////////////
//                           Reflection Modes                                //
///////////////////////////////////////////////////////////////////////////////

void Reflection_half(half3 reflectVector, float3 positionWS, half perceptualRoughness, half occlusion, float2 normalizedScreenSpaceUV, out half3 output)
{
    output = GlossyEnvironmentReflection(reflectVector, positionWS, perceptualRoughness, occlusion, normalizedScreenSpaceUV);
}

float3 ViewPosFromDepth(float2 positionNDC, float deviceDepth)
{
    float4 positionCS  = ComputeClipSpacePosition(positionNDC, deviceDepth);
    float4 hpositionVS = mul(UNITY_MATRIX_I_P, positionCS);
    return hpositionVS.xyz / hpositionVS.w;
}

float2 ViewSpacePosToUV(float3 pos)
{
    return ComputeNormalizedDeviceCoordinates(pos, UNITY_MATRIX_P);
}

half OutOfBoundsFade(half2 uv)
{
    half2 fade = 0;
    fade.x = saturate(1 - abs(uv.x - 0.5) * 2);
    fade.y = saturate(1 - abs(uv.y - 0.5) * 2);
    return fade.x * fade.y;
}

void Raymarch_half(float3 origin, float3 direction, half steps, half stepSize, half thickness, float mask1, out half2 sampleUV, out half valid, out half outOfBounds, out half debug)
{
    sampleUV = 0;
    valid = 0;
    outOfBounds = 0;
    debug = 0;
    if (mask1 < 0.1)
        return;
    int stepsCount = min((int) steps, 8);

    float3 baseOrigin = origin;
    
    direction *= stepSize;
    const half rcpStepCount = 1.0h/stepsCount;
    
    // [loop]
    [unroll]
  
    
        for (int i = 0; i < stepsCount; i++)
        {
            debug++;
        //if(valid == 0)
        {
                origin += direction;
                direction *= 1.5;
                sampleUV = ViewSpacePosToUV(origin);

                outOfBounds = OutOfBoundsFade(sampleUV);
            
            //return;
            
                if (!(sampleUV.x > 1 || sampleUV.x < 0 || sampleUV.y > 1 || sampleUV.y < 0))
                {
                    float deviceDepth = SampleSceneDepth(sampleUV);
                    float3 samplePos = ViewPosFromDepth(sampleUV, deviceDepth);

                    if (distance(samplePos.z, origin.z) > length(direction) * thickness)
                        continue;

                
        
                    if (samplePos.z > origin.z)
                    {
                        valid = 1;
                        return;
                    }
                
                }
                else
                {
                //outOfBounds = OutOfBoundsFade(sampleUV);
                    return;
                }
            }
        }
    
}









#endif 
