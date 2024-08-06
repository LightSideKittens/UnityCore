// Normal
fixed4 Normal(fixed4 baseColor, fixed4 blendColor)
{
    return blendColor;
}

// Darken
fixed4 Darken(fixed4 baseColor, fixed4 blendColor)
{
    return min(baseColor, blendColor);
}

// Multiply
fixed4 Multiply(fixed4 baseColor, fixed4 blendColor)
{
    return baseColor * blendColor;
}

// Linear Burn
fixed4 LinearBurn(fixed4 baseColor, fixed4 blendColor)
{
    return baseColor + blendColor - 1;
}

// Color Burn
fixed4 ColorBurn(fixed4 baseColor, fixed4 blendColor)
{
    return 1 - ((1 - baseColor) / blendColor);
}

// Darker Color
fixed4 DarkerColor(fixed4 baseColor, fixed4 blendColor)
{
    return (dot(baseColor, baseColor) < dot(blendColor, blendColor)) ? baseColor : blendColor;
}

// Lighten
fixed4 Lighten(fixed4 baseColor, fixed4 blendColor)
{
    return max(baseColor, blendColor);
}

// Screen
fixed4 Screen(fixed4 baseColor, fixed4 blendColor)
{
    return 1 - (1 - baseColor) * (1 - blendColor);
}

// Color Dodge
fixed4 ColorDodge(fixed4 baseColor, fixed4 blendColor)
{
    return baseColor / (1 - blendColor);
}

// Linear Dodge
fixed4 LinearDodge(fixed4 baseColor, fixed4 blendColor)
{
    return baseColor + blendColor;
}

// Lighten Color
fixed4 LightenColor(fixed4 baseColor, fixed4 blendColor)
{
    return (dot(baseColor, baseColor) > dot(blendColor, blendColor)) ? baseColor : blendColor;
}

// Overlay
fixed4 Overlay(fixed4 baseColor, fixed4 blendColor)
{
    return (baseColor < 0.5) ? (2 * baseColor * blendColor) : (1 - 2 * (1 - baseColor) * (1 - blendColor));
}

// Soft Light
fixed4 SoftLight(fixed4 baseColor, fixed4 blendColor)
{
    return (blendColor < 0.5) ? (2 * baseColor * blendColor + baseColor * baseColor * (1 - 2 * blendColor)) :
                                (sqrt(baseColor) * (2 * blendColor - 1) + 2 * baseColor * (1 - blendColor));
}

// Hard Light
fixed4 HardLight(fixed4 baseColor, fixed4 blendColor)
{
    return (blendColor < 0.5) ? (2 * baseColor * blendColor) : (1 - 2 * (1 - baseColor) * (1 - blendColor));
}

// Vivid Light
fixed4 VividLight(fixed4 baseColor, fixed4 blendColor)
{
    return (blendColor < 0.5) ? (1 - (1 - baseColor) / (2 * blendColor)) : (baseColor / (1 - 2 * (blendColor - 0.5)));
}

// Linear Light
fixed4 LinearLight(fixed4 baseColor, fixed4 blendColor)
{
    return (blendColor < 0.5) ? (baseColor + 2 * blendColor - 1) : (baseColor + 2 * (blendColor - 0.5));
}

// Pin Light
fixed4 PinLight(fixed4 baseColor, fixed4 blendColor)
{
    return (blendColor < 0.5) ? min(baseColor, 2 * blendColor) : max(baseColor, 2 * (blendColor - 0.5));
}

// Hard Mix
fixed4 HardMix(fixed4 baseColor, fixed4 blendColor)
{
    return (baseColor + blendColor < 1.0) ? fixed4(0,0,0,1) : fixed4(1,1,1,1);
}

// Difference
fixed4 Difference(fixed4 baseColor, fixed4 blendColor)
{
    return abs(baseColor - blendColor);
}

// Exclusion
fixed4 Exclusion(fixed4 baseColor, fixed4 blendColor)
{
    return baseColor + blendColor - 2 * baseColor * blendColor;
}

// Subtract
fixed4 Subtract(fixed4 baseColor, fixed4 blendColor)
{
    return baseColor - blendColor;
}

// Divide
fixed4 Divide(fixed4 baseColor, fixed4 blendColor)
{
    return baseColor / blendColor;
}


fixed4 BlendColors (fixed4 top, fixed4 base) 
{
    top *= base.a;
    #ifdef BM_NORMAL
    return top *= base;
    #elif BM_DARKEN 
    return Darken(top, base);
    #elif BM_MULTIPLY  
    return Multiply(top, base);
    #elif BM_LINEARBURN 
    return LinearBurn(top, base);
    #elif BM_DARKERCOLOR
    return DarkerColor(top, base);
    #elif BM_COLORBURN
    return ColorBurn(top, base);
    #elif BM_LIGHTEN 
    return Lighten(top, base);                
    #elif BM_SCREEN 
    return Screen(top, base);                                
    #elif BM_COLORDODGE 
    return ColorDodge(top, base);                                
    #elif BM_LINEARDODGE 
    return LinearDodge(top, base);                                
    #elif BM_LIGHTENCOLOR 
    return LightenColor(top, base);                
    #elif BM_OVERLAY 
    return Overlay(top, base);                
    #elif BM_SOFTLIGHT 
    return SoftLight(top, base);                
    #elif BM_HARDLIGHT 
    return HardLight(top, base);                
    #elif BM_VIVIDLIGHT 
    return VividLight(top, base);                
    #elif BM_LINEARLIGHT 
    return LinearLight(top, base);                
    #elif BM_PINLIGHT 
    return PinLight(top, base);                
    #elif BM_HARDMIX 
    return HardMix(top, base);                
    #elif BM_DIFFERENCE 
    return Difference(top, base);                
    #elif BM_EXCLUSION 
    return Exclusion(top, base);                
    #elif BM_SUBTRACT 
    return Subtract(top, base);                
    #elif BM_DIVIDE
    return Divide(top, base);
    #else
    top *= base;
    #endif
    
    return top;
}