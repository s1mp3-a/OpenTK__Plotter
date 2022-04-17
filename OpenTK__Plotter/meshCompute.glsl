#version 430

struct v3
{
    float x;
    float y;
    float z;
};

layout(std430, binding = 0) buffer dest
{
    v3 data[];
} verts;

layout(local_size_x = 1, local_size_y = 1, local_size_z = 1) in;

uniform  float xSpan;
uniform  float zSpan;
uniform  float timer;

void main() {
    //TODO
    uint pos = gl_GlobalInvocationID.x + gl_GlobalInvocationID.y * gl_NumWorkGroups.x;

    float x = gl_GlobalInvocationID.x;
    float z = gl_GlobalInvocationID.y;

    float vx = x * xSpan / (gl_NumWorkGroups.x - 1);
    float vz = z * zSpan / (gl_NumWorkGroups.y - 1);

    float xOff = 2f * sin(timer) + 4f;
    float zOff = 6f * cos(timer) + 8f;

    float cx = 1f - pow(abs(2f*(vx-xOff) * zSpan / xSpan / 5f), 1f);
    cx = clamp(cx, 0f, 1f);
    float cz = 1f - pow(abs(2f*(vz-zOff) * xSpan / zSpan / 3f), .7f);
    cz = clamp(cz, 0f, 1f);

    float vy = 9f * pow(cx, 1f/1f) * pow(cz, 1f/.7f);

    verts.data[pos].x = vx;
    verts.data[pos].y = vy;
    verts.data[pos].z = vz;
}
