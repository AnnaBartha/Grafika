using ImGuiNET;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;
using System.Numerics;
using NAudio.Wave;

namespace Szeminarium1_24_02_17_2
{
    internal static class Program
    {
        private static CameraDescriptor cameraDescriptor = new();

        private static CameraDescriptorDroneView cameraDescriptorDroneView = new();

        private static CubeArrangementModel cubeArrangementModel = new();

        private static IWindow window;

        private static IInputContext inputContext;

        private static GL Gl;

        private static ImGuiController controller;

        private static uint program;

        private static GlObject teapot;


        // ******************************************** FISH
        private static GlObject fish;


        private static Random rand = new Random();

        // Store the current positions and directions of the fishes
        private static Vector3[] fishPositions;
        private static Vector3 dronePosition;
        private static Vector3[] fishDirections;
        private const float fishSpeed = 0.35f; // Adjust the speed of the fishes
        private static float[] fishRotations;

        // sounds
        private static WaveOutEvent outputDevice;
        private static AudioFileReader audioFile;
        private static bool isPlayingSound = false;

        // IN ORDER TO TRACK WHICH TYPE OF VIEW I NEED
        private static bool insiderView = false;

        //movement
        private static bool isFKeyPressed = false;
        private static bool isRKeyPressed = false;
        private static bool isLKeyPressed = false;
        private static bool isHKeyPressed = false;
        private static bool isUpKeyPressed = false;
        private static bool isDownKeyPressed = false;
        private static bool isLeftKeyPressed = false;
        private static bool isRightKeyPressed = false;
        private static bool isUKeyPressed = false;
        private static bool isDKeyPressed = false;
        private static bool elore = false;
        private static bool hatra = false;

        // smooth movemnet drone
        private static Vector3 droneMoveDirection = Vector3.Zero;
        private static float rotationAngle = 0.0f;

        // ******************************************** FISH


        private static GlObject table;

        private static GlCube glCubeRotating;

        private static GlCube skyBox;

        private static float Shininess = 50;

        private const string ModelMatrixVariableName = "uModel";
        private const string NormalMatrixVariableName = "uNormal";
        private const string ViewMatrixVariableName = "uView";
        private const string ProjectionMatrixVariableName = "uProjection";

        private const string TextureUniformVariableName = "uTexture";

        private const string LightColorVariableName = "lightColor";
        private const string LightPositionVariableName = "lightPos";
        private const string ViewPosVariableName = "viewPos";
        private const string ShininessVariableName = "shininess";

        static unsafe void Main(string[] args)
        {
            WindowOptions windowOptions = WindowOptions.Default;
            windowOptions.Title = "Anna's dream came true <3";
            windowOptions.Size = new Vector2D<int>(1000, 1000);

            // on some systems there is no depth buffer by default, so we need to make sure one is created
            windowOptions.PreferredDepthBufferBits = 24;

            window = Window.Create(windowOptions);

            window.Load += Window_Load;
            window.Update += Window_Update;
            window.Render += Window_Render;
            window.Closing += Window_Closing;

            window.Run();
        }

