#version 330 core
#define NR_LIGHTS 2
out vec4 FragColor;
  
uniform vec3 objectColor;
uniform vec3 lightColor;
uniform vec3 lightPos[NR_LIGHTS];
uniform vec3 viewPos;

in vec3 Normal;
in vec3 FragPos;

void main()
{
    // ambient lighting
    float ambientStrength = 0.1;
    vec3 ambient = ambientStrength * lightColor;

    vec3 result = ambient;
    vec3 norm = normalize(Normal);
    vec3 viewDir = normalize(viewPos - FragPos);

    for (int i = 0; i < NR_LIGHTS; i++)
    {
        // Diffuse
        vec3 lightDir = normalize(lightPos[i] - FragPos);
        float diff = max(dot(norm, lightDir), 0.0);
        vec3 diffuse = diff * lightColor;

        // Specular
        float specularStrength = 0.5;
        vec3 reflectDir = reflect(-lightDir, norm);
        float spec = pow(max(dot(viewDir, reflectDir), 0.0), 32);
        vec3 specular = specularStrength * spec * lightColor;

        // Accumulate
        result += diffuse + specular;
    }

    result *= objectColor;

    FragColor = vec4(result, 1.0);
}