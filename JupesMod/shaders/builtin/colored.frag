#version 460 core

out vec4 FragColor;
in vec2 TexCoords;
in vec3 WorldPos;
in vec3 Normal;

// Material Maps
uniform sampler2D AlbedoMap;

// Directional light properties
vec3 lightDirection = normalize(vec3(-1.0, -1.0, -1.0)); // Adjust the light direction as needed
vec3 lightColor = vec3(1.0, 1.0, 1.0); // Adjust the light color as needed

void main()
{
    // Sample the albedo map and extract the transparency (alpha) value
    vec4 albedoWithTransparency = texture(AlbedoMap, TexCoords);
    float transparency = albedoWithTransparency.a;

    // Calculate the diffuse lighting contribution
    float diff = max(dot(normalize(Normal), lightDirection), 0.0);
    vec3 diffuse = diff * lightColor;

    // Calculate the final color by multiplying albedo with the lighting
    vec3 finalColor = albedoWithTransparency.rgb * (diffuse + vec3(0.2)); // Add ambient lighting

    // Set the alpha (transparency) component of the output color
    FragColor = vec4(finalColor, transparency);
}
