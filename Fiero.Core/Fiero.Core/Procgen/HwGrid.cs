using SFML.Graphics;
using SFML.Graphics.Glsl;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace Fiero.Core
{
    /// <summary>
    /// Hardware-accelerated grid that uses fragment shaders to run GPU-bound code and vectorizes CPU-bound code
    /// </summary>
    public sealed class HardwareGrid : Drawable, IDisposable
    {
        private string _fragmentShader;

        private Shader _shader;
        private readonly Texture _tex;
        private readonly RenderTexture _render;

        public byte[] Pixels { get; }

        public HardwareGrid(int w, int h)
        {
            _tex = new ((uint)w, (uint)h);
            _render = new ((uint)w, (uint)h);
            Pixels = new byte[w * h * 4];
            RecompileShader(String.Empty, String.Empty);
        }

        private void RecompileShader(string userUniforms, string userCode)
        {
            var fragmentShader = @$"
                uniform sampler2D texture;
                {userUniforms}
                void main() {{
                    vec2 pixel_size = 1.0 / vec2(textureSize(texture, 0));
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

                    {userCode}

                    gl_FragColor = frag;
                }}
            ";
            if (String.Equals(_fragmentShader, fragmentShader)) {
                return;
            }
            _shader?.Dispose();
            _shader = Shader.FromString(null, null, fragmentShader);
            _shader.SetUniform("texture", _tex);
            _fragmentShader = fragmentShader;
        }


        public void SetPixel(Coord xy, Color c)
        {
            Pixels[(xy.X + xy.Y * _render.Size.X) * 4 + 0] = c.R;
            Pixels[(xy.X + xy.Y * _render.Size.X) * 4 + 1] = c.G;
            Pixels[(xy.X + xy.Y * _render.Size.X) * 4 + 2] = c.B;
            Pixels[(xy.X + xy.Y * _render.Size.X) * 4 + 3] = c.A;
        }

        public Color GetPixel(Coord xy)
        {
            return new(
                Pixels[(xy.X + xy.Y * _render.Size.X) * 4 + 0],
                Pixels[(xy.X + xy.Y * _render.Size.X) * 4 + 1],
                Pixels[(xy.X + xy.Y * _render.Size.X) * 4 + 2],
                Pixels[(xy.X + xy.Y * _render.Size.X) * 4 + 3]
            );
        }

        public void SetPixels(Func<Coord, Color> setter)
        {
            Parallel.For(0, _tex.Size.X * _tex.Size.Y, xy => {
                var coord = new Coord((int)(xy % _tex.Size.X), (int)(xy / _tex.Size.Y));
                SetPixel(coord, setter(coord));
            });
            Update();
        }

        public void LoadPixels(Image image)
        {
            Array.Copy(image.Pixels, Pixels, image.Pixels.Length);
            Update();
        }

        public Image CopyToImage()
        {
            Update();
            return _tex.CopyToImage();
        }

        public void Convolve(Mat3 kernel, int times = 1, bool preserveAlpha = false)
        {
            RecompileShader($@"
                uniform mat3 kernel;
            ", $@"
                frag = 
                    kernel[0][0] * n7 + kernel[0][1] * n8 + kernel[0][2] * n9 +
                    kernel[1][0] * n4 + kernel[1][1] * n5 + kernel[1][2] * n6 +
                    kernel[2][0] * n1 + kernel[2][1] * n2 + kernel[2][2] * n3 ;
                {(preserveAlpha ? "" : "frag.a = 1;")}
            ");
            _shader.SetUniform(nameof(kernel), kernel);
            for (int i = 0; i < times; i++) {
                Update();
            }
            RecompileShader(String.Empty, String.Empty);
        }

        public void Gabor(float lambda, float theta, float phi, float rho, float gamma)
        {
            var gammaSquared = gamma * gamma;
            var rhoSquared = rho * rho;
            var tau = Math.Tau;
            RecompileShader($@"
                vec2 gabor(vec2 xy) {{
                    vec2 zw = vec2(
                        xy.x * cos({theta:0.0000}) + xy.y * sin({theta:0.0000}),
                        xy.x * sin({theta:0.0000}) + xy.y * cos({theta:0.0000})
                    );
                    return vec2(
                        exp((zw.x * zw.x + {gammaSquared:0.0000} * zw.y * zw.y) / 2 * {rhoSquared:0.0000}) 
                            * cos({tau:0.0000} * zw.x / {lambda:0.0000} + {phi:0.0000}),
                        exp((zw.x * zw.x + {gammaSquared:0.0000} * zw.y * zw.y) / 2 * {rhoSquared:0.0000}) 
                            * sin({tau:0.0000} * zw.x / {lambda:0.0000} + {phi:0.0000})
                    );
                }}
            ", $@"
                frag = vec4(gabor(gl_TexCoord[0].xy), 0, 1);
            ");
            Update();
            RecompileShader(String.Empty, String.Empty);
        }

        private void Update()
        {
            _tex.Update(Pixels);
            using var sprite = new Sprite(_tex);
            _render.Draw(sprite, new(_shader));
            _render.Display();
            using var image = _render.Texture.CopyToImage();
            Array.Copy(image.Pixels, Pixels, image.Pixels.Length);
        }

        public void Draw(RenderTarget target, RenderStates states)
        {
            using var sprite = new Sprite(_tex);
            target.Draw(sprite, states);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _tex.Dispose();
            _render.Dispose();
            _shader.Dispose();
        }
    }
}
