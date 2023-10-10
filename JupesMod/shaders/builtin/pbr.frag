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

uniform samplerCube irradianceMap;
uniform samplerCube backgroundMap;

// lights
uniform vec3 lightPositions;
uniform vec3 lightColors;
uniform vec3 viewPos;

uniform float emissiveStrength;
uniform float gammaCubemap;
uniform float interpolation;
uniform float luminousStrength;
uniform float specularStrength;

const float PI = 3.14159265359;

vec3 LuminousCubemap(vec3 base)
{
    vec3 envColor = pow(base, vec3(1.0/ (gammaCubemap + 1.0) ));
    vec3 envColorLum = pow(base, vec3(1.0/gammaCubemap));

    return mix(envColor, envColorLum, interpolation);
}

float DistributionGGX(float NdotH, float roughness)
{
    float a = roughness * roughness;
    float a2 = a * a;
    float denom = NdotH * NdotH * (a2 - 1.0) + 1.0;
    denom = PI * denom * denom;
    return a2 / max(denom, 0.0000001);
}

float GeometrySchlickGGX(float NdotV, float roughness)
{
    float r = (roughness + 1.0);
    float k = (r*r) / 8.0;

    float nom   = NdotV;
    float denom = NdotV * (1.0 - k) + k;

    return nom / denom;
}

float GeometrySmith(float NdotV, float NdotL, float roughness)
{
    float r = roughness + 1.0;
    float k = (r * r) / 8.0;
    float ggx1 = NdotV / (NdotV * (1.0 - k) + k);
    float ggx2 = NdotL / (NdotL * (1.0 - k) + k);
    return ggx1 * ggx2;
}

vec3 fresnelSchlick(float HdotV, vec3 baseReflectivity)
{
    return baseReflectivity + (1.0 - baseReflectivity) * pow(1.0 - HdotV, 5.0);
}

vec3 fresnelSchlickRoughness(float HdotV, vec3 F0, float roughness)
{
    return F0 + (max(vec3(1.0 - roughness), F0) - F0) * pow(1.0 - HdotV, 5.0);
}

vec3 elimineAlpha(sampler2D tex)
{
    vec4 TextureAlpha = texture(tex, TexCoords);
    if(TextureAlpha.a <= 0.1)
        discard;
    return TextureAlpha.rgb;
}

vec3 getNormalFromMap()
{
    vec3 tangentNormal = texture(NormalMap, TexCoords).xyz * 2.0 - 1.0;

    vec3 Q1  = dFdx(WorldPos);
    vec3 Q2  = dFdy(WorldPos);
    vec2 st1 = dFdx(TexCoords);
    vec2 st2 = dFdy(TexCoords);

    vec3 N   = normalize(Normal);
    vec3 T  = normalize(Q1*st2.t - Q2*st1.t);
    vec3 B  = -normalize(cross(N, T));
    mat3 TBN = mat3(T, B, N);

    return normalize(TBN * tangentNormal);
}

void main()
{
    // Material properties
    vec3 albedo = texture(AlbedoMap, TexCoords).rgb;
    vec3 emissive = pow(elimineAlpha(EmissiveMap), vec3(2.2));
    vec3 metallicRoughnessAo = texture(AmbienteRoughnessMetallic, TexCoords).rgb;
    float metallic = metallicRoughnessAo.b;
    float roughness = metallicRoughnessAo.g;
    float ao = metallicRoughnessAo.r;

    // Input lighting data
    vec3 N = getNormalFromMap();
    vec3 V = normalize(viewPos - WorldPos);
    vec3 R = reflect(-V, N);

    // Reflectivity
    vec3 baseReflectivity = mix(vec3(0.04), albedo, metallic);

    // Initialize output color
    vec3 Lo = vec3(0.0);

    // Lighting calculations
    vec3 L = normalize(lightPositions - WorldPos);
    vec3 H = normalize(V + L);
    float distance = length(lightPositions - WorldPos);
    float attenuation = 1.0 / (distance * distance);
    vec3 radiance = lightColors * attenuation;

    float NdotV = max(dot(N, V), 0.00001);
    float NdotL = max(dot(N, L), 0.00001);
    float HdotV = max(dot(H, V), 0.0);
    float NdotH = max(dot(N, H), 0.0);

    // Microfacet calculations
    float D = DistributionGGX(NdotH, roughness);
    float G = GeometrySmith(NdotV, NdotL, roughness);
    vec3 F = fresnelSchlickRoughness(NdotV, baseReflectivity, roughness);

    vec3 specular = D * G * F / (4.0 * NdotV * NdotL);
    float nNdotL = max(dot(N, L), 0.0);

    vec3 kD = (1.0 - F) * (1.0 - metallic);

    Lo += (kD * albedo / PI + specular) * radiance * nNdotL;

    // Calculate irradiance and diffuse
    vec3 irradiance = texture(irradianceMap, N).rgb;
    vec3 diffuse = irradiance * albedo;

    // Sample pre-filtered map for IBL specular
    const float MAX_REFLECTION_LOD = 4.0;
    vec3 prefilteredColor = textureLod(backgroundMap, R, roughness * MAX_REFLECTION_LOD).rgb;
    prefilteredColor = LuminousCubemap(prefilteredColor);

    vec2 brdf = vec2(luminousStrength, specularStrength);
    specular += prefilteredColor * (F * brdf.x + brdf.y);

    // Ambient lighting
    vec3 ambient = (kD * diffuse + specular) * ao;

    vec3 emissiveContribution = emissive * emissiveStrength; //* 2.0; 
    // Final color
    vec3 color = ambient + emissiveContribution + Lo;

    // HDR tonemapping
    color = color / (color + vec3(1.0));

    // Gamma correction
    color = pow(color, vec3(1.0/2.2));

    FragColor = vec4(color, 1.0);
}
