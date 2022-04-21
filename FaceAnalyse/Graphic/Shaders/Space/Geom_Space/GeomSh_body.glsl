#version 460 core
layout (triangles, invocations = 4) in;
layout (triangle_strip, max_vertices = 3) out;

uniform vec3 LightPosition_world;
uniform mat4 MVPs[4];
uniform mat4 Ms[4];
uniform mat4 Vs[4];
uniform mat4 Ps[4];
uniform vec2 MouseLoc;
uniform vec2 MouseLocGL;

float[16] matrMul(in float[16] m1,in float[16] m2)
{
	float[16] res = float[](0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0);
	for (int i = 0; i < 4; i++)
    {
        for (int j = 0; j < 4; j++)
        {
            for (int k = 0; k < 4; k++)
            {
                res[i * 4 + j] += m1[i * 4 + k] * m2[k * 4 + j];
            }
        }
    }
	return(res);
}
mat4 matrMMul(in mat4 m1,in mat4 m2)
{
	mat4 res = mat4(0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0);
	for (int i = 0; i < 4; i++)
    {
        for (int j = 0; j < 4; j++)
        {
            for (int k = 0; k < 4; k++)
            {
                res[i][j] += m1[i][k] * m2[k][j];
            }
        }
    }
	return(res);
}
vec4 matrVecMul(in float[16] m1,in vec4 m2)
{
	vec4 res = vec4(0,0,0,0);
	for (int i = 0; i < 4; i++)
    {        
        for (int k = 0; k < 4; k++)
        {
            res[i] += m1[i * 4 + k] * m2[k];
        }        
    }
	return(res);
}

vec4 matrVecMul(in mat4 m1,in vec4 m2)
{
	vec4 res = vec4(0,0,0,0);
	for (int i = 0; i < 4; i++)
    {        
        for (int k = 0; k < 4; k++)
        {
            res[i] += m1[i][k] * m2[k];
        }        
    }
	return(res);
}

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

int checkPointInTriangle(vec2 _A,vec2 _B,vec2 _C,vec2 _P)
{
	int result = 0;
	vec2 P = _P - _A;
	vec2 B = _B - _A;
	vec2 C = _C - _A;
	float m = (P.x * B.y - B.x * P.y) / (C.x * B.y - B.x * C.y);
	if(m>=0 && m<=1)
    {
        float l = (P.x - m * C.x) / B.x;
        if (l >= 0 && (m+l) <= 1)
        {
            result = 1;
        }
    }
    return(result);
}



void main() 
{
	int glID = gl_InvocationID;
	
	gl_ViewportIndex = glID;
	vec4 positGL[4];
	for (int i = 0; i < gl_in.length(); i++)
    { 
		vec4 vertexPosition_view = matrVecMul(Ms[glID], vec4(vs_out[i].vertexPosition_model, 1.0));
		vertexPosition_view.x+=5*vs_out[i].InstanceID;
		positGL[i] = matrVecMul(MVPs[glID],vertexPosition_view);
	}

    for (int i = 0; i < gl_in.length(); i++)
    { 			
        gl_Position = positGL[i];
	    fs_in.Position_world = gl_Position.xyz;
		
		vec4 pos_cam = matrVecMul( Vs[glID] , gl_Position);
	    vec3 vertexPosition_camera = (matrVecMul( Vs[glID] ,matrVecMul( Ms[glID],gl_Position))).xyz;       //model matrix!
	    fs_in.EyeDirection_camera= vec3(0,0,0) - vertexPosition_camera;
	    vec3 LightPosition_camera = matrVecMul(Vs[gl_InvocationID] , vec4(LightPosition_world,1.0)).xyz;


	    fs_in.LightDirection_camera = LightPosition_camera + fs_in.EyeDirection_camera;
		mat4 PV = matrMMul(Ps[glID],Vs[glID]);
	    fs_in.Normal_camera = matrVecMul ( PV , vec4(vs_out[i].vertexNormal_model,0)).xyz;


	    fs_in.Color = vs_out[i].vertexColor;
		fs_in.TextureUV = vs_out[i].vertexTexture;

		
	    EmitVertex();
	 }
}



