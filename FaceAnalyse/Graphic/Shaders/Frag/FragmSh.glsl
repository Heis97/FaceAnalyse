#version 460 core
uniform vec3 LightPosition_world;
uniform vec3 MaterialDiffuse;
uniform vec3 MaterialAmbient;
uniform vec3 MaterialSpecular;
uniform float lightPower;
uniform sampler2D textureSample;
uniform int textureVis;

in GS_FS_INTERFACE
{
vec3 Position_world;
vec3 Color;
vec3 Normal_camera;
vec3 EyeDirection_camera;
vec3 LightDirection_camera;
vec2 TextureUV;
}fs_in;
out vec4 color;
void main() {
	
	float distance = length( LightPosition_world - fs_in.Position_world );
	vec3 n = normalize( fs_in.Normal_camera );
	vec3 l = normalize( fs_in.LightDirection_camera );
	float cosTheta = clamp( dot( n,l ), 0,1 );
	vec3 E = normalize(fs_in.EyeDirection_camera);
	vec3 R = reflect(-l,n);
	float cosAlpha = clamp( dot( E,R ), 0,1 );

	vec3 LightColor = vec3(1.0, 1.0, 1.0);
	float LightPower = lightPower;

	vec3 MaterialDiffuseColor= vec3(1.0, 1.0, 1.0);
	vec3 MaterialAmbientColor = MaterialAmbient;
	vec3 MaterialSpecularColor = MaterialSpecular;
	if(textureVis == 1)
	{
		MaterialDiffuseColor = texture(textureSample,  fs_in.TextureUV).xyz;
		MaterialSpecularColor = 0.2*MaterialDiffuseColor;
	}
	else
	{
	    MaterialDiffuseColor = vec3(1.0, 1.0, 1.0);
		MaterialSpecularColor = 0.2*MaterialDiffuseColor;
	}
	color.xyz = MaterialAmbientColor + MaterialDiffuseColor * LightColor * LightPower * cosTheta / (distance*distance) +MaterialSpecularColor * LightColor * LightPower * pow(cosAlpha,5) / (distance*distance);
	color.w = 1.0;

}