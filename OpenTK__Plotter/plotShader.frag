#version 450

out vec4 outputColor;
in float amogus;

float map(in float value, in float min1, in float max1, in float min2, in float max2) {
    return min2 + (value - min1) * (max2 - min2) / (max1 - min1);
}


vec3 hsv2rgb(in vec3 c)
{
    vec4 K = vec4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    vec3 p = abs(fract(c.xxx + K.xyz) * 6.0 - K.www);
    return c.z * mix(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
}

void main() {
    float abobus = map(amogus, 1f, 9f, 0.2f, 1f);
    vec3 s = vec3(1-abobus, 1f, 1f);

    vec4 aboba = vec4(hsv2rgb(s), 1);
    outputColor = aboba;
}


