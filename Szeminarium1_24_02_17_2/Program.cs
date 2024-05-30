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
        private static List<GlObject> whale = new();


        // ******************************************** FISH
        private static List<GlObject> fish = new();

        

        private static Random rand = new Random();
        //private static Random rand = new Random();

        // Store the current positions and directions of the fishes
        private static Vector3[] fishPositions;
        private static Vector3 dronePosition;
        private static Vector3[] whalePosition;
        private static Vector3[] fishDirections;
        private static Vector3[] whaleDirections;
        private const float fishSpeed = 0.35f; // Adjust the speed of the fishes
        private static float[] fishRotations;
        private static float[] whaleRotations;

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

        //light parameters
       // private static Vector3 BackgroundLightColor = new Vector3(1.0f, 1.0f, 1.0f);
        private static Vector3 AmbientStrength = new Vector3(0.2f);
        private static Vector3 DiffuseStrength = new Vector3(0.3f);
        private static Vector3 SpecularStrength = new Vector3(0.5f);

        private const string AmbientStrengthVariableName = "ambientStrength";
        private const string DiffuseStrengthVariableName = "diffuseStrength";
        private const string SpecularStrengthVariableName = "specularStrength";


        // fish colors
        private static List<float[]> fishColors = new List<float[]>();
        // ******************************************** FISH


        //private static GlObject table;

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

        private const string ObjectColorVariableName = "objectColor";

        private static int playerScore = 0;
        
        static unsafe void Main(string[] args)
        {
            DroneSound.Initialize();

            WindowOptions windowOptions = WindowOptions.Default;
            windowOptions.Title = "Anna's dream came true <3";
            windowOptions.Size = new Vector2D<int>(1000, 1000);

            // on some systems there is no depth buffer by default, so we need to make sure one is created
            windowOptions.PreferredDepthBufferBits = 24;

            window = Window.Create(windowOptions);

            for (int i = 0; i < 25; i++)
            {

                float[] fishColor = { 1.0f, 0.5f, 0.0f, 1.0f };
                fishColors.Add(fishColor);
            }

            for (int i = 0; i < 25; i++)
            {
                Console.WriteLine($"Fish at index {i} has a color of {fishColors[i][0]} {fishColors[i][1]} {fishColors[i][0]} {fishColors[i][0]}");
            }

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

            whalePosition = new Vector3[]
            {
                new Vector3(3f, -5f, -2f),   // Whate 1
                new Vector3(-1f, -5f, 4f),   // Whate 2
                new Vector3(-5f, -5f, 9f),   // Whate 3
            };

            whaleRotations = new float[whalePosition.Length];
            fishDirections = new Vector3[fishPositions.Length];
            for (int i = 0; i < fishDirections.Length; i++)
            {
                fishDirections[i] = GetRandomDirection();
            }

            whaleDirections = new Vector3[whalePosition.Length];
            for(int i= 0; i< whaleDirections.Length; i++)
            {
                whaleDirections[i] = GetRandomDirection();
            }
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
                    isFKeyPressed = true;
                    break;
                case Key.H:
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
            UpdateWhalePositions((float)deltaTime);
           // UpdateFishColor();
            

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
        private static void updateDronePositionLeft()
        {
            dronePosition = new Vector3(dronePosition.X + 0.5f, dronePosition.Y, dronePosition.Z);
            Vector3D<float> convertedPosition = new Vector3D<float>(dronePosition.X, dronePosition.Y, dronePosition.Z);
            cameraDescriptorDroneView.SetDronePosition(convertedPosition);
        }

        private static void updateDronePositionRight()
        {
            dronePosition = new Vector3(dronePosition.X - 0.5f, dronePosition.Y, dronePosition.Z);
            Vector3D<float> convertedPosition = new Vector3D<float>(dronePosition.X, dronePosition.Y, dronePosition.Z);
            cameraDescriptorDroneView.SetDronePosition(convertedPosition);
        }

        private static void droneStartedLanding()
        {
            dronePosition = new Vector3(dronePosition.X, dronePosition.Y - 0.5f, dronePosition.Z);
            Vector3D<float> convertedPosition = new Vector3D<float>(dronePosition.X, dronePosition.Y, dronePosition.Z);
            cameraDescriptorDroneView.SetDronePosition(convertedPosition);
        }

        private static void droneGoingHigher()
        {
            dronePosition = new Vector3(dronePosition.X, dronePosition.Y + 0.5f, dronePosition.Z);
            Vector3D<float> convertedPosition = new Vector3D<float>(dronePosition.X, dronePosition.Y, dronePosition.Z);
            cameraDescriptorDroneView.SetDronePosition(convertedPosition);
        }

        private static void updateDronePositionForward()
        {
            dronePosition = new Vector3(dronePosition.X, dronePosition.Y, dronePosition.Z - 0.5f);
            Vector3D<float> convertedPosition = new Vector3D<float>(dronePosition.X, dronePosition.Y, dronePosition.Z);
            cameraDescriptorDroneView.SetDronePosition(convertedPosition);
        }

        private static void updateDronePositionBackward()
        {
            dronePosition = new Vector3(dronePosition.X, dronePosition.Y, dronePosition.Z + 0.5f);
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

        private static void UpdateWhalePositions(float deltaTime)
        {
            List<Vector3> newWhalePositions = new List<Vector3>();
            List<Vector3> newWhaleDirections = new List<Vector3>();
            List<float> newWhaleRotations = new List<float>();
            for (int i = 0; i < whalePosition.Length; i++)
            {
                float distanceToDrone = Vector3.Distance(whalePosition[i], dronePosition);

                if (distanceToDrone >= 3f)
                {
                    whalePosition[i] += whaleDirections[i] * 0.35f * deltaTime;

                    if (whalePosition[i].Y > -2)
                    {
                        whalePosition[i].Y = -2;
                        whaleDirections[i] *= -1;
                    }

                    // random direction change
                    if (whalePosition[i].X > 10 || whalePosition[i].X < -10 ||
                        whalePosition[i].Y > 10 || whalePosition[i].Y < -10 ||
                        whalePosition[i].Z > 10 || whalePosition[i].Z < -10)
                    {
                        whaleDirections[i] = GetRandomDirection();
                    }

                    // rotation
                    float rotationAngle = MathF.Atan2(-whaleDirections[i].X, whaleDirections[i].Z);
                    whaleRotations[i] = rotationAngle;
                    newWhalePositions.Add(whalePosition[i]);
                    newWhaleDirections.Add(whaleDirections[i]);
                    //rotation
                    newWhaleRotations.Add(rotationAngle);

                }
                else if (!isPlayingSound)  // Only play sound if not already playing
                {
                    PlaySound("dissapear.wav");
                    Console.WriteLine($"whale at index {i} removed, Distance to drone: {distanceToDrone}");
                    playerScore = playerScore + 10;
                }

            }
            whalePosition = newWhalePositions.ToArray();
            whaleDirections = newWhaleDirections.ToArray();
            whaleRotations = newWhaleRotations.ToArray();
           


            Console.WriteLine($"Remaining whale count: {whalePosition.Length}");

        }

        private static void UpdateFishPositions(float deltaTime)
        {
            // two lists for new fish directions and updated positions
            List<Vector3> newFishPositions = new List<Vector3>();
            List<Vector3> newFishDirections = new List<Vector3>();

            // rotation
            List<float> newFishRotations = new List<float>();

            //colors
            List<float[]> newFishColors = new List<float[]>();

            // I iterate through all my fishes that are not fished yet
            for (int i = 0; i < fishPositions.Length; i++)
            {
                // I calculate the distance between the drone and my fish
                float distanceToDrone = Vector3.Distance(fishPositions[i], dronePosition);

                float distanceToOneWhale = 100f;
                for(int j=0; j<whalePosition.Length; j++)
                {
                    float actualDistance = Vector3.Distance(fishPositions[i], whalePosition[j]);
                    if(actualDistance < distanceToOneWhale)
                    {
                        // looking for the closest whale
                        distanceToOneWhale = actualDistance;
                    }
                }


                // Check if the fish is further than 5 units from the drone
                if (distanceToDrone >= 3f && distanceToOneWhale >= 3f)
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


                    // if the fish is still alive i update it's position
                    float[] fishColor;

                    // the fish is close to the drone
                    if (distanceToDrone <= 10f)
                    {
                        // Green color if the fish is within 10 units from the drone
                        fishColor = new float[] { 0f, 1f, 0f, 1f }; // Green color
                        //Console.WriteLine($"Fish at index {i} close -> green");
                    }
                    else if (distanceToDrone > 10 && distanceToDrone <= 12)
                    {
                        //the fish is not that close to drone
                        fishColor = new float[] { 0.2f, 0.8f, 0f, 1f }; // intermediate #1
                       // Console.WriteLine($"Fish at index {i} far away -> red");
                    }
                    else if(distanceToDrone > 12 && distanceToDrone <= 14)
                    {
                        //the fish is not that close to drone
                        fishColor = new float[] { 0.4f, 0.6f, 0f, 1f }; // intermediate #1
                    }
                    else if(distanceToDrone > 14 && distanceToDrone <= 16)
                    {
                        fishColor = new float[] { 0.6f, 0.4f, 0f, 1f };
                    }
                    else if (distanceToDrone > 16 && distanceToDrone <= 18)
                    {
                        fishColor = new float[] { 0.8f, 0.2f, 0f, 1f };
                    }
                    else 
                    {
                        // the fish is not close to my drone
                        fishColor = new float[] { 1f, 0f, 0f, 1f };
                    }

                    newFishColors.Add(fishColor);

                }
                else if (!isPlayingSound)  // Only play sound if not already playing
                {
                    // the fish was fished by drone
                    if(distanceToOneWhale > 3f && distanceToDrone < 3f)
                    {
                        PlaySound("dissapear.wav");
                        Console.WriteLine($"Fish at index {i} removed, Distance to drone: {distanceToDrone}");
                        playerScore = playerScore + 2;
                    } else
                    {
                        //playerScore = playerScore - 1;
                    }
                   
                }
                
            }

            // Update the fish positions array with the fishes that has not been fished yet 
            fishPositions = newFishPositions.ToArray();
            fishDirections = newFishDirections.ToArray();
            fishRotations = newFishRotations.ToArray();
            fishColors = newFishColors;


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
            SetLigthingParams();

            SetObjectColor(new Vector4(0.5f, 0.5f, 0.5f, 1.0f));
            DrawPulsingTeapot(dronePosition);

            // whale
            // render multiple whale objects
            for(int i=0; i< whalePosition.Length; i++)
            {
                SetObjectColor(new Vector4(1.0f, 0.8431f, 0.0f, 1.0f));
                DrawWhale(whalePosition[i], whaleRotations[0], i);
            }

            // Render multiple fish at different positions
            for (int i = 0; i < fishPositions.Length; i++)
            {
                SetObjectColor(new Vector4(fishColors[i][0], fishColors[i][1], fishColors[i][2], fishColors[i][3]));
                DrawFish(fishPositions[i], fishRotations[i], i); // Pass the position of each fish to the DrawFish method
            }

            SetObjectColor(new Vector4(0.0f, 0.0f, 0.0f, 1.0f));
            DrawSkyBox();

            //ImGuiNET.ImGui.ShowDemoWindow();
            ImGuiNET.ImGui.Begin("Lighting properties",
                ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoTitleBar);
            ImGuiNET.ImGui.SliderFloat("Shininess", ref Shininess, 1, 200);
            ImGuiNET.ImGui.End();

            // Fish count panel
            ImGuiNET.ImGui.Begin("Fish Counter", ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoTitleBar);
            ImGuiNET.ImGui.Text($"Fish Count: {fishPositions.Length}");
            ImGuiNET.ImGui.End();

            // Whale count panel
            ImGuiNET.ImGui.Begin("Whale Counter", ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoTitleBar);
            ImGuiNET.ImGui.Text($"Whale Count: {whalePosition.Length}");
            ImGuiNET.ImGui.End();

            // Player Score
            ImGuiNET.ImGui.Begin("Player Score", ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoTitleBar);
            //ImGuiNET.ImGui.Text($"Player Score: {((25 - fishPositions.Length) * 2) + ((5 - whalePosition.Length) * 10)}");
            ImGuiNET.ImGui.Text($"Player Score: {playerScore}");
            ImGuiNET.ImGui.End();

            // lights
            ImGui.Begin("Settings", ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoTitleBar);
            ImGui.SliderFloat("Shininess", ref Shininess, 1, 200);
            ImGui.ColorEdit3("Ambient", ref AmbientStrength);
            ImGui.ColorEdit3("Diffuse", ref DiffuseStrength);
            ImGui.ColorEdit3("Specular", ref SpecularStrength);
            ImGui.End();

            controller.Render();
        }
        
        private static unsafe void SetLigthingParams()
        {
            int ambientLoc = Gl.GetUniformLocation(program, AmbientStrengthVariableName);
            int diffuseLoc = Gl.GetUniformLocation(program, DiffuseStrengthVariableName);
            int specularLoc = Gl.GetUniformLocation(program, SpecularStrengthVariableName);

            if (ambientLoc == -1 || diffuseLoc == -1 || specularLoc == -1)
            {
                throw new Exception($" uniform not found on shader.");
            }

            Gl.Uniform3(ambientLoc, AmbientStrength);
            Gl.Uniform3(diffuseLoc, DiffuseStrength);
            Gl.Uniform3(specularLoc, SpecularStrength);
            CheckError();
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

        private static unsafe void DrawWhale(Vector3 position, float rotationAngle, int i)
        {
            // model matrix for the dragon with translation and rotation
            // Create the model matrix with the rotation and translation
            Matrix4X4<float> modelMatrix = Matrix4X4.CreateScale(0.5f) *
                                           Matrix4X4.CreateRotationY(rotationAngle) *
                                           Matrix4X4.CreateTranslation(position.X, position.Y, position.Z);

            SetModelMatrix(modelMatrix);
            Gl.BindVertexArray(whale[i].Vao);
            Gl.DrawElements(GLEnum.Triangles, whale[i].IndexArrayLength, GLEnum.UnsignedInt, null);
            Gl.BindVertexArray(0);

        }

        // ****************************************** FISH
        private static unsafe void DrawFish(Vector3 position, float rotationAngle, int i)
        {
            // Create the model matrix with the rotation and translation
            Matrix4X4<float> modelMatrix = Matrix4X4.CreateScale(0.5f) *
                                           Matrix4X4.CreateRotationY(rotationAngle) *
                                           Matrix4X4.CreateTranslation(position.X, position.Y, position.Z);

            SetModelMatrix(modelMatrix);
            Gl.BindVertexArray(fish[i].Vao);
            Gl.DrawElements(GLEnum.Triangles, fish[i].IndexArrayLength, GLEnum.UnsignedInt, null);
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

        // Setting every object + skybox color based on their rgba colors
        private static unsafe void SetObjectColor(Vector4 color)
        {
            int location = Gl.GetUniformLocation(program, ObjectColorVariableName);
            if (location == -1)
            {
                throw new Exception($"{ModelMatrixVariableName} uniform not found on shader.");
            }

            Gl.Uniform4(location, ref color);
            CheckError();
        }

        private static unsafe void SetUpObjects()
        {
            // *********************************************************************************8 FISH

            //float[] fishColor = { 1.0f, 0.5f, 0.0f, 1.0f }; // Orange color for the fish
            float[] fishColor = { 1.0f, 0.5f, 0.0f, 1.0f };
            //CurrentColors.Add(fishColor);
            
            //fish = ObjResourceReader.CreateFishWithColor(Gl, fishColor);

            // in order to make the fish look smaller than theh drone
            float scaleX = 0.03f;
            float scaleY = 0.03f;
            float scaleZ = 0.03f;

            //fish = ObjResourceReader.CreateFishWithColor(Gl, fishColor, scaleX, scaleY, scaleZ);
            for(int i = 0; i<= 25; i++)
            {
                fish.Add(ObjResourceReader.CreateFishWithColor(Gl, fishColor, scaleX, scaleY, scaleZ));
            }

            //************************************************************************************

            float[] tableColor = { 0.5f, 0.5f, 0.5f, 1f };
            //teapot = ObjResourceReader.CreateTeapotWithColor(Gl, face1Color);
            teapot = ObjResourceReader.CreateTeapotWithColor(Gl, tableColor);

            // whale
             scaleX = 0.2f;
             scaleY = 0.2f;
             scaleZ = 0.2f;
            for(int i = 0; i<3; i++)
            {
                whale.Add(ObjResourceReader.CreateWhaleWithColor(Gl, tableColor, scaleX, scaleY, scaleZ));
            }
            //whale = ObjResourceReader.CreateWhaleWithColor(Gl, tableColor, scaleX, scaleY, scaleZ);

            //table = GlCube.CreateSquare(Gl, tableColor);

            skyBox = GlCube.CreateInteriorCube(Gl, "");
        }

        private static void Window_Closing()
        {
            teapot.ReleaseGlObject();
            //glCubeRotating.ReleaseGlObject();
            foreach(var f in fish)
            {
                f.ReleaseGlObject();
            }
            foreach(var w in whale)
            {
                w.ReleaseGlObject();
            }
            //whale.ReleaseGlObject();

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