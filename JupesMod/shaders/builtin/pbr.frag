#version 460 core

out vec4 FragColor;
in vec2 TexCoords;
in vec3 WorldPos;
in vec3 Normal;

// material Maps
uniform sampler2D AlbedoMap;
uniform sampler2D NormalMap;
uniform sampler2D AmbienteRoughnessMetallic;
uniform sampler2D EmissiveMap;

// lights
uniform vec3 lightPositions;
uniform vec3 lightColors;
uniform vec3 viewPos;

const float PI = 3.14159265359;


//----------------------------------------------------
float DistributionGGX(float NdotH, float roughness)
{
    float a = roughness * roughness;
    float a2 = a * a;
    float denom = NdotH * NdotH * (a2 - 1.0) + 1.0;
    denom = PI * denom * denom;
    return a2 / max(denom, 0.0000001);
}
//----------------------------------------------------
float GeometrySchlickGGX(float NdotV, float roughness)
{
    float r = (roughness + 1.0);
    float k = (r*r) / 8.0;

    float nom   = NdotV;
    float denom = NdotV * (1.0 - k) + k;

    return nom / denom;
}
//----------------------------------------------------
float GeometrySmith(float NdotV, float NdotL, float roughness)
{
    float r = roughness + 1.0;
    float k = (r * r) / 8.0;
    float ggx1 = NdotV / (NdotV * (1.0 - k) + k);
    float ggx2 = NdotL / (NdotL * (1.0 - k) + k);
    return ggx1 * ggx2;
}
//----------------------------------------------------
vec3 fresnelSchlick(float HdotV, vec3 baseReflectivity)
{
    return baseReflectivity + (1.0 - baseReflectivity) * pow(1.0 - HdotV, 5.0);
}
//----------------------------------------------------
vec3 elimineAlpha(sampler2D tex)
{
    vec4 TextureAlpha = texture(tex, TexCoords);
    if(TextureAlpha.a <= 0.1)
        discard;
    return TextureAlpha.rgb;
}
//----------------------------------------------------
void main()
{		
    // material properties
    
    vec3 albedo = pow(elimineAlpha(AlbedoMap), vec3(2.2));

    vec3 emissive = elimineAlpha(EmissiveMap);

    float metallic = texture(AmbienteRoughnessMetallic, TexCoords).b;
    float roughness = texture(AmbienteRoughnessMetallic, TexCoords).g;
    float ao = texture(AmbienteRoughnessMetallic, TexCoords).r;
       
       
    // input lighting data
    vec3 N = normalize(Normal);
    vec3 V = normalize(viewPos - WorldPos);
    vec3 R = reflect(-V, N); 

    vec3 baseReflectivity = mix(vec3(0.04), albedo, metallic);

    // reflectance equation
    vec3 Lo = vec3(0.0);

    vec3 L = normalize(lightPositions - WorldPos);
    vec3 H = normalize(V + L);
    float distance = length(lightPositions - WorldPos);
    float attenuation = 1.0 / (distance * distance);
    vec3 radiance = lightColors * attenuation;

    float NdotV = max(dot(N,V), 0.00001);
    float NdotL = max(dot(N,L), 0.00001);
    float HdotV = max(dot(H,V), 0.0);
    float NdotH = max(dot(N,H), 0.0);

    float D = DistributionGGX(NdotH, roughness);
    float G = GeometrySmith(NdotV, NdotL, roughness);
    vec3 F = fresnelSchlick(HdotV, baseReflectivity);

    vec3 specular = D * G * F;
    specular /= 4.0 * NdotV * NdotL;

    vec3 kD = vec3(1.0) - F;
    kD *= 1.0 - metallic;
    Lo += (kD * albedo / PI + specular) * radiance * NdotL;
    
    vec3 ambient = vec3(0.03) * albedo;

    vec3 color = ambient + Lo;
    
   
    // HDR tonemapping
    color = color / (color + vec3(1.0));

    // gamma correct
    color = pow(color, vec3(1.0/2.2)); 

    FragColor = vec4(color , 1.0);
}