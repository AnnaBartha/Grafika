using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace Szeminarium1_24_02_17_2
{
    internal class ObjResourceReader
    {
        public static unsafe GlObject CreateTeapotWithColor(GL Gl, float[] faceColor)
        {
            uint vao = Gl.GenVertexArray();
            Gl.BindVertexArray(vao);

            /*List<float[]> objVertices;
            List<int[]> objFaces;*/

            // ****************
            List<float[]> objVertices;
            List<float[]> objTextureCoords;
            List<float[]> objNormals;
            List<int[]> objFaces;
            //*****************

            //ReadObjDataForTeapot(out objVertices, out objFaces);
            ReadObjDataForTeapot(out objVertices, out objTextureCoords, out objNormals, out objFaces);


            List<float> glVertices = new List<float>();
            List<float> glColors = new List<float>();
            List<uint> glIndices = new List<uint>();

            CreateGlArraysFromObjArrays(faceColor, objVertices, objFaces, glVertices, glColors, glIndices);

            return CreateOpenGlObject(Gl, vao, glVertices, glColors, glIndices);
        }

        //******************************* FISH *******************
        public static unsafe GlObject CreateFishWithColor(GL Gl, float[] faceColor, float scaleX, float scaleY, float scaleZ)
        {
            uint vao = Gl.GenVertexArray();
            Gl.BindVertexArray(vao);

            List<float[]> objVertices;
            List<int[]> objFaces;

            ReadObjDataForFish(out objVertices, out objFaces);

            List<float> glVertices = new List<float>();
            List<float> glColors = new List<float>();
            List<uint> glIndices = new List<uint>();

            CreateGlArraysFromObjArrays(faceColor, objVertices, objFaces, glVertices, glColors, glIndices);

            // **************************** scaling
            // Apply scaling to the glVertices
            for (int i = 0; i < glVertices.Count; i += 6) // assuming each vertex has 6 elements (3 for position, 3 for normal)
            {
                glVertices[i] *= scaleX;
                glVertices[i + 1] *= scaleY;
                glVertices[i + 2] *= scaleZ;
            }
            //*************************************

            return CreateOpenGlObject(Gl, vao, glVertices, glColors, glIndices);
        }
        // *********************************************************************************************



        private static unsafe GlObject CreateOpenGlObject(GL Gl, uint vao, List<float> glVertices, List<float> glColors, List<uint> glIndices)
        {
            uint offsetPos = 0;
            uint offsetNormal = offsetPos + (3 * sizeof(float));
            uint vertexSize = offsetNormal + (3 * sizeof(float));

            uint vertices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, vertices);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)glVertices.ToArray().AsSpan(), GLEnum.StaticDraw);
            Gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, vertexSize, (void*)offsetPos);
            Gl.EnableVertexAttribArray(0);

            Gl.EnableVertexAttribArray(2);
            Gl.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, vertexSize, (void*)offsetNormal);

            uint colors = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, colors);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)glColors.ToArray().AsSpan(), GLEnum.StaticDraw);
            Gl.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 0, null);
            Gl.EnableVertexAttribArray(1);

            uint indices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, indices);
            Gl.BufferData(GLEnum.ElementArrayBuffer, (ReadOnlySpan<uint>)glIndices.ToArray().AsSpan(), GLEnum.StaticDraw);

            // release array buffer
            Gl.BindBuffer(GLEnum.ArrayBuffer, 0);
            uint indexArrayLength = (uint)glIndices.Count;

            return new GlObject(vao, vertices, colors, indices, indexArrayLength, Gl);
        }

        private static unsafe void CreateGlArraysFromObjArrays(float[] faceColor, List<float[]> objVertices, List<int[]> objFaces, List<float> glVertices, List<float> glColors, List<uint> glIndices)
        {
            Dictionary<string, int> glVertexIndices = new Dictionary<string, int>();

            foreach (var objFace in objFaces)
            {
                var aObjVertex = objVertices[objFace[0] - 1];
                var a = new Vector3D<float>(aObjVertex[0], aObjVertex[1], aObjVertex[2]);
                var bObjVertex = objVertices[objFace[1] - 1];
                var b = new Vector3D<float>(bObjVertex[0], bObjVertex[1], bObjVertex[2]);
                var cObjVertex = objVertices[objFace[2] - 1];
                var c = new Vector3D<float>(cObjVertex[0], cObjVertex[1], cObjVertex[2]);

                var normal = Vector3D.Normalize(Vector3D.Cross(b - a, c - a));

                // process 3 vertices
                for (int i = 0; i < objFace.Length; ++i)
                {
                    var objVertex = objVertices[objFace[i] - 1];

                    // create gl description of vertex
                    List<float> glVertex = new List<float>();
                    glVertex.AddRange(objVertex);
                    glVertex.Add(normal.X);
                    glVertex.Add(normal.Y);
                    glVertex.Add(normal.Z);
                    // add textrure, color

                    // check if vertex exists
                    var glVertexStringKey = string.Join(" ", glVertex);
                    if (!glVertexIndices.ContainsKey(glVertexStringKey))
                    {
                        glVertices.AddRange(glVertex);
                        glColors.AddRange(faceColor);
                        glVertexIndices.Add(glVertexStringKey, glVertexIndices.Count);
                    }

                    // add vertex to triangle indices
                    glIndices.Add((uint)glVertexIndices[glVertexStringKey]);
                }
            }
        }

        /*private static unsafe void ReadObjDataForTeapot(out List<float[]> objVertices, out List<int[]> objFaces)
        {
            objVertices = new List<float[]>();
            objFaces = new List<int[]>();
            using (Stream objStream = typeof(ObjResourceReader).Assembly.GetManifestResourceStream("Szeminarium1_24_02_17_2.Resources.drone.obj"))
            //using (Stream objStream = typeof(ObjResourceReader).Assembly.GetManifestResourceStream("Szeminarium1_24_02_17_2.Resources.teapot.obj"))
            using (StreamReader objReader = new StreamReader(objStream))
            {
                while (!objReader.EndOfStream)
                {
                    var line = objReader.ReadLine();

                    if (String.IsNullOrEmpty(line) || line.Trim().StartsWith("#"))
                        continue;

                    var lineClassifier = line.Substring(0, line.IndexOf(' '));
                    var lineData = line.Substring(lineClassifier.Length).Trim().Split(' ');

                    switch (lineClassifier)
                    {
                        case "v":
                            float[] vertex = new float[3];
                            for (int i = 0; i < vertex.Length; ++i)
                                vertex[i] = float.Parse(lineData[i]);
                            objVertices.Add(vertex);
                            break;
                        case "f":
                            int[] face = new int[3];
                            for (int i = 0; i < face.Length; ++i)
                                face[i] = int.Parse(lineData[i]);
                            objFaces.Add(face);
                            break;
                    }
                }
            }
        }*/

        // ********************************************************************************************* READ DRONE OBJ WITH VT AND WN
        private static unsafe void ReadObjDataForTeapot(out List<float[]> objVertices, out List<float[]> objTextureCoords, out List<float[]> objNormals, out List<int[]> objFaces)
        {
            objVertices = new List<float[]>();
            objTextureCoords = new List<float[]>();
            objNormals = new List<float[]>();
            objFaces = new List<int[]>();

            using (Stream objStream = typeof(ObjResourceReader).Assembly.GetManifestResourceStream("Szeminarium1_24_02_17_2.Resources.drone.obj"))
            using (StreamReader objReader = new StreamReader(objStream))
            {
                while (!objReader.EndOfStream)
                {
                    var line = objReader.ReadLine();

                    if (String.IsNullOrEmpty(line) || line.Trim().StartsWith("#"))
                        continue;

                    var parts = line.Trim().Split(' ');
                    var lineClassifier = parts[0];
                    var lineData = parts.Skip(1).ToArray();

                    switch (lineClassifier)
                    {
                        case "v":
                            float[] vertex = new float[3];
                            for (int i = 0; i < vertex.Length; ++i)
                                vertex[i] = float.Parse(lineData[i]);
                            objVertices.Add(vertex);
                            break;
                        case "vt":
                            float[] textureCoord = new float[2];
                            for (int i = 0; i < textureCoord.Length; ++i)
                                textureCoord[i] = float.Parse(lineData[i]);
                            objTextureCoords.Add(textureCoord);
                            break;
                        case "vn":
                            float[] normal = new float[3];
                            for (int i = 0; i < normal.Length; ++i)
                                normal[i] = float.Parse(lineData[i]);
                            objNormals.Add(normal);
                            break;
                        case "f":
                            int[] face = new int[3];
                            for (int i = 0; i < face.Length; ++i)
                            {
                                var vertexData = lineData[i].Split('/');
                                face[i] = int.Parse(vertexData[0]); // Assuming only vertex indices are needed
                                                                    // Additional parsing for texture and normal indices can be added here if needed
                            }
                            objFaces.Add(face);
                            break;
                    }
                }
            }

        }
        // ********************************************************************************************* READ DRONE OBJ WITH VT AND WN FINISH



        // *************************************************************************************8 FISH 
        private static unsafe void ReadObjDataForFish(out List<float[]> objVertices, out List<int[]> objFaces)
        {
            objVertices = new List<float[]>();
            objFaces = new List<int[]>();
            using (Stream objStream = typeof(ObjResourceReader).Assembly.GetManifestResourceStream("Szeminarium1_24_02_17_2.Resources.fish.obj"))
            using (StreamReader objReader = new StreamReader(objStream))
            {
                while (!objReader.EndOfStream)
                {
                    var line = objReader.ReadLine();

                    if (String.IsNullOrEmpty(line) || line.Trim().StartsWith("#"))
                        continue;

                    var lineClassifier = line.Substring(0, line.IndexOf(' '));
                    var lineData = line.Substring(lineClassifier.Length).Trim().Split(' ');

                    switch (lineClassifier)
                    {
                        case "v":
                            float[] vertex = new float[3];
                            for (int i = 0; i < vertex.Length; ++i)
                                vertex[i] = float.Parse(lineData[i]);
                            objVertices.Add(vertex);
                            break;
                        case "f":
                            int[] face = new int[3];
                            for (int i = 0; i < face.Length; ++i)
                                face[i] = int.Parse(lineData[i]);
                            objFaces.Add(face);
                            break;
                    }
                }
            }
        }
        // *********************************************************************************************************************

        }

    }

