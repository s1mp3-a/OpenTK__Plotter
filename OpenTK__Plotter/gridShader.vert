#version 450

layout(location = 0) in vec3 aPos;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

out float amogus;

void main() {
    gl_Position = vec4(aPos, 1.0) * model * view * projection;
    amogus = aPos.y;
}
