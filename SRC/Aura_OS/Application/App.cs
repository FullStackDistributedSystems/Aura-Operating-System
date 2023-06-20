using Cosmos.System;
using Cosmos.System.Graphics;
using System.Drawing;
using Aura_OS.Processing;
using System;

namespace Aura_OS
{
    public class App : Process
    {
        public static bool HasWindowMoving = false;

        public Bitmap Icon;

        public readonly uint baseWidth;
        public readonly uint baseHeight;
        public readonly uint width;
        public readonly uint height;

        public uint dockX;
        public uint dockY;
        public uint dockWidth = 40;
        public uint dockHeight = 30;

        public uint baseX;
        public uint baseY;
        public uint x;
        public uint y;
        public string name;

        int px;
        int py;
        bool lck = false;

        bool pressed;
        public bool visible = false;

        const int MoveBarHeight = 20;

        public App(string name, uint width, uint height, uint x = 0, uint y = 0) : base(name, ProcessType.Program)
        {
            Icon = Kernel.programIco;

            baseWidth = width;
            baseHeight = height;
            baseX = x;
            baseY = y;

            this.x = x + 2;
            this.y = y + MoveBarHeight;
            this.width = width - 4;
            this.height = height - MoveBarHeight - 1;

            this.name = name;
        }

        public virtual void UpdateApp() { }

        public override void Initialize()
        {
            base.Initialize();

            Kernel.ProcessManager.Register(this);
        }

        public override void Update()
        {
            if (visible)
            {
                if (Kernel.Pressed)
                {
                    if (!HasWindowMoving && MouseManager.X > baseX && MouseManager.X < baseX + baseWidth && MouseManager.Y > baseY && MouseManager.Y < baseY + MoveBarHeight)
                    {
                        HasWindowMoving = true;

                        //Focus window
                        foreach (var app in Kernel.WindowManager.apps)
                        {
                            if (app.Equals(this))
                            {
                                Kernel.WindowManager.apps.Remove(app);
                                Kernel.WindowManager.apps.Add(this);
                                break;
                            }
                        }

                        this.pressed = true;
                        if (!lck)
                        {
                            px = (int)((int)MouseManager.X - this.baseX);
                            py = (int)((int)MouseManager.Y - this.baseY);
                            lck = true;
                        }
                    }
                }
                else
                {
                    pressed = false;
                    lck = false;
                    HasWindowMoving = false;
                }

                if (pressed)
                {
                    baseX = (uint)(MouseManager.X - px);
                    baseY = (uint)(MouseManager.Y - py);

                    x = (uint)(MouseManager.X - px + 2);
                    y = (uint)(MouseManager.Y - py + MoveBarHeight);
                }

                DrawWindow();

                //Kernel.canvas.DrawFilledRectangle(Kernel.WhiteColor, (int)baseX, (int)baseY, (int)baseWidth, (int)baseHeight);
                //Kernel.canvas.DrawRectangle(Kernel.avgColPen, (int)baseX, (int)baseY, (int)baseWidth, (int)baseHeight);

                //Kernel.canvas.DrawImage(Icon, (int)(baseX + 2), (int)(baseY + 2));
                //Kernel.canvas.DrawString(name, Kernel.font, Kernel.BlackColor, (int)(baseX + Icon.Width + 2), (int)(baseY + 2));
            }
        }

        public void DrawWindow()
        {
            // Calculate content dimensions
            int contentWidth = (int)(baseWidth - 1);
            int contentHeight = (int)(baseHeight - 1);

            //Window content
            Kernel.canvas.DrawFilledRectangle(Kernel.DarkGrayLight, (int)baseX + 2, (int)baseY + 2, contentWidth - 3, contentHeight - 3);

            UpdateApp();

            // Window border
            Kernel.canvas.DrawLine(Kernel.Gray, (int)baseX, (int)baseY, (int)(baseX + contentWidth), (int)baseY);
            Kernel.canvas.DrawLine(Kernel.WhiteColor, (int)baseX, (int)baseY + 1, (int)(baseX + contentWidth), (int)baseY + 1);
            Kernel.canvas.DrawLine(Kernel.Gray, (int)baseX, (int)baseY, (int)baseX, (int)baseY + contentHeight);
            Kernel.canvas.DrawLine(Kernel.WhiteColor, (int)baseX + 1, (int)baseY + 1, (int)baseX + 1, (int)baseY + contentHeight);
            Kernel.canvas.DrawLine(Kernel.DarkGray, (int)baseX + 1, (int)(baseY + contentHeight - 1), (int)baseX + contentWidth, (int)baseY + contentHeight - 1);
            Kernel.canvas.DrawLine(Kernel.BlackColor, (int)baseX, (int)(baseY + contentHeight), (int)baseX + contentWidth + 1, (int)baseY + contentHeight);
            Kernel.canvas.DrawLine(Kernel.DarkGray,(int)(baseX + contentWidth - 1), (int)baseY + 1, (int)(baseX + contentWidth - 1), (int)(baseY + contentHeight));
            Kernel.canvas.DrawLine(Kernel.BlackColor, (int)(baseX + contentWidth), (int)baseY, (int)baseX + contentWidth, (int)baseY + contentHeight);

            //Window bar
            Kernel.canvas.DrawFilledRectangle(Kernel.DarkBlue, (int)baseX + 3, (int)baseY + 3, contentWidth - 5, 18);
            Kernel.canvas.DrawString(name, Kernel.font, Kernel.WhiteColor, (int)(baseX + 2), (int)(baseY + 2));
            Kernel.canvas.DrawImage(Kernel.CloseNormal, (int)(baseX + contentWidth - 20), (int)(baseY + 5));
        }
    }
}