#version 430 core
layout (triangles, invocations = 1) in;
layout (triangle_strip, max_vertices = 3) out;
uniform vec3 LightPosition_world;
uniform mat4 VPs[4];
uniform mat4 Vs[4];
uniform mat4 Ps[4];
uniform vec2 MouseLoc;
uniform vec2 MouseLocGL;

in VS_GS_INTERFACE
{
vec3 vertexPosition_world;
vec3 vertexNormal_world;
vec3 vertexColor;
vec2 vertexTexture;
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

bool checkPointInTriangle(vec2 _A,vec2 _B,vec2 _C,vec2 _P)
{
	vec2 P = _P - _A;
	vec2 B = _B - _A;
	vec2 C = _C - _A;
	float m = (P.x * B.y - B.x * P.y) / (C.x * B.y - B.x * C.y);
	if(m>=0 && m<=1)
    {
        float l = (P.x - m * C.x) / B.x;
        if (l >= 0 && (m+l) <= 1)
        {
            return (true);
        }
    }
    return(false);
}

void main() 
{
	bool selected_triangle = checkPointInTriangle(
	vs_out[0].vertexTexture,
	vs_out[1].vertexTexture,
	vs_out[2].vertexTexture,
	MouseLoc);


   for (int i = 0; i < gl_in.length(); i++)
   { 
	    gl_ViewportIndex = gl_InvocationID;
        gl_Position = VPs[gl_InvocationID] * vec4(vs_out[i].vertexPosition_world, 1.0);

	    fs_in.Position_world = vs_out[i].vertexPosition_world;
	    vec3 vertexPosition_camera = (Vs[gl_InvocationID] * vec4(vs_out[i].vertexPosition_world, 1.0)).xyz;
	    fs_in.EyeDirection_camera = vec3(0,0,0) - vertexPosition_camera;
	    vec3 LightPosition_camera = ( Vs[gl_InvocationID] * vec4(LightPosition_world,1)).xyz;
	    fs_in.LightDirection_camera = LightPosition_camera + fs_in.EyeDirection_camera;
	    fs_in.Normal_camera = ( Vs[gl_InvocationID] * vec4(vs_out[i].vertexNormal_world, 1.0)).xyz;
	    fs_in.Color = vs_out[i].vertexColor;
		fs_in.TextureUV = vs_out[i].vertexTexture;
		if(selected_triangle)
		{
		    fs_in.Color = vec3(0,1,0);
		}
	    EmitVertex();
	}
}
