/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Taskbar
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System.Collections.Generic;
using Cosmos.System;
using Aura_OS.System.Graphics.UI.GUI.Components;
using Aura_OS.System.Processing.Processes;

namespace Aura_OS.System.Graphics.UI.GUI
{
    public class HourButton : Button
    {
        public HourButton(string text, int x, int y, int width, int height) : base(text, x, y, width, height)
        {
            Light = true;
            ForceDirty = true;
        }

        public override void Draw()
        {
            base.Draw();

            // Hour
            string time = Time.TimeString(true, true, true);
            Text = time;
        }
    }

    public class Taskbar : Panel
    {
        public static int taskbarHeight = 33;

        Button StartButton;
        Button HourButton;
        Button NetworkButton;

        public bool Clicked = false;

        public Dictionary<uint, Button> Buttons;

        public Taskbar() : base(Kernel.Gray, 0, (int)Kernel.screenHeight - taskbarHeight, (int)Kernel.screenWidth, taskbarHeight)
        {
            // Start button
            int startButtonWidth = 70;
            int startButtonHeight = 28;
            int startButtonX = 2;
            int startButtonY = 2;
            StartButton = new Button(Kernel.ResourceManager.GetIcon("00-start.bmp"), "Start", startButtonX, startButtonY, startButtonWidth, startButtonHeight);
            StartButton.HasTransparency = true;
            StartButton.Frame = Kernel.ThemeManager.GetFrame("button.disabled");
            AddChild(StartButton);

            // Hour button
            string time = Time.TimeString(true, true, true);
            int hourButtonWidth = time.Length * (Kernel.font.Width + 1);
            int hourButtonHeight = 28;
            int hourButtonX = (int)(Kernel.screenWidth - time.Length * (Kernel.font.Width + 1) - 2);
            int hourButtonY = 2;
            HourButton = new HourButton(time, hourButtonX, hourButtonY, hourButtonWidth, hourButtonHeight);
            HourButton.Frame = Kernel.ThemeManager.GetFrame("button.disabled");
            AddChild(HourButton);

            // Network icon
            int networkButtonWidth = 16;
            int networkButtonHeight = 16;
            int netoworkButtonX = (int)(Kernel.screenWidth - time.Length * (Kernel.font.Width + 1) - 2) - 20;
            int networkButtonY = (taskbarHeight / 2) - (networkButtonHeight / 2);
            NetworkButton = new Button(Kernel.ResourceManager.GetIcon("16-network-offline.bmp"), netoworkButtonX, networkButtonY, networkButtonWidth, networkButtonHeight);
            NetworkButton.NoBorder = true;
            AddChild(NetworkButton);

            Buttons = new Dictionary<uint, Button>();
        }

        public override void Update()
        {
            if (MouseManager.MouseState == MouseState.Left)
            {
                if (!Clicked)
                {
                    Clicked = true;
                }
            }
            else if (Clicked)
            {
                Clicked = false;
                HandleClick();
            }
        }

        private void HandleClick()
        {
            MarkDirty();

            if (StartButton.IsInside((int)MouseManager.X, (int)MouseManager.Y))
            {
                Explorer.ShowStartMenu = !Explorer.ShowStartMenu;
            }

            foreach (var application in  Explorer.WindowManager.Applications)
            {
                if (Buttons[application.ID].IsInside((int)MouseManager.X, (int)MouseManager.Y))
                {
                    if (application.Visible)
                    {
                        application.Window.Minimize.Click();
                    }
                    else
                    {
                        application.Window.Maximize.Click();
                    }
                }
            }
        }

        public void UpdateApplicationButtons()
        {
            foreach (var button in Buttons.Values)
            {
                Children.Remove(button);
            }
            Buttons.Clear();

            int buttonX = 74;

            foreach (var app in Explorer.WindowManager.Applications)
            {
                string appName = app.Name + " (" + app.ID.ToString() + ")";
                var spacing = appName.Length * 9 + (int)app.Window.Icon.Width;
                var button = new Button(app.Window.Icon, appName, buttonX, 2, spacing, 28);
                button.Frame = Kernel.ThemeManager.GetFrame("button.disabled");

                AddChild(button);
                Buttons.Add(app.ID, button);

                buttonX += spacing + 4;
            }

            MarkDirty();
        }

        public override void Draw()
        {
            base.Draw();

            // Taskbar
            //DrawLine(Kernel.WhiteColor, 0, 0, (int)Kernel.screenWidth + 10, 1); TODO
            //DrawFilledRectangle(Kernel.Gray, 0, startY + 1, (int)Kernel.screenWidth, taskbarHeight - 1);
            StartButton.Draw(this);

            // Notifications
            DrawNotifications();

            // Applications
            DrawApplications();
        }

        private void DrawApplications()
        {
            foreach (var button in Buttons)
            {
                uint pid = button.Key;
                Button btn = button.Value;
                Application application = Kernel.ApplicationManager.GetApplicationByPid(pid);

                if (application.Focused)
                {
                    btn.Frame = Kernel.ThemeManager.GetFrame("button.normal");
                }
                else
                {
                    btn.Frame = Kernel.ThemeManager.GetFrame("button.disabled");
                }

                button.Value.Draw(this);
            }
        }

        public void DrawNotifications()
        {
            if (Kernel.NetworkTransmitting)
            {
                //NetworkButton.Image = Kernel.networkTransmitIco;
            }
            else
            {
                if (Kernel.NetworkConnected)
                {
                    NetworkButton.Image = Kernel.ResourceManager.GetIcon("16-network-idle.bmp");
                }
                else
                {
                    NetworkButton.Image = Kernel.ResourceManager.GetIcon("16-network-offline.bmp");
                }
            }

            NetworkButton.Draw(this);
        }

        public override void MarkDirty()
        {
            base.MarkDirty();

            foreach (var button in Buttons)
            {
                button.Value.MarkDirty();
            }
        }
    }
}
