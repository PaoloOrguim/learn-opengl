#version 330 core
layout (location = 0) in vec3 aPos;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec2 aTexCoords;
layout (location = 3) in vec3 aTangent;
layout (location = 4) in vec3 aBitangent;

out VS_OUT {
    vec3 FragPos;
    vec2 TexCoords;
    vec3 TangentLightPos;
    vec3 TangentViewPos;
    vec3 TangentFragPos;
    mat3 TBN;         // NEW: tangent->world
} vs_out;

uniform mat4 projection;
uniform mat4 view;
uniform mat4 model;

uniform vec3 lightPos;
uniform vec3 viewPos;

void main()
{
    // world-space fragment position
    vs_out.FragPos = vec3(model * vec4(aPos, 1.0));   
    vs_out.TexCoords = aTexCoords;   
    
    // build orthonormal TBN (columns = T, B, N)
    vec3 T = normalize(mat3(model) * aTangent);
    vec3 B = normalize(mat3(model) * aBitangent);
    vec3 N = normalize(mat3(model) * aNormal);
    mat3 TBN_world = mat3(T, B, N);         // tangent -> world
    vs_out.TBN = TBN_world;

    // To get world -> tangent, multiply by transpose(TBN_world) (assuming TBN is orthonormal)
    mat3 worldToTangent = transpose(TBN_world);

    vs_out.TangentLightPos = worldToTangent * lightPos;
    vs_out.TangentViewPos  = worldToTangent * viewPos;
    vs_out.TangentFragPos  = worldToTangent * vs_out.FragPos;
    
    gl_Position = projection * view * model * vec4(aPos, 1.0);
}