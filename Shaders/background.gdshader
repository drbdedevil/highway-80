shader_type canvas_item;

void fragment() {
    vec2 res = 1.0 / SCREEN_PIXEL_SIZE;
    vec2 p = vec2(FRAGCOORD.x, res.y - FRAGCOORD.y);

    float iTime = TIME;
    vec3 iResolution = vec3(res, 1.0);

    vec3 c1 = vec3(43.0/255.0, 15.0/255.0, 84.0/255.0);
    vec3 c2 = vec3(171.0/255.0, 31.0/255.0, 101.0/255.0);
    vec3 c3 = vec3(255.0/255.0, 79.0/255.0, 105.0/255.0);
    vec3 c4 = vec3(255.0/255.0, 247.0/255.0, 248.0/255.0);
    vec3 c5 = vec3(255.0/255.0, 129.0/255.0, 66.0/255.0);
    vec3 c6 = vec3(255.0/255.0, 218.0/255.0, 69.0/255.0);
    vec3 c7 = vec3(51.0/255.0, 104.0/255.0, 220.0/255.0);
    vec3 c8 = vec3(73.0/255.0, 231.0/255.0, 236.0/255.0);

    vec3 sky    = c1;
    vec3 split1 = c5;
    vec3 split2 = c6;
    vec3 out1   = c2;
    vec3 out2   = c3;
    vec3 road1  = c1;
    vec3 road2  = c1;
    vec3 lines  = c7;

    vec3 q = iResolution;
    vec3 d = vec3(p - 0.5 * q.xy, q.y) / q.y;

    q = d / (0.1 - d.y);
    float a = iTime;
    float k = sin(0.2 * a);

    float t = q.x - 0.05 * k * k * k * q.z * q.z;
    q.x = t * t;
    float w = q.x;

    vec3 color;
    if (d.y > 0.04) {
        color = sky;
    } else {
        if (sin(4.0 * q.z + 40.0 * a) > 0.0) {
            if (w > 2.0)
                color = out1;
            else if (w > 1.2)
                color = split1;
            else
                color = road1;
        } else {
            if (w > 2.0)
                color = out2;
            else if (w > 1.2)
                color = split2;
            else
                color = (w > 0.004) ? road2 : lines;
        }
    }

    COLOR = vec4(color, 1.0);
}