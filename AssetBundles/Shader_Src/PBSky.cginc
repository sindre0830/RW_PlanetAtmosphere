
sampler2D translucentLUT;
sampler2D scatterLUT;
float4 translucentLUT_TexelSize;
float4 scatterLUT_Size;
float3 reayleighScatterFactor;
float3 OZoneAbsorbFactor;
float mie_amount;
float mie_absorb;
float minh;
float maxh;
float H_Reayleigh;
float H_Mie;
float H_OZone;
float D_OZone;

#define ingCount 2048
// #define ingLightCount 8

#define PI 3.1415926535897932384626433832795


float2 Map2AH(float2 map)
{
    map = clamp(map,float2(0.0,0.0),float2(1.0,1.0));
    map.y *= map.y;
    map.y *= maxh - minh;
    map.y += minh;
    
    float horizonAngA = asin(clamp(minh/map.y,-1.0,1.0)); //p
    float horizonAngB = PI - horizonAngA; //p
    float ang = map.x*(1.0+sqrt(horizonAngA/horizonAngB))-1.0;
    map.x = horizonAngB*(1.0+sign(ang)*ang*ang);
    // map.x *= PI;

    // map.x *= 2.0;
    // map.x -= 1.0;
    // map.x = sign(map.x)*map.x*map.x*PI*0.5;
    // map.x += 0.5*PI;
    return map;
}

float2 AH2Map(float2 ah)
{
    ah = clamp(ah,float2(0.0,minh),float2(PI,maxh));

    float horizonAngA = asin(clamp(minh/ah.y,-1.0,1.0)); //p
    float horizonAngB = PI - horizonAngA; //p
    float ang = ah.x / horizonAngB - 1.0;
    ah.x = (1.0 + sign(ang) * sqrt(abs(ang)))/(1.0 + sqrt(horizonAngA/horizonAngB));
    // ah.x /= PI;

    // ah.x -= 0.5*PI;
    // ah.x = 0.5 + 0.5 * sign(ah.x) * sqrt(2.0*abs(ah.x)/PI);

    ah.y -= minh;
    ah.y /= maxh - minh;
    ah.y = sqrt(ah.y);
    ah = clamp(ah,float2(0.0,0.0),float2(1.0,1.0));
    return ah;
}


float4 Map2AHLW(float2 map)
{
    map = clamp(map,0.0,1.0);
    map *= scatterLUT_Size.xy*scatterLUT_Size.zw-float2(1.0,1.0);
    float4 result = map.xyxy;
    result.zw = floor(result.zw / scatterLUT_Size.xy);
    result.xy = (result.xy - result.zw * scatterLUT_Size.xy) / (scatterLUT_Size.xy - float2(1.0,1.0));
    result.zw = result.zw / (scatterLUT_Size.zw - float2(1.0,1.0));
    result.xy = Map2AH(result.xy);
    result.zw *= PI;
    return result;
}


float3 translucent(float3 light, float3 scatterAndAbsorb, float dens)
{
    float3 result = dens * scatterAndAbsorb;
    result = max(float3(0.0,0.0,0.0),result);
    result = exp(-result);
    return light * result;
}

float IngOZoneDensity(float x, float h)
{
    // return 0.0;
    float result = 0.0;
    // h*=h;
    float ozMin = 0.15+minh;
    float ozMid = 0.25+minh;
    float ozMax = 0.35+minh;
    if (h < ozMax)
    {
        float qh = h*h;
        float xCur = abs(x);
        float proccessedX = 0.0;
        float proccessedH = h;
        float xMax = sqrt(ozMax*ozMax-qh);
        if (h < ozMid)
        {
            float xMid = sqrt(ozMid*ozMid-qh);
            if (h < ozMin)
            {
                float xMin = sqrt(ozMin*ozMin-qh);
                if (xCur > xMin)
                {
                    proccessedX = xMin;
                    proccessedH = ozMin;
                }
                else return 0.0;
            }
            if (xCur > proccessedX)
            {
                float nextX = min(xCur,xMid);
                float nextH = sqrt(nextX*nextX+qh);
                //f(x) = sgn(H-targetH)(5*qh*ln(targetH+x)+(5*targetH−10*H)*x) + x
                //fd(x) = (5*qh*ln(targetH+x)+(5*targetH−10*H)*x) + x
                //fd(x) = (5*qh*ln(targetH+x)+5*(targetH−2*H)*x) + x
                //fd(x) = 5*(qh*ln(targetH+x)+(targetH−2*H)*x) + x
                //fd(x) = 5*((targetH-2*H)*x+qh*ln(targetH+x)) + x
                // result += 5.0*((nextH - 2.0*H) * nextX + qh * log(nextH + nextX)) + nextX
                // - 5.0*((proccessedH - 2.0*H) * proccessedX + qh * log(proccessedH + proccessedX)) - proccessedX
                result += 5.0 * ((nextH - 2.0 * ozMid) * nextX - (proccessedH - 2.0 * ozMid) * proccessedX + qh * log((nextH + nextX) / (proccessedH + proccessedX))) + nextX - proccessedX;
                proccessedX = nextX;
                proccessedH = nextH;
            }
        }
        if (xCur > proccessedX)
        {
            float nextX = min(xCur,xMax);
            float nextH = sqrt(nextX*nextX+qh);
            //f(x) = sgn(H-targetH)(5*qh*ln(targetH+x)+(5*targetH−10*H)*x) + x
            //fu(x) = -(5*qh*ln(targetH+x)+(5*targetH−10*H)*x) + x
            //fu(x) = -5*qh*ln(targetH+x)-(5*targetH−10*H)*x + x
            //fu(x) = -5*qh*ln(targetH+x)+(10*H-5*targetH)*x + x
            //fu(x) = -5*qh*ln(targetH+x)+5*(2*H-targetH)*x + x
            //fu(x) = 5*(2*H-targetH)*x-5*qh*ln(targetH+x)+x
            //fu(x) = 5*((2*H-targetH)*x-qh*ln(targetH+x))+x
            // result += 5.0*((2.0 * H - nextH) * nextX - qh * log(nextH + nextX)) + nextX
            //         - 5.0*((2.0 * H - proccessedH) * proccessedX - qh * log(proccessedH + proccessedX)) - proccessedX;
            result += 5.0 * ((2.0 * ozMid - nextH) * nextX - (2.0 * ozMid - proccessedH) * proccessedX - qh * log((nextH + nextX) / (proccessedH + proccessedX))) + nextX - proccessedX;
            // proccessedX = nextX;
            // proccessedH = nextH;
        }
    }
    return result * sign(x);
}



