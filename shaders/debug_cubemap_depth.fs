#version 330 core
out vec4 FragColor;

in vec2 TexCoords;

uniform samplerCube depthMap;
uniform vec3 lightPos;
uniform float far_plane;

void main()
{
    // Convert quad UV into direction vector
    // Map TexCoords (0→1) to (-1→1)
    vec2 uv = TexCoords * 2.0 - 1.0;

    // We visualize one face (for example +Z face)
    vec3 sampleDir = normalize(vec3(uv.x, uv.y, 1.0));

    float depthValue = texture(depthMap, sampleDir).r;

    // Convert back to world distance
    depthValue *= far_plane;

    FragColor = vec4(vec3(depthValue / far_plane), 1.0);
}