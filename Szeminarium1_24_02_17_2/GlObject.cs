﻿using Silk.NET.OpenGL;

namespace Szeminarium1_24_02_17_2
{
    internal class GlObject
    {
        public uint Vao { get; }
        public uint Vertices { get; }
        public uint Colors { get; }
        public uint Indices { get; }
        public uint IndexArrayLength { get; }

        private GL Gl;

        public uint Texture { get; }

        public float[] CurrentColors { get; set; }  

        public GlObject(uint vao, uint vertices, uint colors, uint indeces, uint indexArrayLength, GL gl)
        {
            this.Vao = vao;
            this.Vertices = vertices;
            this.Colors = colors;
            this.Indices = indeces;
            this.IndexArrayLength = indexArrayLength;
            this.Gl = gl;
        }

        public GlObject(uint vao, uint vertices, uint colors, uint indices, uint indexArrayLength, GL gl, uint texture)
        {
            this.Vao = vao;
            this.Vertices = vertices;
            this.Colors = colors;
            this.Indices = indices;
            this.IndexArrayLength = indexArrayLength;
            this.Gl = gl;
            this.Texture = texture;
        }

        internal void ReleaseGlObject()
        {
            // always unbound the vertex buffer first, so no halfway results are displayed by accident
            Gl.DeleteBuffer(Vertices);
            Gl.DeleteBuffer(Colors);
            Gl.DeleteBuffer(Indices);
            Gl.DeleteVertexArray(Vao);
        }

        // ***************************8 texture
        public void Bind()
        {
            Gl.BindVertexArray(Vao);
            Gl.BindBuffer(GLEnum.ArrayBuffer, Vertices);
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, Indices);
        }

        // ****************************** texture finish
    }
}
