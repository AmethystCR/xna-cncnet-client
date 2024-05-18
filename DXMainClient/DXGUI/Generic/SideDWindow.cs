using System;
using ClientGUI;
using Microsoft.Xna.Framework;
using Rampastring.XNAUI;
using Rampastring.XNAUI.XNAControls;
using ClientCore.Extensions;

namespace DTAClient.DXGUI.Generic
{
    public class SideDWindow : XNAWindow
    {
        public SideDWindow(WindowManager windowManager) : base(windowManager)
        {
        }
        private XNAClientTabControl tabControl;
        private XNAClientButton btnDbCancel;

        public override void Initialize()
        {
            Name = "SideDWindow";
            ClientRectangle = new Rectangle(0, 0, 1280, 800);
            BackgroundTexture = AssetLoader.LoadTexture("Database/SideDM.png");

            tabControl = new XNAClientTabControl(WindowManager);
            tabControl.Name = "tabControl";
            tabControl.ClientRectangle = new Rectangle(128, 40, 0, 0);
            tabControl.ClickSound = new EnhancedSoundEffect("MainMenu/button.wav");
            tabControl.FontIndex = 1;
            tabControl.AddTab("SideDM".L10N("Client:Main:SideDM"), UIDesignConstants.BUTTON_WIDTH_121);
            tabControl.AddTab("SideDT1".L10N("Client:Main:SideDT1"), UIDesignConstants.BUTTON_WIDTH_121);
            tabControl.AddTab("SideDT2".L10N("Client:Main:SideDT2"), UIDesignConstants.BUTTON_WIDTH_121);
            tabControl.AddTab("SideDT3".L10N("Client:Main:SideDT3"), UIDesignConstants.BUTTON_WIDTH_121);
            tabControl.AddTab("SideDT4".L10N("Client:Main:SideDT4"), UIDesignConstants.BUTTON_WIDTH_121);
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
                BackgroundTexture = AssetLoader.LoadTexture("Database/SideD-T1.png");
            }
            else if (tabControl.SelectedTab == 2)
            {
                BackgroundTexture = AssetLoader.LoadTexture("Database/SideD-T2.png");
            }
            else if (tabControl.SelectedTab == 3)
            {
                BackgroundTexture = AssetLoader.LoadTexture("Database/SideD-T3.png");
            }
            else if (tabControl.SelectedTab == 4)
            {
                BackgroundTexture = AssetLoader.LoadTexture("Database/SideD-T4.png");
            }
            else
            {
                BackgroundTexture = AssetLoader.LoadTexture("Database/SideDM.png");
            }
        }

        private void BtnDbCancel_LeftClick(object sender, EventArgs e)
        {
            Disable();
        }
    }
}
