using System;
using ClientGUI;
using Microsoft.Xna.Framework;
using Rampastring.XNAUI;
using Rampastring.XNAUI.XNAControls;
using ClientCore.Extensions;

namespace DTAClient.DXGUI.Generic
{
    public class SideSWindow : XNAWindow
    {
        public SideSWindow(WindowManager windowManager) : base(windowManager)
        {
        }
        private XNAClientTabControl tabControl;
        private XNAClientButton btnDbCancel;

        public override void Initialize()
        {
            Name = "SideSWindow";
            ClientRectangle = new Rectangle(0, 0, 1280, 800);
            BackgroundTexture = AssetLoader.LoadTexture("Database/SideSM.png");

            tabControl = new XNAClientTabControl(WindowManager);
            tabControl.Name = "tabControl";
            tabControl.ClientRectangle = new Rectangle(128, 40, 0, 0);
            tabControl.ClickSound = new EnhancedSoundEffect("MainMenu/button.wav");
            tabControl.FontIndex = 1;
            tabControl.AddTab("SideSM".L10N("Client:Main:SideSM"), UIDesignConstants.BUTTON_WIDTH_121);
            tabControl.AddTab("SideST1".L10N("Client:Main:SideST1"), UIDesignConstants.BUTTON_WIDTH_121);
            tabControl.AddTab("SideST2".L10N("Client:Main:SideST2"), UIDesignConstants.BUTTON_WIDTH_121);
            tabControl.AddTab("SideST3".L10N("Client:Main:SideST3"), UIDesignConstants.BUTTON_WIDTH_121);
            tabControl.AddTab("SideST4".L10N("Client:Main:SideST4"), UIDesignConstants.BUTTON_WIDTH_121);
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
                BackgroundTexture = AssetLoader.LoadTexture("Database/SideS-T1.png");
            }
            else if (tabControl.SelectedTab == 2)
            {
                BackgroundTexture = AssetLoader.LoadTexture("Database/SideS-T2.png");
            }
            else if (tabControl.SelectedTab == 3)
            {
                BackgroundTexture = AssetLoader.LoadTexture("Database/SideS-T3.png");
            }
            else if (tabControl.SelectedTab == 4)
            {
                BackgroundTexture = AssetLoader.LoadTexture("Database/SideS-T4.png");
            }
            else
            {
                BackgroundTexture = AssetLoader.LoadTexture("Database/SideSM.png");
            }
        }

        private void BtnDbCancel_LeftClick(object sender, EventArgs e)
        {
            Disable();
        }
    }
}
