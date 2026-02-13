#version 330 core

out vec4 FragColor;

struct Material {
    sampler2D diffuse;  // We remove the ambient material color vector since the ambient color is equal to the diffuse color
                        // now that we control ambient with the light. So there's no need to store it separately.
    sampler2D specular;
    sampler2D emission; // Give the objects a glowing effect
    float shininess;
};

struct Light {
    vec3 position;

    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
};

in vec3 FragPos;  
in vec3 Normal;
in vec2 texCoords;
  
uniform vec3 viewPos;
uniform Material material;
uniform Light light;
uniform float time;

void main()
{
    // ambient
    vec3 ambient = light.ambient * texture(material.diffuse, texCoords).rgb;
  	
    // diffuse 
    vec3 norm = normalize(Normal);
    vec3 lightDir = normalize(light.position - FragPos);
    float diff = max(dot(norm, lightDir), 0.0);
    vec3 diffuse = light.diffuse * diff * texture(material.diffuse, texCoords).rgb;
    
    // specular
    vec3 viewDir = normalize(viewPos - FragPos);
    vec3 reflectDir = reflect(-lightDir, norm);  
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), material.shininess);
    vec3 specular = light.specular * spec * texture(material.specular, texCoords).rgb;

    // emission
    //vec3 emission = texture(material.emission, texCoords).rgb;
    // Alex Dee's comment on learnopengl
    vec3 emission = vec3(0.0f);
    if (texture(material.specular, texCoords).r == 0.0){    // rough check for blackbox inside spec texture
        emission = texture(material.emission, texCoords + vec2(0.0, time * 0.25f)).rgb;     // move emission texture
        emission = emission * (sin(time) * 0.5 + 0.5) * 2.0;                            // Fade emission texture
    }

    vec3 result = ambient + diffuse + specular + emission;
    FragColor = vec4(result, 1.0);
}