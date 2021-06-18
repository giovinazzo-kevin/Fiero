
                uniform sampler2D texture;
                
                vec2 gabor(ivec2 index) {
                    float k = (index.x * index.x + index.y * index.y) / (0.500);
                    return vec2(
                        pow(0.500 * 2.718, -k) * cos(3.142 * (index.x * cos(0.500) + index.y * sin(0.500))),
                        pow(0.500f * 2.718f, -k) * sin(3.142 * (index.x * cos(0.500) + index.y * sin(0.500))));
                }
            
                void main() {
                    vec2 texture_size = vec2(textureSize(texture, 0));
                    vec2 pixel_size = 1.0 / texture_size;
                    ivec2 index = ivec2(floor(gl_TexCoord[0].xy.x * pixel_size.x), floor(gl_TexCoord[0].xy.y * pixel_size.y));

                    vec4 frag = texture2D(texture, gl_TexCoord[0].xy);
                    vec4 n7 = texture2D(texture, gl_TexCoord[0].xy + pixel_size * vec2(-1.0, -1.0));
                    vec4 n8 = texture2D(texture, gl_TexCoord[0].xy + pixel_size * vec2( 0.0, -1.0));
                    vec4 n9 = texture2D(texture, gl_TexCoord[0].xy + pixel_size * vec2( 1.0, -1.0));
                    vec4 n4 = texture2D(texture, gl_TexCoord[0].xy + pixel_size * vec2(-1.0,  0.0));
                    vec4 n5 = frag;
                    vec4 n6 = texture2D(texture, gl_TexCoord[0].xy + pixel_size * vec2( 1.0,  0.0));
                    vec4 n1 = texture2D(texture, gl_TexCoord[0].xy + pixel_size * vec2(-1.0,  1.0));
                    vec4 n2 = texture2D(texture, gl_TexCoord[0].xy + pixel_size * vec2( 0.0,  1.0));
                    vec4 n3 = texture2D(texture, gl_TexCoord[0].xy + pixel_size * vec2( 1.0,  1.0));

                    
                frag.xy = gabor(index);
                frag.zw = vec2(1, 1);
            

                    gl_FragColor = frag;
                }
            