void IngAirDensityFromTo(in float x0,in float end,in float h, out float reayleigh, out float mie)
{
    reayleigh = 0.0;
    mie = 0.0;
    float d = (end-x0)/float(ingCount);
    float prve_H = sqrt(x0*x0+h*h)-minh;
    float prve_reayleigh = exp(-prve_H/H_Reayleigh);
    float prve_mie = exp(-prve_H/H_Mie);
    // float prve_mie = prve_reayleigh * prve_reayleigh;
    // prve_mie *= prve_mie * prve_mie * prve_reayleigh;
    for(int i = 1; i < ingCount; i++)
    {
        float pos = x0+d*float(i);
        float current_H = sqrt(pos*pos+h*h)-minh;
        float current_reayleigh = exp(-current_H/H_Reayleigh);
        float current_mie = exp(-current_H/H_Mie);
        // float current_mie = current_reayleigh * current_reayleigh;
        // current_mie *= current_mie * current_mie * current_reayleigh;
        // if(abs(pos) > d)
        // {
        //     reayleigh += H_Reayleigh * (prve_reayleigh - current_reayleigh) * d / (current_H - prve_H);
        //     mie += H_Mie * (prve_mie - current_mie) * d / (current_H - prve_H);
        // }
        // else
        // {
        //     reayleigh += (prve_reayleigh + current_reayleigh) * d * 0.5;
        //     mie += (prve_mie + current_mie) * d * 0.5;
        // }
        reayleigh += (prve_reayleigh + current_reayleigh) * d * 0.5;
        mie += (prve_mie + current_mie) * d * 0.5;
        prve_H = current_H;
        prve_reayleigh = current_reayleigh;
        prve_mie = current_mie;
    }
}

void IngAirDensity(in float x0, in float h, out float reayleigh, out float mie, out float oZone)
{
    float mh = 0.0;
    if(h < maxh)
    {
        mh = sqrt(maxh*maxh-h*h);
    }
    float ml = -mh;

    x0 = max(ml,x0);
    x0 = min(mh,x0);
    IngAirDensityFromTo(x0,mh,h,reayleigh,mie);
    // oZone = abs(IngOZoneDensity(mh,h) - IngOZoneDensity(x0,h));
    oZone = IngOZoneDensity(mh,h) - IngOZoneDensity(x0,h);
    // x0 = max(ml,x0);
    // x0 = min(mh,x0);
    // IngAirDensityFromTo(x0,mh,h,reayleigh,mie);
    // // oZone = abs(IngOZoneDensity(mh,h) - IngOZoneDensity(x0,h));
    // oZone = IngOZoneDensity(mh,h) - IngOZoneDensity(x0,h);
    
}

float3 translucentFromLUT(float2 ah)
{
    if(ah.y < minh) return float3(0.0,0.0,0.0);
    ah = AH2Map(ah);
    ah = (ah * (translucentLUT_TexelSize.zw - float2(1.0,1.0)) + float2(0.5,0.5)) * translucentLUT_TexelSize.xy;
    return tex2Dlod(translucentLUT,float4(ah.x,ah.y,0.0,0.0)).xyz;
}