        private static void Window_Load()
        {
            //Console.WriteLine("Load");

            // set up input handling
            inputContext = window.CreateInput();
            foreach (var keyboard in inputContext.Keyboards)
            {
                keyboard.KeyDown += Keyboard_KeyDown;
                keyboard.KeyUp += Keyboard_KeyUp;
            }

            Gl = window.CreateOpenGL();

            controller = new ImGuiController(Gl, window, inputContext);

            // Handle resizes
            window.FramebufferResize += s =>
            {
                // Adjust the viewport to the new window size
                Gl.Viewport(s);
            };


            Gl.ClearColor(System.Drawing.Color.Black);

            SetUpObjects();

            LinkProgram();

            //Gl.Enable(EnableCap.CullFace);

            Gl.Enable(EnableCap.DepthTest);
            Gl.DepthFunc(DepthFunction.Lequal);

            //********************************************************************
            fishPositions = new Vector3[]
            {
                new Vector3(0f, -5f, 0f),   // Fish 1
                new Vector3(1f, -5f, 5f),   // Fish 2
                new Vector3(4f, -5f, 1f),   // Fish 3
                new Vector3(1f, -5f, 10f),  // Fish 4
                new Vector3(1f, -5f, 6f),   // Fish 5
                new Vector3(7f, -5f, 2f),   // Fish 6
                new Vector3(2f, -5f, 0f),   // Fish 7
                new Vector3(1f, -5f, 3f),   // Fish 8
                new Vector3(9f, -5f, 3f),   // Fish 9
                new Vector3(-2f, -5f, 8f),  // Fish 10
                new Vector3(-6f, -5f, -5f), // Fish 11
                new Vector3(-8f, -5f, 7f),  // Fish 12
                new Vector3(-4f, -5f, -10f),// Fish 13
                new Vector3(-9f, -5f, -9f), // Fish 14
                new Vector3(-3f, -5f, 3f),  // Fish 15
                new Vector3(-5f, -5f, 6f),  // Fish 16
                new Vector3(-7f, -5f, -2f),  // Fish 17
                new Vector3(-2f, -5f, -8f),  // Fish 18
                new Vector3(-6f, -5f, 5f),   // Fish 19
                new Vector3(-8f, -5f, -7f),  // Fish 20
                new Vector3(-4f, -5f, 10f),  // Fish 21
                new Vector3(-9f, -5f, 9f),   // Fish 22
                new Vector3(-3f, -5f, -3f),  // Fish 23
                new Vector3(-5f, -5f, -6f),  // Fish 24
                new Vector3(-7f, -5f, 2f),    // Fish 25
            };
            fishRotations = new float[fishPositions.Length];

            dronePosition = new Vector3(0f, 0f, 0f);


            fishDirections = new Vector3[fishPositions.Length];
            for (int i = 0; i < fishDirections.Length; i++)
            {
                fishDirections[i] = GetRandomDirection();
            }
            // **********************************************************************
        }

        private static void LinkProgram()
        {
            uint vshader = Gl.CreateShader(ShaderType.VertexShader);
            uint fshader = Gl.CreateShader(ShaderType.FragmentShader);

            Gl.ShaderSource(vshader, ReadShader("VertexShader.vert"));
            Gl.CompileShader(vshader);
            Gl.GetShader(vshader, ShaderParameterName.CompileStatus, out int vStatus);
            if (vStatus != (int)GLEnum.True)
                throw new Exception("Vertex shader failed to compile: " + Gl.GetShaderInfoLog(vshader));

            Gl.ShaderSource(fshader, ReadShader("FragmentShader.frag"));
            Gl.CompileShader(fshader);

            program = Gl.CreateProgram();
            Gl.AttachShader(program, vshader);
            Gl.AttachShader(program, fshader);
            Gl.LinkProgram(program);
            Gl.GetProgram(program, GLEnum.LinkStatus, out var status);
            if (status == 0)
            {
                Console.WriteLine($"Error linking shader {Gl.GetProgramInfoLog(program)}");
            }
            Gl.DetachShader(program, vshader);
            Gl.DetachShader(program, fshader);
            Gl.DeleteShader(vshader);
            Gl.DeleteShader(fshader);
        }

        private static string ReadShader(string shaderFileName)
        {
            using (Stream shaderStream = typeof(Program).Assembly.GetManifestResourceStream("Szeminarium1_24_02_17_2.Shaders." + shaderFileName))
            using (StreamReader shaderReader = new StreamReader(shaderStream))
                return shaderReader.ReadToEnd();
        }


        // ************************************** NAVBAR FOR CAMERA TYPE
        private static void RenderNavBar()
        {
            ImGui.Begin("Camera Navigation");

            // Button to toggle between camera views
            if (ImGui.Button("Change View"))
            {
                insiderView = !insiderView; // Toggle the view mode
            }

            ImGui.End();
        }
        // ****************************************************************************


