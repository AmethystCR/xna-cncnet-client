using System;
using ClientGUI;
using Microsoft.Xna.Framework;
using Rampastring.XNAUI;
using Rampastring.XNAUI.XNAControls;
using ClientCore.Extensions;

namespace DTAClient.DXGUI.Generic
{
    public class SideCWindow : XNAWindow
    {
        public SideCWindow(WindowManager windowManager) : base(windowManager)
        {
        }
        private XNAClientTabControl tabControl;
        private XNAClientButton btnDbCancel;

        public override void Initialize()
        {
            Name = "SideCWindow";
            ClientRectangle = new Rectangle(0, 0, 1280, 800);
            BackgroundTexture = AssetLoader.LoadTexture("Database/SideCM.png");

            tabControl = new XNAClientTabControl(WindowManager);
            tabControl.Name = "tabControl";
            tabControl.ClientRectangle = new Rectangle(128, 40, 0, 0);
            tabControl.ClickSound = new EnhancedSoundEffect("MainMenu/button.wav");
            tabControl.FontIndex = 1;
            tabControl.AddTab("SideCM".L10N("Client:Main:SideCM"), UIDesignConstants.BUTTON_WIDTH_121);
            tabControl.AddTab("SideCT1".L10N("Client:Main:SideCT1"), UIDesignConstants.BUTTON_WIDTH_121);
            tabControl.AddTab("SideCT2".L10N("Client:Main:SideCT2"), UIDesignConstants.BUTTON_WIDTH_121);
            tabControl.AddTab("SideCT3".L10N("Client:Main:SideCT3"), UIDesignConstants.BUTTON_WIDTH_121);
            tabControl.AddTab("SideCT4".L10N("Client:Main:SideCT4"), UIDesignConstants.BUTTON_WIDTH_121);
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
                BackgroundTexture = AssetLoader.LoadTexture("Database/SideC-T1.png");
            }
            else if (tabControl.SelectedTab == 2)
            {
                BackgroundTexture = AssetLoader.LoadTexture("Database/SideC-T2.png");
            }
            else if (tabControl.SelectedTab == 3)
            {
                BackgroundTexture = AssetLoader.LoadTexture("Database/SideC-T3.png");
            }
            else if (tabControl.SelectedTab == 4)
            {
                BackgroundTexture = AssetLoader.LoadTexture("Database/SideC-T4.png");
            }
            else
            {
                BackgroundTexture = AssetLoader.LoadTexture("Database/SideCM.png");
            }
        }

        private void BtnDbCancel_LeftClick(object sender, EventArgs e)
        {
            Disable();
        }
    }
}
