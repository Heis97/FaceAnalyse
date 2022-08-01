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
uniform int render_count;
uniform int show_faces;
uniform int inv_norm;


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

vec3 comp_norm(vec3 _A, vec3 _B, vec3 _C)
{
	vec3 v1 = _B - _A;
	vec3 v2 = _C - _A;
	return(cross(v1, v2));
}

vec4 project_point_xy(vec4 _A, vec4 n, vec2 _P)
{
	if (n.z == 0)
	{
		return(vec4(_P, 0, 1));
	}
	float d = _A.x * n.x + _A.y * n.y + _A.z * n.z;
	float z = (-d - _P.x * n.x - _P.y * n.y) / n.z;
	return(vec4(_P, z, 1));
}

void compProjection(mat4 VP,vec4 A, vec4 B, vec4 C,vec4 n)
{
	mat4 inv_VP = inverse(VP);	
	for (int i = 0; i<imageSize(landmark2d_data).x; i++)
	{
		vec2 P_2d = imageLoad(landmark2d_data, ivec2(i, 0)).xy;
		vec2 P_3d = (VP * vec4(P_2d,0, 1.0)).xy;
		if (checkPointInTriangle(A.xy, B.xy, C.xy, P_3d))
		{
			vec4 p_proj = project_point_xy(A, n, P_3d);
			vec4 p3d = inv_VP * A;
			imageStore(landmark3d_data, ivec2(i, 0), p3d);
		}
		
	}	
	imageStore(landmark3d_data, ivec2(imageSize(landmark2d_data).x - 1, 0), vec4(render_count, 0, 0, 0));
}


void main() 
{
	gl_ViewportIndex = gl_InvocationID;
	vec4 A = Vs[gl_InvocationID] * vec4(vs_out[0].vertexPosition_world, 1.0);
	vec4 B = Vs[gl_InvocationID] * vec4(vs_out[1].vertexPosition_world, 1.0);
	vec4 C = Vs[gl_InvocationID] * vec4(vs_out[2].vertexPosition_world, 1.0);
	vec4 n = vec4(normalize((comp_norm(A.xyz, B.xyz, C.xyz))), 0);
	bool selected_triangle = checkPointInTriangle(A.xy,B.xy,C.xy,MouseLocGL);

	if (comp_proj == 1)
	{
		compProjection(Vs[gl_InvocationID],A,B,C,n);
	}
	


   for (int i = 0; i < gl_in.length(); i++)
   { 
	    
        gl_Position = VPs[gl_InvocationID] * vec4(vs_out[i].vertexPosition_world, 1.0);

	    fs_in.Position_world = vs_out[i].vertexPosition_world;
	    vec3 vertexPosition_camera = (Vs[gl_InvocationID] * vec4(vs_out[i].vertexPosition_world, 1.0)).xyz;
	    fs_in.EyeDirection_camera = vec3(0,0,0) - vertexPosition_camera;
	    vec3 LightPosition_camera = ( Vs[gl_InvocationID] * vec4(LightPosition_world,1)).xyz;
	    fs_in.LightDirection_camera = LightPosition_camera + fs_in.EyeDirection_camera;
	    fs_in.Normal_camera = ( Vs[gl_InvocationID] * vec4(vs_out[i].vertexNormal_world, 0)).xyz;
		if (show_faces == 1)
		{
			
			fs_in.Normal_camera = n.xyz;
			if (inv_norm == 1)
			{
				fs_in.Normal_camera *= -1;
			}
		}
	    fs_in.Color = vs_out[i].vertexColor;
		fs_in.TextureUV = vs_out[i].vertexTexture;
		if(selected_triangle)
		{
		    fs_in.Color = vec3(0,1,0);
		}

	    EmitVertex();
	}
}
