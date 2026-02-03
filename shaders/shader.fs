#version 330 core
out vec4 FragColor;

in vec3 ourColor;
in vec2 TexCoord;

// texture samplers
uniform sampler2D texture1; // Container
uniform sampler2D texture2; // Awesomeface

void main()
{
	// linearly interpolate between both textures (80% container, 20% awesomeface)
	FragColor = mix(texture(texture1, TexCoord), texture(texture2, vec2(1.0 - TexCoord.x, TexCoord.y)), 0.2);
    // 1.0 - TexCoord.x is used to flip the awesomeface texture horizontally
    // Simply doing -TexCoord.x would move it out of the texture coordinates range [0,1]
    // In this case it would still work due to the GL_REPEAT wrapping mode set earlier
    // But that is just a coincidence and that "solution" should be avoided
}