        // inside view
        private static void Keyboard_KeyDown(IKeyboard keyboard, Key key, int arg3)
        {
            switch (key)
            {

                case Key.Left:
                    if (!insiderView)
                    {
                        isLeftKeyPressed = true;
                    }
                    break;
                    ;
                case Key.Right:
                    if (!insiderView)
                    {
                        isRightKeyPressed = true;
                    }
                    break;
                case Key.Down:
                    if (!insiderView)
                    {
                        isDownKeyPressed = true;
                    }
                    break;
                case Key.Up:
                    if (!insiderView)
                    {
                        isUpKeyPressed = true;
                    }
                    break;
                case Key.U:
                    if (!insiderView)
                    {
                        isUKeyPressed = true;
                    }
                    break;
                case Key.D:
                    if (!insiderView)
                    {
                        isDKeyPressed = true;
                    }
                    break;
                case Key.L:
                    droneMoveDirection = new Vector3(1,0,0);
                    isLKeyPressed = true;
                    break;
                case Key.R:
                    droneMoveDirection = new Vector3(-1, 0, 0);
                    isRKeyPressed = true;
                    break;
                case Key.F:
                    //droneMoveDirection = new Vector3(0, -1, 0);
                    isFKeyPressed = true;
                    break;
                case Key.H:
                    //droneMoveDirection = new Vector3(0, 1, 0);
                    isHKeyPressed = true;
                    break;
                case Key.E:
                    elore = true;
                    droneMoveDirection = new Vector3(0, 0, 1);
                    break;
                case Key.B:
                    hatra = true;
                    droneMoveDirection = new Vector3(0, 0, -1);
                    break;
            }
        }

        private static void Keyboard_KeyUp(IKeyboard keyboard, Key key, int arg3)
        {
            switch (key)
            {

                case Key.Left:
                    if (!insiderView)
                    {
                        isLeftKeyPressed = false;
                    }
                    break;
                    ;
                case Key.Right:
                    if (!insiderView)
                    {
                        isRightKeyPressed = false;
                    }
                    break;
                case Key.Down:
                    if (!insiderView)
                    {
                        isDownKeyPressed = false;
                    }
                    break;
                case Key.Up:
                    if (!insiderView)
                    {
                        isUpKeyPressed = false;
                    }
                    break;
                case Key.U:
                    if (!insiderView)
                    {
                        isUKeyPressed = false;
                    }
                    break;
                case Key.D:
                    if (!insiderView)
                    {
                        isDKeyPressed = false;
                    }
                    break;
                case Key.L:
                    isLKeyPressed = false;
                    break;
                case Key.R:
                    isRKeyPressed = false;
                    break;
                case Key.F:
                    isFKeyPressed = false;
                    break;
                case Key.H:
                    isHKeyPressed = false;
                    break;
                case Key.E:
                    elore = false;
                    break;
                case Key.B:
                    hatra = false;  
                    break;
            }
        }

        private static void Window_Update(double deltaTime)
        {
            //Console.WriteLine($"Update after {deltaTime} [s].");
            // multithreaded
            // make sure it is threadsafe
            // NO GL calls
            cubeArrangementModel.AdvanceTime(deltaTime);

            UpdateFishPositions((float)deltaTime);

            controller.Update((float)deltaTime);

            // ************************************************************************************* sound
            if (isPlayingSound && outputDevice.PlaybackState != PlaybackState.Playing)
            {
                outputDevice.Dispose();
                audioFile.Dispose();
                isPlayingSound = false;
            }
            // ********************************************************************************************
           
            
            if(isLeftKeyPressed) { cameraDescriptor.DecreaseZYAngle(); }
            if(isRightKeyPressed) { cameraDescriptor.IncreaseZYAngle(); }
            if(isDownKeyPressed) { cameraDescriptor.IncreaseDistance(); }
            if(isUpKeyPressed) { cameraDescriptor.DecreaseDistance(); }
            if(isRKeyPressed) { updateDronePositionRight(); }
            if(isFKeyPressed) { droneStartedLanding(); }
            if (isLKeyPressed) { updateDronePositionLeft(); }
            if(isHKeyPressed) { droneGoingHigher(); }
            if(isUKeyPressed) { cameraDescriptor.IncreaseZXAngle();  }
            if(isDKeyPressed) { cameraDescriptor.DecreaseZXAngle();  }
            if( elore ) { updateDronePositionForward(); }
            if (hatra) { updateDronePositionBackward(); }

        }
        //*************************************************************************************************
        private static void updateDronePositionLeft()
        {
            dronePosition = new Vector3(dronePosition.X + 0.5f, dronePosition.Y, dronePosition.Z);

            // ********************************************* update also the camera positin
            Vector3D<float> convertedPosition = new Vector3D<float>(dronePosition.X, dronePosition.Y, dronePosition.Z);
            cameraDescriptorDroneView.SetDronePosition(convertedPosition);
        }