float3 scatterFromLUT(float4 ahlw)
{
    ahlw.zw /= PI;
    ahlw.xy = AH2Map(ahlw.xy);
    ahlw = clamp(ahlw,0.0,1.0) * (scatterLUT_Size - float4(1.0,1.0,1.0,1.0));
    ahlw.xy += float2(0.5,0.5);
    float2 zwFloor = clamp(floor(ahlw.zw),float2(0.0,0.0),scatterLUT_Size.zw - float2(1.0,1.0));
    float2 from = ahlw.xy / scatterLUT_Size.xy + zwFloor;
    float2 to = (from + float2(1.0,1.0)) / scatterLUT_Size.zw;
    from /= scatterLUT_Size.zw;
    // float2 wFrom = clamp(ceil(ahlw.zw) - ahlw.zw,0.0,1.0);
    float2 wTo = clamp(ahlw.zw - zwFloor,0.0,1.0);
    float2 wFrom = float2(1.0,1.0) - wTo;

    return  (tex2Dlod(scatterLUT,float4(from.x,from.y,0.0,0.0)).xyz * wFrom.y + tex2Dlod(scatterLUT,float4(from.x,to.y,0.0,0.0)).xyz * wTo.y) * wFrom.x +
            (tex2Dlod(scatterLUT,float4(to.x,from.y,0.0,0.0)).xyz * wFrom.y + tex2Dlod(scatterLUT,float4(to.x,to.y,0.0,0.0)).xyz * wTo.y) * wTo.x;
}

float3 GenScatterInfo(float viewAng, float height, float lightAng, float lightToViewAng)
{
    // const float3 reayleighScatterFactor = float3(0.58,1.35,3.31);
    // const float3 OZoneAbsorbFactor = float3(0.21195,0.20962,0.01686);
    // const float3 OZoneAbsorbFactor = float3(0.065,0.1881,0.0085);
    float3 mieScatterFactor = mie_amount.xxx;
    float3 mieAbsorbFactor = (mie_absorb * mie_amount).xxx;
    float3 result = float3(0.0,0.0,0.0);

    float x0 = cos(viewAng)*height;
    float h0 = sin(viewAng)*height;
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
    if(x0 >= ml || x0 <= -ml)
    {
        if(x0 < -ml)
        {
            float mml = -ml;
            ml = -mh;
            mh = mml;
        }
        x0 = max(ml,x0);
        x0 = min(mh,x0);
        
        float d = (mh-x0)/float(ingCount - 1);
        float reayleigh = 0.0;
        float mie = 0.0;
        float oZone = IngOZoneDensity(x0,h0);
        float prve_reayleigh = exp((minh - height)/H_Reayleigh);
        float prve_mie = exp((minh - height)/H_Mie);
        float prve_oZone = oZone;
        float3 viewDir = float3(sin(viewAng),cos(viewAng),0.0);
        float3 lightDir = float3(sin(lightAng)*cos(lightToViewAng),cos(lightAng),sin(lightAng)*sin(lightToViewAng));
        float3 prve_light = translucentFromLUT(float2(lightAng,height));
        // float prve_mie = prve_reayleigh * prve_reayleigh;
        // prve_mie *= prve_mie * prve_mie * prve_reayleigh;
        for(int i = 1; i < ingCount; i++)
        {
            // exp((h0-sqrt(x^2+h^2))/H) dx
            // -sqrt(x^2+h^2)*H/x d(exp((h0-sqrt(x^2+h^2))/H))
            float l = d * float(i);
            float3 current_postion = l * viewDir;
            current_postion.y += height;
            float current_H = length(current_postion);
            float3 current_light = translucentFromLUT(float2(acos(clamp(dot(current_postion/current_H,lightDir),-1.0,1.0)),current_H));
            current_H -= minh;
            float current_reayleigh = exp(-current_H/H_Reayleigh);
            float current_mie = exp(-current_H/H_Mie);
            float current_oZone = IngOZoneDensity(x0+l,h0);
            // float current_mie = current_reayleigh * current_reayleigh;
            // current_mie *= current_mie * current_mie * current_reayleigh;
            float reayleighScatterAmount = (prve_reayleigh + current_reayleigh) * d * 0.5;
            float mieScatterAmount = (prve_mie + current_mie) * d * 0.5;


            float3 reayleighScatterLight = (prve_light + current_light) * 0.5 * reayleighScatterAmount * reayleighScatterFactor;
            float3 mieScatterLight = (prve_light + current_light) * 0.5 * mieScatterAmount * mieScatterFactor;
            reayleighScatterLight = translucent(reayleighScatterLight, reayleighScatterFactor, reayleigh);
            reayleighScatterLight = translucent(reayleighScatterLight, mieScatterFactor + mieAbsorbFactor, mie);
            reayleighScatterLight = translucent(reayleighScatterLight, OZoneAbsorbFactor, prve_oZone - oZone);
            mieScatterLight = translucent(mieScatterLight, reayleighScatterFactor, reayleigh);
            mieScatterLight = translucent(mieScatterLight, mieScatterFactor + mieAbsorbFactor, mie);
            mieScatterLight = translucent(mieScatterLight, OZoneAbsorbFactor, prve_oZone - oZone);
            result += reayleighScatterLight;
            result += mieScatterLight;


            reayleigh += reayleighScatterAmount;
            mie += mieScatterAmount;

            prve_reayleigh = current_reayleigh;
            prve_mie = current_mie;
            prve_oZone = current_oZone;
            prve_light = current_light;
        }
    }
    return max(result, float3(0.0,0.0,0.0));
}
