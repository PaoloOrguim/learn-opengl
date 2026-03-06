#version 330 core
out vec4 FragColor;

in VS_OUT {
    vec3 FragPos;
    vec2 TexCoords;
    vec3 TangentLightPos;
    vec3 TangentViewPos;
    vec3 TangentFragPos;
    mat3 TBN;         // NEW (tangent->world)
} fs_in;

uniform sampler2D diffuseMap;
uniform sampler2D normalMap;
uniform sampler2D depthMap;

uniform samplerCube interiorCube;      // NEW: interior cubemap
uniform bool cubeIsTangentSpace;       // NEW: 1 = cubemap baked in tangent-space, 0 = world-space
uniform float heightScale;
uniform vec3 lightPos;              // for lighting calculations (world-space)

//////////////////////
// PARALLAX (returns displaced UV AND height)
//////////////////////
vec2 ParallaxMapping(vec2 texCoords, vec3 viewDir, out float outHeight)
{ 
    // number of depth layers
    const float minLayers = 8.0;
    const float maxLayers = 32.0;
    float numLayers = mix(maxLayers, minLayers, abs(dot(vec3(0.0, 0.0, 1.0), viewDir)));  
    float layerDepth = 1.0 / numLayers;
    float currentLayerDepth = 0.0;
    vec2 P = viewDir.xy / viewDir.z * heightScale; 
    vec2 deltaTexCoords = P / numLayers;
  
    vec2  currentTexCoords     = texCoords;
    float currentDepthMapValue = texture(depthMap, currentTexCoords).r;
      
    while(currentLayerDepth < currentDepthMapValue)
    {
        currentTexCoords -= deltaTexCoords;
        currentDepthMapValue = texture(depthMap, currentTexCoords).r;  
        currentLayerDepth += layerDepth;  
    }
    
    vec2 prevTexCoords = currentTexCoords + deltaTexCoords;
    float afterDepth  = currentDepthMapValue - currentLayerDepth;
    float beforeDepth = texture(depthMap, prevTexCoords).r - currentLayerDepth + layerDepth;
    float weight = afterDepth / (afterDepth - beforeDepth);
    weight = clamp(weight, 0.0, 1.0);
    vec2 finalTexCoords = prevTexCoords * weight + currentTexCoords * (1.0 - weight);

    // compute final height (in [0..1] from heightmap) — useful for intersection point
    float finalHeightMap = mix(texture(depthMap, prevTexCoords).r, texture(depthMap, currentTexCoords).r, weight);
    outHeight = finalHeightMap * heightScale;   // scale to world units (approx)

    return finalTexCoords;
}

void main()
{
    // compute view dir in tangent space (you already had this)
    vec3 viewDirTS = normalize(fs_in.TangentViewPos - fs_in.TangentFragPos);
    vec2 texCoords = fs_in.TexCoords;
    
    float intersectionHeight;
    texCoords = ParallaxMapping(fs_in.TexCoords, viewDirTS, intersectionHeight);       
    if(texCoords.x > 1.0 || texCoords.y > 1.0 || texCoords.x < 0.0 || texCoords.y < 0.0)
        discard;

    // sample normal & color using displaced UV (same as your current shader)
    vec3 normalTS = texture(normalMap, texCoords).rgb;
    normalTS = normalize(normalTS * 2.0 - 1.0);
    vec3 color = texture(diffuseMap, texCoords).rgb;

    // ------- INTERIOR CUBEMAP SAMPLING -------
    // Compute intersection point in tangent-space:
    // map UV [0,1] -> plane XY [-1,1] (your quad sits in -1..1 in renderQuad)
    vec2 planeXY = (texCoords - 0.5) * 2.0;        // assumes quad spans [-1,1] x/y
    float zTS = -intersectionHeight;              // pull 'into' the surface along -Z
    vec3 intersectionTS = vec3(planeXY.x, planeXY.y, zTS);

    // camera position in tangent-space (point)
    vec3 camPosTS = fs_in.TangentViewPos;         // TBN_inv * viewPos was computed in VS
    // build direction from intersection to camera (tangent-space)
    vec3 sampleDirTS = normalize(camPosTS - intersectionTS);

    // transform direction to world if necessary
    vec3 sampleDirWorld;
    if (cubeIsTangentSpace)
        sampleDirWorld = sampleDirTS;            // cubemap was baked in tangent space
    else
        sampleDirWorld = normalize(fs_in.TBN * sampleDirTS); // convert to world-space using TBN (tangent->world)

    // fetch interior color from cubemap
    vec3 interiorColor = texture(interiorCube, sampleDirWorld).rgb;

    // lighting in world or tangent as you prefer — here we use simple shading with tangent-space normal transformed to world
    vec3 normalWorld = normalize(fs_in.TBN * normalTS);
    vec3 lightDirWorld = normalize(lightPos - fs_in.FragPos); // keep your original lighting code if you prefer
    // For demo, simply modulate by interior color
    vec3 ambient = 0.1 * interiorColor;
    vec3 diffuse = max(dot(normalWorld, normalize(vec3(0.0,0.0,1.0))), 0.0) * interiorColor; // placeholder
    vec3 finalColor = ambient + diffuse;

    // combine (you may prefer to mix interiorColor with your diffuseMap color)
    FragColor = vec4(finalColor, 1.0);
}