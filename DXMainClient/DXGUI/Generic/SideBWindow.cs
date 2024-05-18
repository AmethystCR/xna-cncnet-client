using System;
using ClientGUI;
using Microsoft.Xna.Framework;
using Rampastring.XNAUI;
using Rampastring.XNAUI.XNAControls;
using ClientCore.Extensions;

namespace DTAClient.DXGUI.Generic
{
    public class SideBWindow : XNAWindow
    {
        public SideBWindow(WindowManager windowManager) : base(windowManager)
        {
        }
        private XNAClientTabControl tabControl;
        private XNAClientButton btnDbCancel;

        public override void Initialize()
        {
            Name = "SideBWindow";
            ClientRectangle = new Rectangle(0, 0, 1280, 800);
            BackgroundTexture = AssetLoader.LoadTexture("Database/SideBM.png");

            tabControl = new XNAClientTabControl(WindowManager);
            tabControl.Name = "tabControl";
            tabControl.ClientRectangle = new Rectangle(128, 40, 0, 0);
            tabControl.ClickSound = new EnhancedSoundEffect("MainMenu/button.wav");
            tabControl.FontIndex = 1;
            tabControl.AddTab("SideBM".L10N("Client:Main:SideBM"), UIDesignConstants.BUTTON_WIDTH_121);
            tabControl.AddTab("SideBT1".L10N("Client:Main:SideBT1"), UIDesignConstants.BUTTON_WIDTH_121);
            tabControl.AddTab("SideBT2".L10N("Client:Main:SideBT2"), UIDesignConstants.BUTTON_WIDTH_121);
            tabControl.AddTab("SideBT3".L10N("Client:Main:SideBT3"), UIDesignConstants.BUTTON_WIDTH_121);
            tabControl.AddTab("SideBT4".L10N("Client:Main:SideBT4"), UIDesignConstants.BUTTON_WIDTH_121);
            tabControl.SelectedIndexChanged += TabControl_SelectedIndexChanged;

            btnDbCancel = new XNAClientButton(WindowManager);
            btnDbCancel.Name = "btnDbCancel";
            btnDbCancel.ClientRectangle = new Rectangle(tabControl.Right + UIDesignConstants.BUTTON_WIDTH_121, tabControl.Y, UIDesignConstants.BUTTON_WIDTH_121, UIDesignConstants.BUTTON_HEIGHT);
            btnDbCancel.Text = "Cancel".L10N("Client:Main:ExCancel");
            btnDbCancel.LeftClick += BtnDbCancel_LeftClick;

            AddChild(tabControl);
            AddChild(btnDbCancel);

            base.Initialize();
            CenterOnParent();
        }

        private void TabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl.SelectedTab == 1)
            {
                BackgroundTexture = AssetLoader.LoadTexture("Database/SideB-T1.png");
            }
            else if (tabControl.SelectedTab == 2)
            {
                BackgroundTexture = AssetLoader.LoadTexture("Database/SideB-T2.png");
            }
            else if (tabControl.SelectedTab == 3)
            {
                BackgroundTexture = AssetLoader.LoadTexture("Database/SideB-T3.png");
            }
            else if (tabControl.SelectedTab == 4)
            {
                BackgroundTexture = AssetLoader.LoadTexture("Database/SideB-T4.png");
            }
            else
            {
                BackgroundTexture = AssetLoader.LoadTexture("Database/SideBM.png");
            }
        }

        private void BtnDbCancel_LeftClick(object sender, EventArgs e)
        {
            Disable();
        }
    }
}
