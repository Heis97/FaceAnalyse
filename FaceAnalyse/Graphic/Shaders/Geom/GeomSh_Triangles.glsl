#version 430 core
layout (triangles, invocations = 1) in;
layout (triangle_strip, max_vertices = 3) out;
layout(rgba32f, binding = 1) uniform  image2D landmark2d_data;
layout(rgba32f, binding = 2) uniform  image2D landmark3d_data;
uniform vec3 LightPosition_world;
uniform mat4 VPs[4];
uniform mat4 Vs[4];
uniform mat4 Ps[4];
uniform vec2 MouseLoc;
uniform vec2 MouseLocGL;
uniform int comp_proj;

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

void compProjection(mat4 VP)
{
	vec4 A = VP * vec4(vs_out[0].vertexPosition_world, 1.0);
	vec4 B = VP * vec4(vs_out[1].vertexPosition_world, 1.0);
	vec4 C = VP * vec4(vs_out[2].vertexPosition_world, 1.0);
	
	for (int i = 0; i<imageSize(landmark2d_data).x; i++)
	{
		float prev_z = imageLoad(landmark3d_data, ivec2(i, 0)).z;
		vec2 P_2d = imageLoad(landmark2d_data, ivec2(i, 0)).xy;
		vec2 P_3d = (VP * vec4(P_2d,0, 1.0)).xy;
		if (checkPointInTriangle(A.xy, B.xy, C.xy, P_3d))
		{
			vec4 p3d = inverse(VP) * A;
			if (p3d.z > prev_z)
			{
				imageStore(landmark3d_data, ivec2(i, 0), p3d);
			}
			
		}
	}
	
}

void main() 
{
	gl_ViewportIndex = gl_InvocationID;
	bool selected_triangle = checkPointInTriangle(
		(VPs[gl_InvocationID] * vec4(vs_out[0].vertexPosition_world, 1.0)).xy,
		(VPs[gl_InvocationID] * vec4(vs_out[1].vertexPosition_world, 1.0)).xy,
		(VPs[gl_InvocationID] * vec4(vs_out[2].vertexPosition_world, 1.0)).xy,
	MouseLocGL);
	if (comp_proj == 1)
	{
		compProjection(Vs[gl_InvocationID]);
	}
	


   for (int i = 0; i < gl_in.length(); i++)
   { 
	    
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
