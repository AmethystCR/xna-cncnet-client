using System;
using ClientGUI;
using Microsoft.Xna.Framework;
using Rampastring.XNAUI;
using Rampastring.XNAUI.XNAControls;
using ClientCore.Extensions;

namespace DTAClient.DXGUI.Generic
{
    public class DatabaseWindow : XNAWindow
    {
        public DatabaseWindow(WindowManager windowManager) : base(windowManager)
        {
        }
        private SideAWindow sideAWindow;
        private SideBWindow sideBWindow;
        private SideCWindow sideCWindow;
        private SideDWindow sideDWindow;
        private SideSWindow sideSWindow;

        public override void Initialize()
        {
            Name = "DatabaseWindow";
            ClientRectangle = new Rectangle(0, 0, 1280, 800);
            BackgroundTexture = AssetLoader.LoadTexture("DatabaseWindow.png");

            var btnSideA = new XNAClientButton(WindowManager);
            btnSideA.Name = "btnSideA";
            btnSideA.ClientRectangle = new Rectangle(365, 75, 250, 250);
            btnSideA.IdleTexture = AssetLoader.LoadTexture("Database/SideA.png");
            btnSideA.HoverTexture = AssetLoader.LoadTexture("Database/SideA_c.png");
            btnSideA.HoverSoundEffect = new EnhancedSoundEffect("MainMenu/button.wav");
            btnSideA.Text = " ";
            btnSideA.LeftClick += BtnSideA_LeftClick;

            var btnSideB = new XNAClientButton(WindowManager);
            btnSideB.Name = "btnSideB";
            btnSideB.ClientRectangle = new Rectangle(665, 75, 250, 250);
            btnSideB.IdleTexture = AssetLoader.LoadTexture("Database/SideB.png");
            btnSideB.HoverTexture = AssetLoader.LoadTexture("Database/SideB_c.png");
            btnSideB.HoverSoundEffect = new EnhancedSoundEffect("MainMenu/button.wav");
            btnSideB.Text = " ";
            btnSideB.LeftClick += BtnSideB_LeftClick;

            var btnSideC = new XNAClientButton(WindowManager);
            btnSideC.Name = "btnSideC";
            btnSideC.ClientRectangle = new Rectangle(390, 400, 200, 200);
            btnSideC.IdleTexture = AssetLoader.LoadTexture("Database/SideC.png");
            btnSideC.HoverTexture = AssetLoader.LoadTexture("Database/SideC_c.png");
            btnSideC.HoverSoundEffect = new EnhancedSoundEffect("MainMenu/button.wav");
            btnSideC.Text = " ";
            btnSideC.LeftClick += BtnSideC_LeftClick;

            var btnSideD = new XNAClientButton(WindowManager);
            btnSideD.Name = "btnSideD";
            btnSideD.ClientRectangle = new Rectangle(665, 375, 250, 250);
            btnSideD.IdleTexture = AssetLoader.LoadTexture("Database/SideD.png");
            btnSideD.HoverTexture = AssetLoader.LoadTexture("Database/SideD_c.png");
            btnSideD.HoverSoundEffect = new EnhancedSoundEffect("MainMenu/button.wav");
            btnSideD.Text = " ";
            btnSideD.LeftClick += BtnSideD_LeftClick;

            var btnSideS = new XNAClientButton(WindowManager);
            btnSideS.Name = "btnSideS";
            btnSideS.ClientRectangle = new Rectangle(515, 225, 150, 150);
            btnSideS.IdleTexture = AssetLoader.LoadTexture("Database/SideS.png");
            btnSideS.HoverTexture = AssetLoader.LoadTexture("Database/SideS_c.png");
            btnSideS.HoverSoundEffect = new EnhancedSoundEffect("MainMenu/button.wav");
            btnSideS.Text = " ";
            btnSideS.LeftClick += BtnSideS_LeftClick;

            var btnDbCancel = new XNAClientButton(WindowManager);
            btnDbCancel.Name = "btnDbCancel";
            btnDbCancel.ClientRectangle = new Rectangle(580, 750, UIDesignConstants.BUTTON_WIDTH_121, UIDesignConstants.BUTTON_HEIGHT);
            btnDbCancel.IdleTexture = AssetLoader.LoadTexture("121pxbtn.png");
            btnDbCancel.HoverTexture = AssetLoader.LoadTexture("121pxbtn_c.png");
            btnDbCancel.HoverSoundEffect = new EnhancedSoundEffect("MainMenu/button.wav");
            btnDbCancel.Text = "Cancel".L10N("Client:Main:ExCancel");
            btnDbCancel.LeftClick += BtnDbCancel_LeftClick;

            AddChild(btnSideA);
            AddChild(btnSideB);
            AddChild(btnSideC);
            AddChild(btnSideD);
            AddChild(btnSideS);
            AddChild(btnDbCancel);

            base.Initialize();
            CenterOnParent();

            sideAWindow = new SideAWindow(WindowManager);
            var dp1 = new DarkeningPanel(WindowManager);
            dp1.AddChild(sideAWindow);
            AddChild(dp1);
            dp1.CenterOnParent();
            sideAWindow.CenterOnParent();
            sideAWindow.Disable();

            sideBWindow = new SideBWindow(WindowManager);
            var dp2 = new DarkeningPanel(WindowManager);
            dp2.AddChild(sideBWindow);
            AddChild(dp2);
            dp2.CenterOnParent();
            sideBWindow.CenterOnParent();
            sideBWindow.Disable();

            sideCWindow = new SideCWindow(WindowManager);
            var dp3 = new DarkeningPanel(WindowManager);
            dp3.AddChild(sideCWindow);
            AddChild(dp3);
            dp3.CenterOnParent();
            sideCWindow.CenterOnParent();
            sideCWindow.Disable();

            sideDWindow = new SideDWindow(WindowManager);
            var dp4 = new DarkeningPanel(WindowManager);
            dp4.AddChild(sideDWindow);
            AddChild(dp4);
            dp4.CenterOnParent();
            sideDWindow.CenterOnParent();
            sideDWindow.Disable();

            sideSWindow = new SideSWindow(WindowManager);
            var dps = new DarkeningPanel(WindowManager);
            dps.AddChild(sideSWindow);
            AddChild(dps);
            dps.CenterOnParent();
            sideSWindow.CenterOnParent();
            sideSWindow.Disable();
        }

        private void BtnSideA_LeftClick(object sender, EventArgs e) => sideAWindow.Enable();
        private void BtnSideB_LeftClick(object sender, EventArgs e) => sideBWindow.Enable();
        private void BtnSideC_LeftClick(object sender, EventArgs e) => sideCWindow.Enable();
        private void BtnSideD_LeftClick(object sender, EventArgs e) => sideDWindow.Enable();
        private void BtnSideS_LeftClick(object sender, EventArgs e) => sideSWindow.Enable();

        private void BtnDbCancel_LeftClick(object sender, EventArgs e)
        {
            Enabled = false;
        }
    }
}