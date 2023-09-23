using Meadow.Devices;
using Meadow.Devices.Esp32.MessagePayloads;
using Meadow.Foundation;
using Meadow.Foundation.Audio;
using Meadow.Foundation.Graphics;
using Meadow.Units;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectLab_Demo
{
    public partial class DisplayController
    {
        readonly MicroGraphics graphics;

        public DisplayController(IGraphicsDisplay display)
        {
            graphics = new MicroGraphics(display)
            {
                CurrentFont = new Font12x16()
            };

            graphics.Clear(true);
        }

        ////////////////////////////////////////////////////////////////////////
        
        /// <summary>
        /// The code that follows has been left in for possible later use.
        /// </summary>
        public (Temperature? Temperature, RelativeHumidity? Humidity, Pressure? Pressure, Resistance? GasResistance)? AtmosphericConditions
        {
            get => atmosphericConditions;
            set
            {
                atmosphericConditions = value;
                Update();
            }
        }
        (Temperature? Temperature, RelativeHumidity? Humidity, Pressure? Pressure, Resistance? GasResistance)? atmosphericConditions;

        public Illuminance? LightConditions
        {
            get => lightConditions;
            set
            {
                lightConditions = value;
                Update();
            }
        }
        Illuminance? lightConditions;

        public (Acceleration3D? Acceleration3D, AngularVelocity3D? AngularVelocity3D, Temperature? Temperature) AccelerationConditions
        {
            get => accelerationConditions;
            set
            {
                accelerationConditions = value;
                Update();
            }
        }
        (Acceleration3D? Acceleration3D, AngularVelocity3D? AngularVelocity3D, Temperature? Temperature) accelerationConditions;


        public bool UpButtonState
        {
            get => upButtonState;
            set
            {
                upButtonState = value;
                Update();
            }
        }
        bool upButtonState = false;

        public bool DownButtonState
        {
            get => downButtonState;
            set
            {
                downButtonState = value;
                Update();
            }
        }
        bool downButtonState = false;

        public bool LeftButtonState
        {
            get => leftButtonState;
            set
            {
                leftButtonState = value;
                Update();
            }
        }
        bool leftButtonState = false;

        public bool RightButtonState
        {
            get => rightButtonState;
            set
            {
                rightButtonState = value;
                Update();
            }
        }
        bool rightButtonState = false;

        bool isUpdating = false;
        bool needsUpdate = false;



        public void Update()
        {
            return;
            if (isUpdating)
            {   //queue up the next update
                needsUpdate = true;
                return;
            }

            isUpdating = true;

            graphics.Clear();
            Draw();
            graphics.Show();

            isUpdating = false;

            if (needsUpdate)
            {
                needsUpdate = false;
                Update();
            }
        }

        /// <summary>
        /// Not used
        /// </summary>
        /// <param name="label"></param>
        /// <param name="value"></param>
        /// <param name="color"></param>
        /// <param name="yPosition"></param>
        void DrawStatus(string label, string value, Color color, int yPosition)
        {
            graphics.DrawText(x: 2, y: yPosition, label, color: color);
            graphics.DrawText(x: graphics.Width - 2, y: yPosition, value, alignmentH: HorizontalAlignment.Right, color: color);
        }


        /// <summary>
        /// An example of writing text (not used);
        /// Note spacing at 20 is pixels not lines.
        /// </summary>
        /// <param name="message">Text to display at top</param>
        void DrawTexts(string message)
        {
            graphics.Clear(true);

            int indent = 20;
            int spacing = 20;
            int y = 5;

            graphics.CurrentFont = new Font12x16();
            graphics.DrawText(indent, y, message);
            graphics.DrawText(indent, y += spacing, "Red", Color.Red);
            graphics.DrawText(indent, y += spacing, "Purple", Color.Purple);
            graphics.DrawText(indent, y += spacing, "BlueViolet", Color.BlueViolet);
            graphics.DrawText(indent, y += spacing, "Blue", Color.Blue);
            graphics.DrawText(indent, y += spacing, "Cyan", Color.Cyan);
            graphics.DrawText(indent, y += spacing, "LawnGreen", Color.LawnGreen);
            graphics.DrawText(indent, y += spacing, "GreenYellow", Color.GreenYellow);
            graphics.DrawText(indent, y += spacing, "Yellow", Color.Yellow);
            graphics.DrawText(indent, y += spacing, "Orange", Color.Orange);
            graphics.DrawText(indent, y += spacing, "Brown", Color.Brown);
            graphics.Show();

            Thread.Sleep(5000);
        }

        /// <summary>
        /// Unchanged from origibal (not used)
        /// </summary>
        void Draw()
        {
            graphics.DrawText(x: 2, y: 0, "Hello PROJ LAB!", WildernessLabsColors.AzureBlue);

            if (AtmosphericConditions is { } conditions)
            {
                if (conditions.Temperature is { } temp)
                {
                    DrawStatus("Temperature:", $"{temp.Celsius:N1}C", WildernessLabsColors.GalleryWhite, 35);
                }

                if (conditions.Pressure is { } pressure)
                {
                    DrawStatus("Pressure:", $"{pressure.StandardAtmosphere:N1}atm", WildernessLabsColors.GalleryWhite, 55);
                }

                if (conditions.Humidity is { } humidity)
                {
                    DrawStatus("Humidity:", $"{humidity.Percent:N1}%", WildernessLabsColors.GalleryWhite, 75);
                }
            }

            if (LightConditions is { } light)
            {
                DrawStatus("Lux:", $"{light:N0}Lux", WildernessLabsColors.GalleryWhite, 95);
            }

            if (AccelerationConditions is { } acceleration)
            {
                if (acceleration.Acceleration3D is { } accel3D)
                {
                    DrawStatus("Accel:", $"{accel3D.X.Gravity:0.#},{accel3D.Y.Gravity:0.#},{accel3D.Z.Gravity:0.#}g", WildernessLabsColors.AzureBlue, 115);
                }

                if (acceleration.AngularVelocity3D is { } angular3D)
                {
                    DrawStatus("Gyro:", $"{angular3D.X:0},{angular3D.Y:0},{angular3D.Z:0}rpm", WildernessLabsColors.AzureBlue, 135);
                }
            }

            DrawStatus("Left:", $"{(LeftButtonState ? "pressed" : "released")}", WildernessLabsColors.ChileanFire, 200);
            DrawStatus("Down:", $"{(DownButtonState ? "pressed" : "released")}", WildernessLabsColors.ChileanFire, 180);
            DrawStatus("Up:", $"{(UpButtonState ? "pressed" : "released")}", WildernessLabsColors.ChileanFire, 160);
            DrawStatus("Right:", $"{(RightButtonState ? "pressed" : "released")}", WildernessLabsColors.ChileanFire, 220);
        }  
    }
}