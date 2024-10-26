Shader "Custom/GroundUnder"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {} // Main texture for the ground
    }
    SubShader
    {
        Tags { "Queue" = "Background" } // Ensure it's rendered before everything else
        Pass
        {
            ZTest LEqual   // Only render if the pixel is behind or equal in depth
            ZWrite On      // Write to the depth buffer, but don't block other objects
            Cull Back      // Cull back faces for optimization
            Lighting Off   // Disable lighting if you don't need it for the ground
           
            // Use blending if you want a transparent effect (optional)
            // Blend SrcAlpha OneMinusSrcAlpha

            // Setup the texture sample and UVs
            SetTexture[_MainTex] { combine texture } // Sample the texture
        }
    }
}