        private static void updateDronePositionRight()
        {
            dronePosition = new Vector3(dronePosition.X - 0.5f, dronePosition.Y, dronePosition.Z);

            // ********************************************* update also the camera positin
            Vector3D<float> convertedPosition = new Vector3D<float>(dronePosition.X, dronePosition.Y, dronePosition.Z);
            cameraDescriptorDroneView.SetDronePosition(convertedPosition);
        }

        private static void droneStartedLanding()
        {
            dronePosition = new Vector3(dronePosition.X, dronePosition.Y - 0.5f, dronePosition.Z);

            // ********************************************* update also the camera positin
            Vector3D<float> convertedPosition = new Vector3D<float>(dronePosition.X, dronePosition.Y, dronePosition.Z);
            cameraDescriptorDroneView.SetDronePosition(convertedPosition);
        }

        private static void droneGoingHigher()
        {
            dronePosition = new Vector3(dronePosition.X, dronePosition.Y + 0.5f, dronePosition.Z);

            // ********************************************* update also the camera positin
            Vector3D<float> convertedPosition = new Vector3D<float>(dronePosition.X, dronePosition.Y, dronePosition.Z);
            cameraDescriptorDroneView.SetDronePosition(convertedPosition);
        }

        private static void updateDronePositionForward()
        {
            dronePosition = new Vector3(dronePosition.X, dronePosition.Y, dronePosition.Z - 0.5f);

            // ********************************************* update also the camera positin
            Vector3D<float> convertedPosition = new Vector3D<float>(dronePosition.X, dronePosition.Y, dronePosition.Z);
            cameraDescriptorDroneView.SetDronePosition(convertedPosition);
        }

        private static void updateDronePositionBackward()
        {
            dronePosition = new Vector3(dronePosition.X, dronePosition.Y, dronePosition.Z + 0.5f);

            // ********************************************* update also the camera positin
            Vector3D<float> convertedPosition = new Vector3D<float>(dronePosition.X, dronePosition.Y, dronePosition.Z);
            cameraDescriptorDroneView.SetDronePosition(convertedPosition);
        }

        //************************************************SOUND EFFECT
        private static void PlaySound(string filePath)
        {
            if (outputDevice != null)
            {
                outputDevice.Dispose();
            }
            if (audioFile != null)
            {
                audioFile.Dispose();
            }

            audioFile = new AudioFileReader(filePath);
            outputDevice = new WaveOutEvent();
            outputDevice.Init(audioFile);
            outputDevice.Play();
            isPlayingSound = true;
        }

        //*************************************************

