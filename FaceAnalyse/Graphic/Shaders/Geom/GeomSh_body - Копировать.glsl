/*#version 460 core
layout (triangles, invocations = 1) in;
layout (triangle_strip, max_vertices = 3) out;*/

uniform vec3 LightPosition_world;
uniform mat4 MVPs[4];
uniform mat4 Ms[4];
uniform mat4 Vs[4];
uniform mat4 Ps[4];
uniform vec2 MouseLoc;
uniform vec2 MouseLocGL;

in VS_GS_INTERFACE
{
vec3 vertexPosition_model;
vec3 vertexNormal_model;
vec3 vertexColor;
vec2 vertexTexture;
int InstanceID;
}vs_out[];

out GS_FS_INTERFACE
{
vec3 Position_world;
vec3 Color;
vec3 Normal_camera;
vec3 EyeDirection_camera;
vec3 LightDirection_camera;
vec2 TextureUV;
} fs_in;

void main() 
{
   for (int i = 0; i < gl_in.length(); i++)
   { 
	    gl_ViewportIndex = gl_InvocationID;
        gl_Position = MVPs[gl_InvocationID]*Ms[gl_InvocationID] * vec4(vs_out[i].vertexPosition_model, 1.0);
	    fs_in.Position_world = (Ms[gl_InvocationID] * vec4(vs_out[i].vertexPosition_model,1)).xyz;
	    vec3 vertexPosition_camera = ( Vs[gl_InvocationID] * Ms[gl_InvocationID] * vec4(vs_out[i].vertexPosition_model,1)).xyz;
	    fs_in.EyeDirection_camera = vec3(0,0,0) - vertexPosition_camera;
	    vec3 LightPosition_camera = ( Vs[gl_InvocationID] * vec4(LightPosition_world,1)).xyz;
	    fs_in.LightDirection_camera = LightPosition_camera + fs_in.EyeDirection_camera;
	    fs_in.Normal_camera = ( Vs[gl_InvocationID] * Ms[gl_InvocationID] * vec4(vs_out[i].vertexNormal_model,0)).xyz;
	    fs_in.Color = vs_out[i].vertexColor;
		fs_in.TextureUV = vs_out[i].vertexTexture;
	    EmitVertex();
	}
}
