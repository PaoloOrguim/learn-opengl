#version 330 core
out vec4 FragColor;

//in vec3 ourColor;
in vec3 ourPosition;

void main()
{
    //FragColor = vec4(ourColor, 1.0f);
    FragColor = vec4(ourPosition, 1.0); // Position is interpolated giving the triangle the different colors
    // Bottom-left coordinate of the triangle is (-0.5f, -0.5f, 0.0f), negative values are clamped to 0.0f
    // The values only start becoming positive (therefore not black) after the center of the sides.
}