        private static void UpdateFishPositions(float deltaTime)
        {
            // two lists for new fish directions and updated positions
            List<Vector3> newFishPositions = new List<Vector3>();
            List<Vector3> newFishDirections = new List<Vector3>();

            // rotation
            List<float> newFishRotations = new List<float>();

            // I iterate through all my fishes that are not fished yet
            for (int i = 0; i < fishPositions.Length; i++)
            {
                // I calculate the distance between the drone and my fish
                float distanceToDrone = Vector3.Distance(fishPositions[i], dronePosition);

                // Check if the fish is further than 5 units from the drone
                if (distanceToDrone >= 5f)
                {
                    // THE FISH IS STILL ALIVE
                    // I update the position of the fish
                    fishPositions[i] += fishDirections[i] * fishSpeed * deltaTime;

                    // If the height of a fish is greater than -2 = Y > -2  
                    // Change the direction and resize the Y coordinate 
                    // In order not to overflow (out of the sea's height)
                    if (fishPositions[i].Y > -2)
                    {
                        fishPositions[i].Y = -2;
                        fishDirections[i] *= -1;
                    }

                    // In order not to swim out of the sea (by x coord)
                    /*if (fishPositions[i].X > 50)
                    {
                        fishPositions[i].X = 50;
                        fishDirections[i] *= -1;
                    }

                    if (fishPositions[i].X < -50)
                    {
                        fishPositions[i].X = -50;
                        fishDirections[i] *= -1;
                    }*/

                    // random direction change
                    if (fishPositions[i].X > 10 || fishPositions[i].X < -10 ||
                        fishPositions[i].Y > 10 || fishPositions[i].Y < -10 ||
                        fishPositions[i].Z > 10 || fishPositions[i].Z < -10)
                    {
                        fishDirections[i] = GetRandomDirection();
                    }

                    // rotation
                    float rotationAngle = MathF.Atan2(-fishDirections[i].X, fishDirections[i].Z);
                    fishRotations[i] = rotationAngle;

                    // Add the updated fish position and direction to the new updated lists
                    newFishPositions.Add(fishPositions[i]);
                    newFishDirections.Add(fishDirections[i]);
                    //rotation
                    newFishRotations.Add(rotationAngle);
                }
                else if (!isPlayingSound)  // Only play sound if not already playing
                {
                    PlaySound("dissapear.wav");

                    Console.WriteLine($"Fish at index {i} removed, Distance to drone: {distanceToDrone}");
                }
            }

            // Update the fish positions array with the fishes that has not been fished yet 
            fishPositions = newFishPositions.ToArray();
            fishDirections = newFishDirections.ToArray();
            fishRotations = newFishRotations.ToArray();

            Console.WriteLine($"Remaining fish count: {fishPositions.Length}");
        }


        private static Vector3 GetRandomDirection()
        {
            float randomX = (float)(rand.NextDouble() * 2 - 1);
            float randomY = (float)(rand.NextDouble() * 2 - 1);
            float randomZ = (float)(rand.NextDouble() * 2 - 1);

            Vector3 direction = new Vector3(randomX, randomY, randomZ);
            return Vector3.Normalize(direction);
        }
        //********************************************************************************************

        private static unsafe void Window_Render(double deltaTime)
        {
            Gl.Clear(ClearBufferMask.ColorBufferBit);
            Gl.Clear(ClearBufferMask.DepthBufferBit);


            Gl.UseProgram(program);

            RenderNavBar();

            SetViewMatrix();
            SetProjectionMatrix();

            SetLightColor();
            SetLightPosition();
            SetViewerPosition();
            SetShininess();

            DrawPulsingTeapot(dronePosition);

            // Render multiple fish at different positions
            for (int i = 0; i < fishPositions.Length; i++)
            {

                DrawFish(fishPositions[i], fishRotations[i]); // Pass the position of each fish to the DrawFish method
            }

            DrawSkyBox();

            //ImGuiNET.ImGui.ShowDemoWindow();
            ImGuiNET.ImGui.Begin("Lighting properties",
                ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoTitleBar);
            ImGuiNET.ImGui.SliderFloat("Shininess", ref Shininess, 1, 200);
            ImGuiNET.ImGui.End();


            controller.Render();
        }

