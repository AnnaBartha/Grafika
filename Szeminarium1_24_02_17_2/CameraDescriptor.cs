using Silk.NET.Maths;

namespace Szeminarium1_24_02_17_2
{
    internal class CameraDescriptor
    {
        private double DistanceToOrigin = 4;

        private double AngleToZYPlane = 0;

        private double AngleToZXPlane = 0;

        private const double DistanceScaleFactor = 1.1;

        private const double AngleChangeStepSize = Math.PI / 180 * 5;

        //********************
        private Vector3D<float> dronePosition; // Reference to the drone's position
        //***********************


        // ********************************** DRON POZICIIOJANAK BEALLITAA
        public void SetDronePosition(Vector3D<float> position)
        {
            dronePosition = position;
        }
        //*********************************************

        /// <summary>
        /// Gets the position of the camera.
        /// </summary>
        public Vector3D<float> Position
        {
            get
            {
                return GetPointFromAngles(DistanceToOrigin, AngleToZYPlane, AngleToZXPlane);
            }
        }
        /*public Vector3D<float> Position
        {
            get
            {
    
                    // Define an offset vector to position the camera slightly behind and above the drone
                    Vector3D<float> offset = new Vector3D<float>(0, 1, -2); // Adjust these values as needed

                    // Add the offset to the drone's position
                    return dronePosition + offset;
                
            }
        }*/


        /// <summary>
        /// Gets the up vector of the camera.
        /// </summary>
        public Vector3D<float> UpVector
        {
            get
            {
                return Vector3D.Normalize(GetPointFromAngles(DistanceToOrigin, AngleToZYPlane, AngleToZXPlane + Math.PI / 2));
            }
        }

        /// <summary>
        /// Gets the target point of the camera view.
        /// </summary>
        public Vector3D<float> Target
        {
            get
            {
                // For the moment the camera is always pointed at the origin.
                return Vector3D<float>.Zero;
            }
        }
        /*public Vector3D<float> Target
        {
            get
            {
                return dronePosition; // Camera targets the drone's position
            }
        }*/

        public void IncreaseZXAngle()
        {
            AngleToZXPlane += AngleChangeStepSize;
        }

        public void DecreaseZXAngle()
        {
            AngleToZXPlane -= AngleChangeStepSize;
        }

        public void IncreaseZYAngle()
        {
            AngleToZYPlane += AngleChangeStepSize;

        }

        public void DecreaseZYAngle()
        {
            AngleToZYPlane -= AngleChangeStepSize;
        }

        public void IncreaseDistance()
        {
            DistanceToOrigin = DistanceToOrigin * DistanceScaleFactor;
        }

        public void DecreaseDistance()
        {
            DistanceToOrigin = DistanceToOrigin / DistanceScaleFactor;
        }

        private static Vector3D<float> GetPointFromAngles(double distanceToOrigin, double angleToMinZYPlane, double angleToMinZXPlane)
        {
            var x = distanceToOrigin * Math.Cos(angleToMinZXPlane) * Math.Sin(angleToMinZYPlane);
            var z = distanceToOrigin * Math.Cos(angleToMinZXPlane) * Math.Cos(angleToMinZYPlane);
            var y = distanceToOrigin * Math.Sin(angleToMinZXPlane);

            return new Vector3D<float>((float)x, (float)y, (float)z);
        }

        // ************************************************* circular moving of camera
        /*public void MoveCircular(int direction)
        {
            // Calculate the new angle based on the direction
            double angleIncrement = AngleChangeStepSize * direction;

            // Update the angle
            AngleToZYPlane += angleIncrement;

            // Calculate the new camera position using the circular path formula
            double radius = DistanceToOrigin; // Use the distance to origin as the radius
            double x = radius * Math.Cos(AngleToZYPlane);
            double z = radius * Math.Sin(AngleToZYPlane);

            // Update the camera position
            dronePosition = new Vector3D<float>((float)x, dronePosition.Y, (float)z);
        }*/

        public void MoveCircular(int direction)
        {
            // Calculate the new angle based on the direction
            double angleIncrement = AngleChangeStepSize * direction;

            // Update the angle
            AngleToZXPlane += angleIncrement;

            // Calculate the new camera position using the circular path formula
            double radius = DistanceToOrigin; // Use the distance to origin as the radius
            double y = radius * Math.Sin(AngleToZXPlane);
            double z = radius * Math.Cos(AngleToZXPlane);

            // Update the camera position
            dronePosition = new Vector3D<float>(dronePosition.X, (float)y, (float)z);
        }

        // ********************************************************************************************
    }
}
