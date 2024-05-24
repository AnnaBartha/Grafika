using Silk.NET.Maths;
using System;

namespace Szeminarium1_24_02_17_2
{
    internal class CameraDescriptorDroneView
    {
        private const double DistanceScaleFactor = 1.1;
        private const double AngleChangeStepSize = Math.PI / 180 * 5;

        // Reference to the drone's position
        private Vector3D<float> dronePosition = new Vector3D<float> (0f,0f,0f);

        // Offset values to position the camera behind and above the drone
        private Vector3D<float> offset = new Vector3D<float>(0, 20, -10);

        // Set the drone's position
        public void SetDronePosition(Vector3D<float> position)
        {
            dronePosition = position;
        }

        // Gets the position of the camera
        public Vector3D<float> Position
        {
            get
            {
                return dronePosition + offset;
            }
        }

        // Gets the up vector of the camera
        public Vector3D<float> UpVector
        {
            get
            {
                // Assuming up vector is always pointing up in the world space (0, 1, 0)
                return new Vector3D<float>(0, 1, 0);
            }
        }

        // Gets the target point of the camera view
        public Vector3D<float> Target
        {
            get
            {
                return dronePosition;
            }
        }

      
    }
}
