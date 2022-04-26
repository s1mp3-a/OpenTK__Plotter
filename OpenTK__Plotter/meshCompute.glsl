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

float map(float value, float min1, float max1, float min2, float max2) {
    return min2 + (value - min1) * (max2 - min2) / (max1 - min1);
}

void main() {
    //TODO
    uint pos = gl_GlobalInvocationID.x + gl_GlobalInvocationID.y * gl_NumWorkGroups.x;

    float x = gl_GlobalInvocationID.x;
    float z = gl_GlobalInvocationID.y;
    
    float xOff = 2f * sin(timer) + 4f;
    float zOff = 6f * cos(timer) + 8f;

    float vx = x * xSpan / (gl_NumWorkGroups.x - 1);
    float vz = z * zSpan / (gl_NumWorkGroups.y - 1);

     vx = map(vx, 0f, xSpan, (xOff-(5f+0.1f)/2f), (xOff+(5f+0.1f)/2f));
     vz = map(vz, 0f, zSpan, (zOff-(3f+0.1f)/2f), (zOff+(3f+0.1f)/2f));
    
    float cx = 1f - pow(abs(2f*(vx-xOff) / 5f), 1f);
    cx = clamp(cx, 0f, 1f);
    float cz = 1f - pow(abs(2f*(vz-zOff) / 3f), .7f);
    cz = clamp(cz, 0f, 1f);

    float vy = 9f * pow(cx, 1f/1f) * pow(cz, 1f/.7f);
    
    vx = vx < 0 ? 0 : vx;
    vx = vx > xSpan ? xSpan : vx;

    verts.data[pos].x = vx;
    verts.data[pos].y = vy;
    verts.data[pos].z = vz;
}