        private static unsafe void DrawSkyBox()
        {
            Matrix4X4<float> modelMatrix = Matrix4X4.CreateScale(400f);
            SetModelMatrix(modelMatrix);
            Gl.BindVertexArray(skyBox.Vao);

            int textureLocation = Gl.GetUniformLocation(program, TextureUniformVariableName);
            if (textureLocation == -1)
            {
                throw new Exception($"{TextureUniformVariableName} uniform not found on shader.");
            }
            // set texture 0
            Gl.Uniform1(textureLocation, 0);

            Gl.ActiveTexture(TextureUnit.Texture0);
            Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)GLEnum.Linear);
            Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)GLEnum.Linear);
            Gl.BindTexture(TextureTarget.Texture2D, skyBox.Texture.Value);

            Gl.DrawElements(GLEnum.Triangles, skyBox.IndexArrayLength, GLEnum.UnsignedInt, null);
            Gl.BindVertexArray(0);

            CheckError();
            Gl.BindTexture(TextureTarget.Texture2D, 0);
            CheckError();
        }

        private static unsafe void SetLightColor()
        {
            int location = Gl.GetUniformLocation(program, LightColorVariableName);

            if (location == -1)
            {
                throw new Exception($"{LightColorVariableName} uniform not found on shader.");
            }

            Gl.Uniform3(location, 1f, 1f, 1f);
            CheckError();
        }

        private static unsafe void SetLightPosition()
        {
            int location = Gl.GetUniformLocation(program, LightPositionVariableName);

            if (location == -1)
            {
                throw new Exception($"{LightPositionVariableName} uniform not found on shader.");
            }

            Gl.Uniform3(location, 0f, 10f, 0f);
            CheckError();
        }

        private static unsafe void SetViewerPosition()
        {
            int location = Gl.GetUniformLocation(program, ViewPosVariableName);

            if (location == -1)
            {
                throw new Exception($"{ViewPosVariableName} uniform not found on shader.");
            }

            if (insiderView)
            {
                Gl.Uniform3(location, cameraDescriptorDroneView.Position.X, cameraDescriptorDroneView.Position.Y, cameraDescriptorDroneView.Position.Z);

            }
            else
            {
                Gl.Uniform3(location, cameraDescriptor.Position.X, cameraDescriptor.Position.Y, cameraDescriptor.Position.Z);
            }
            
            CheckError();
        }


        private static unsafe void SetShininess()
        {
            int location = Gl.GetUniformLocation(program, ShininessVariableName);

            if (location == -1)
            {
                throw new Exception($"{ShininessVariableName} uniform not found on shader.");
            }

            Gl.Uniform1(location, Shininess);
            CheckError();
        }


        private static unsafe void DrawPulsingTeapot(Vector3 position)
        {
            // calculate the target rotation angle based on the movement direction
            float targetRotationAngle = MathF.Atan2(-droneMoveDirection.X, droneMoveDirection.Z);
            if (droneMoveDirection == Vector3.Zero)
            {
                targetRotationAngle = rotationAngle;
            }
            else
            {
                rotationAngle = targetRotationAngle;
            }

            // model matrix for the dragon with translation and rotation
            Matrix4X4<float> dragonModelMatrix = Matrix4X4.CreateScale(0.5f) *
                                                 Matrix4X4.CreateRotationY(targetRotationAngle) * // Rotate around Y-axis
                                                 Matrix4X4.CreateTranslation(new Vector3D<float>(position.X, position.Y, position.Z));

            SetModelMatrix(dragonModelMatrix);
            Gl.BindVertexArray(teapot.Vao);
            Gl.DrawElements(GLEnum.Triangles, teapot.IndexArrayLength, GLEnum.UnsignedInt, null);
            Gl.BindVertexArray(0);

        }

        // ****************************************** FISH
        private static unsafe void DrawFish(Vector3 position, float rotationAngle)
        {
            // Create the model matrix with the rotation and translation
            Matrix4X4<float> modelMatrix = Matrix4X4.CreateScale(0.5f) *
                                           Matrix4X4.CreateRotationY(rotationAngle) *
                                           Matrix4X4.CreateTranslation(position.X, position.Y, position.Z);

            SetModelMatrix(modelMatrix);
            Gl.BindVertexArray(fish.Vao);
            Gl.DrawElements(GLEnum.Triangles, fish.IndexArrayLength, GLEnum.UnsignedInt, null);
            Gl.BindVertexArray(0);
        }
        // *****************************************************************************************************************

        private static unsafe void SetModelMatrix(Matrix4X4<float> modelMatrix)
        {
            int location = Gl.GetUniformLocation(program, ModelMatrixVariableName);
            if (location == -1)
            {
                throw new Exception($"{ModelMatrixVariableName} uniform not found on shader.");
            }

            Gl.UniformMatrix4(location, 1, false, (float*)&modelMatrix);
            CheckError();

            var modelMatrixWithoutTranslation = new Matrix4X4<float>(modelMatrix.Row1, modelMatrix.Row2, modelMatrix.Row3, modelMatrix.Row4);
            modelMatrixWithoutTranslation.M41 = 0;
            modelMatrixWithoutTranslation.M42 = 0;
            modelMatrixWithoutTranslation.M43 = 0;
            modelMatrixWithoutTranslation.M44 = 1;

            Matrix4X4<float> modelInvers;
            Matrix4X4.Invert<float>(modelMatrixWithoutTranslation, out modelInvers);
            Matrix3X3<float> normalMatrix = new Matrix3X3<float>(Matrix4X4.Transpose(modelInvers));
            location = Gl.GetUniformLocation(program, NormalMatrixVariableName);
            if (location == -1)
            {
                throw new Exception($"{NormalMatrixVariableName} uniform not found on shader.");
            }
            Gl.UniformMatrix3(location, 1, false, (float*)&normalMatrix);
            CheckError();
        }

        private static unsafe void SetUpObjects()
        {
            // *********************************************************************************8 FISH

            //float[] fishColor = { 1.0f, 0.5f, 0.0f, 1.0f }; // Orange color for the fish
            float[] fishColor = { 0.7f, 0.3f, 0.7f, 1.0f };
            //fish = ObjResourceReader.CreateFishWithColor(Gl, fishColor);

            // in order to make the fish look smaller than theh drone
            float scaleX = 0.03f;
            float scaleY = 0.03f;
            float scaleZ = 0.03f;

            fish = ObjResourceReader.CreateFishWithColor(Gl, fishColor, scaleX, scaleY, scaleZ);


            //************************************************************************************

            float[] face1Color = [1f, 0f, 0f, 1.0f];
            float[] face2Color = [0.0f, 1.0f, 0.0f, 1.0f];
            float[] face3Color = [0.0f, 0.0f, 1.0f, 1.0f];
            float[] face4Color = [1.0f, 0.0f, 1.0f, 1.0f];
            float[] face5Color = [0.0f, 1.0f, 1.0f, 1.0f];
            float[] face6Color = [1.0f, 1.0f, 0.0f, 1.0f];

            float[] tableColor = { 0.5f, 0.5f, 0.5f, 1f };
            //teapot = ObjResourceReader.CreateTeapotWithColor(Gl, face1Color);
            teapot = ObjResourceReader.CreateTeapotWithColor(Gl, tableColor);

            table = GlCube.CreateSquare(Gl, tableColor);

            glCubeRotating = GlCube.CreateCubeWithFaceColors(Gl, face1Color, face2Color, face3Color, face4Color, face5Color, face6Color);

            skyBox = GlCube.CreateInteriorCube(Gl, "");
        }

        private static void Window_Closing()
        {
            teapot.ReleaseGlObject();
            glCubeRotating.ReleaseGlObject();
            fish.ReleaseGlObject();

        }

        private static unsafe void SetProjectionMatrix()
        {
            var projectionMatrix = Matrix4X4.CreatePerspectiveFieldOfView<float>((float)Math.PI / 4f, 1024f / 768f, 0.1f, 1000);
            int location = Gl.GetUniformLocation(program, ProjectionMatrixVariableName);

            if (location == -1)
            {
                throw new Exception($"{ViewMatrixVariableName} uniform not found on shader.");
            }

            Gl.UniformMatrix4(location, 1, false, (float*)&projectionMatrix);
            CheckError();
        }

        private static unsafe void SetViewMatrix()
        {
            var viewMatrix = insiderView
                ? Matrix4X4.CreateLookAt(cameraDescriptorDroneView.Position, cameraDescriptorDroneView.Target, cameraDescriptorDroneView.UpVector)
                : Matrix4X4.CreateLookAt(cameraDescriptor.Position, cameraDescriptor.Target, cameraDescriptor.UpVector);

            int location = Gl.GetUniformLocation(program, ViewMatrixVariableName);

            if (location == -1)
            {
                throw new Exception($"{ViewMatrixVariableName} uniform not found on shader.");
            }

            Gl.UniformMatrix4(location, 1, false, (float*)&viewMatrix);
            CheckError();
        }

        public static void CheckError()
        {
            var error = (ErrorCode)Gl.GetError();
            if (error != ErrorCode.NoError)
                throw new Exception("GL.GetError() returned " + error.ToString());
        }

    }
}