using Silk.NET.OpenGL;
using StbImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Szeminarium1_24_02_17_2
{
    internal class DroneTexture : GlObject
    {
        public uint? Texture { get; private set; }

        private DroneTexture(uint vao, uint vertices, uint colors, uint indices, uint indexArrayLength, GL gl, uint texture = 0)
            : base(vao, vertices, colors, indices, indexArrayLength, gl)
        {
            Texture = texture;
        }

        public static unsafe DroneTexture CreateTexturedDrone(GL gl, string textureResource)
        {
            // Load texture image
            ImageResult imageResult = ReadTextureImage(textureResource);

            uint vao = gl.GenVertexArray();
            gl.BindVertexArray(vao);

            // Define vertices, colors, and texture coordinates
            float[] vertexArray = new float[] {
                // Define your drone vertices here
            };

            float[] textureCoordsArray = new float[] {
                // Define texture coordinates here
            };

            uint[] indexArray = new uint[] {
                // Define indices here
            };

            uint offsetPos = 0;
            uint offsetTexCoords = offsetPos + (3 * sizeof(float));
            uint vertexSize = offsetTexCoords + (2 * sizeof(float));

            // Generate and bind vertex buffer
            uint verticesBuffer = gl.GenBuffer();
            gl.BindBuffer(GLEnum.ArrayBuffer, verticesBuffer);
            gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)vertexArray.AsSpan(), GLEnum.StaticDraw);
            gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, vertexSize, (void*)offsetPos);
            gl.EnableVertexAttribArray(0);

            // Generate and bind texture coordinates buffer
            uint texCoordsBuffer = gl.GenBuffer();
            gl.BindBuffer(GLEnum.ArrayBuffer, texCoordsBuffer);
            gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)textureCoordsArray.AsSpan(), GLEnum.StaticDraw);
            gl.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, vertexSize, (void*)offsetTexCoords);
            gl.EnableVertexAttribArray(2);

            // Generate and bind index buffer
            uint indicesBuffer = gl.GenBuffer();
            gl.BindBuffer(GLEnum.ElementArrayBuffer, indicesBuffer);
            gl.BufferData(GLEnum.ElementArrayBuffer, (ReadOnlySpan<uint>)indexArray.AsSpan(), GLEnum.StaticDraw);

            // Load texture
            uint texture = LoadTexture(gl, imageResult);

            // Unbind buffers
            gl.BindBuffer(GLEnum.ArrayBuffer, 0);
            gl.BindBuffer(GLEnum.ElementArrayBuffer, 0);

            return new DroneTexture(vao, verticesBuffer, texCoordsBuffer, indicesBuffer, (uint)indexArray.Length, gl, texture);
        }

        private static unsafe uint LoadTexture(GL gl, ImageResult imageResult)
        {
            uint texture = gl.GenTexture();
            gl.BindTexture(TextureTarget.Texture2D, texture);

            var textureBytes = (ReadOnlySpan<byte>)imageResult.Data.AsSpan();
            gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, (uint)imageResult.Width,
                (uint)imageResult.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, textureBytes);

            gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

            gl.BindTexture(TextureTarget.Texture2D, 0);

            return texture;
        }

      

        private static unsafe ImageResult ReadTextureImage(string textureResource)
        {
            ImageResult result;
            using (Stream skyeboxStream
                = typeof(DroneTexture).Assembly.GetManifestResourceStream("Szeminarium1_24_02_17_2.Resources." + textureResource))
                result = ImageResult.FromStream(skyeboxStream, ColorComponents.RedGreenBlueAlpha);

            return result;
        }
       

    }
}
