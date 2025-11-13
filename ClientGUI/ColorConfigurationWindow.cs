using ClientCore.Extensions;
using ClientCore;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Rampastring.Tools;
using Rampastring.XNAUI;
using Rampastring.XNAUI.XNAControls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ClientGUI
{
    /// <summary>
    /// A window for configuring in-game unified colors.
    /// </summary>
    public class ColorConfigurationWindow : XNAWindow
    {
        public ColorConfigurationWindow(WindowManager windowManager) : base(windowManager)
        {
        }
        private XNALabel lblColorsOne;
        private XNALabel lblColorsTwo;
        private XNALabel lblColorsThree;
        private XNALabel lblColorsFour;

        private XNAClientDropDown ddColorsOne;
        private XNAClientDropDown ddColorsTwo;
        private XNAClientDropDown ddColorsThree;
        private XNAClientDropDown ddColorsFour;

        private IniFile UTColorsINI;

        protected List<UnifiedTechnoColor> UTColors;

        public override void Initialize()
        {
            Name = "ColorConfigurationWindow";
            ClientRectangle = new Rectangle(0, 0, 600, 450);
            BackgroundTexture = AssetLoader.LoadTextureUncached("hotkeybg.png");

            UTColors = UnifiedTechnoColor.LoadColors();
            UTColorsINI = new IniFile(SafePath.CombineFilePath(ProgramConstants.GamePath, "INI", "Mod Code", "rulesmd_x_utcolors.ini"));

            lblColorsOne = new XNALabel(WindowManager);
            lblColorsOne.Name = "lblColorsOne";
            lblColorsOne.ClientRectangle = new Rectangle(40, 40, 0, 0);
            lblColorsOne.FontIndex = 1;
            lblColorsOne.Text = "Self Color:".L10N("Client:Main:UTSelfColor");

            ddColorsOne = new XNAClientDropDown(WindowManager);
            ddColorsOne.Name = "ddColorsOne";
            ddColorsOne.ClientRectangle = new Rectangle(40, 70, 100, 21);

            lblColorsTwo = new XNALabel(WindowManager);
            lblColorsTwo.Name = "lblColorsTwo";
            lblColorsTwo.ClientRectangle = new Rectangle(180, 40, 0, 0);
            lblColorsTwo.FontIndex = 1;
            lblColorsTwo.Text = "Ally Color:".L10N("Client:Main:UTAllyColor");

            ddColorsTwo = new XNAClientDropDown(WindowManager);
            ddColorsTwo.Name = "ddColorsTwo";
            ddColorsTwo.ClientRectangle = new Rectangle(180, 70, 100, 21);

            lblColorsThree = new XNALabel(WindowManager);
            lblColorsThree.Name = "lblColorsThree";
            lblColorsThree.ClientRectangle = new Rectangle(320, 40, 0, 0);
            lblColorsThree.FontIndex = 1;
            lblColorsThree.Text = "Enemy Color:".L10N("Client:Main:UTEnemyColor");

            ddColorsThree = new XNAClientDropDown(WindowManager);
            ddColorsThree.Name = "ddColorsThree";
            ddColorsThree.ClientRectangle = new Rectangle(320, 70, 100, 21);

            lblColorsFour = new XNALabel(WindowManager);
            lblColorsFour.Name = "lblColorsFour";
            lblColorsFour.ClientRectangle = new Rectangle(460, 40, 0, 0);
            lblColorsFour.FontIndex = 1;
            lblColorsFour.Text = "Neutral Color:".L10N("Client:Main:UTNeutralColor");

            ddColorsFour = new XNAClientDropDown(WindowManager);
            ddColorsFour.Name = "ddColorsFour";
            ddColorsFour.ClientRectangle = new Rectangle(460, 70, 100, 21);

            foreach (UnifiedTechnoColor UTColor in UTColors)
            {
                XNADropDownItem item = new()
                {
                    Text = UTColor.Name.L10N($"INI:Colors:{UTColor.Name}"),
                    Tag = UTColor,
                    TextColor = UTColor.XnaColor,
                };
                ddColorsOne.AddItem(item);
                ddColorsTwo.AddItem(item);
                ddColorsThree.AddItem(item);
                ddColorsFour.AddItem(item);
            }

            var btnSave = new XNAClientButton(WindowManager);
            btnSave.Name = "btnSave";
            btnSave.ClientRectangle = new Rectangle(40, 410 - UIDesignConstants.BUTTON_HEIGHT, 
                UIDesignConstants.BUTTON_WIDTH_92, UIDesignConstants.BUTTON_HEIGHT);
            btnSave.Text = "Save".L10N("Client:DTAConfig:ButtonSave");
            btnSave.LeftClick += BtnSave_LeftClick;

            var btnResetAllColors = new XNAClientButton(WindowManager);
            btnResetAllColors.Name = "btnResetColorToDefaults";
            btnResetAllColors.ClientRectangle = new Rectangle(0, btnSave.Y, 
                UIDesignConstants.BUTTON_WIDTH_121, UIDesignConstants.BUTTON_HEIGHT);
            btnResetAllColors.Text = "Reset All Colors".L10N("Client:DTAConfig:ResetAllColors");
            btnResetAllColors.LeftClick += BtnResetAllColors_LeftClick;
            AddChild(btnResetAllColors);
            btnResetAllColors.CenterOnParentHorizontally();

            var btnCancel = new XNAClientButton(WindowManager);
            btnCancel.Name = "btnExit";
            btnCancel.ClientRectangle = new Rectangle(560 - UIDesignConstants.BUTTON_WIDTH_92, btnSave.Y, 
                UIDesignConstants.BUTTON_WIDTH_92, UIDesignConstants.BUTTON_HEIGHT);
            btnCancel.Text = "Cancel".L10N("Client:DTAConfig:ButtonCancel");
            btnCancel.LeftClick += BtnCancel_LeftClick;

            AddChild(lblColorsOne);
            AddChild(lblColorsTwo);
            AddChild(lblColorsThree);
            AddChild(lblColorsFour);
            AddChild(ddColorsOne);
            AddChild(ddColorsTwo);
            AddChild(ddColorsThree);
            AddChild(ddColorsFour);
            AddChild(btnSave);
            AddChild(btnCancel);

            base.Initialize();
            CenterOnParent();

            LoadUTColors();
            SetUTColors();

            ddColorsOne.SelectedIndexChanged += DdColors_SelectedIndexChanged;
            ddColorsTwo.SelectedIndexChanged += DdColors_SelectedIndexChanged;
            ddColorsThree.SelectedIndexChanged += DdColors_SelectedIndexChanged;
            ddColorsFour.SelectedIndexChanged += DdColors_SelectedIndexChanged;
        }

        private void DdColors_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetUTColors();
        }

        private void LoadUTColors()
        {
            int selcolorone = UserINISettings.Instance.UTColorOne;
            ddColorsOne.SelectedIndex = selcolorone >= ddColorsOne.Items.Count || selcolorone < 0 ? 0 : selcolorone;

            int selcolortwo = UserINISettings.Instance.UTColorTwo;
            ddColorsTwo.SelectedIndex = selcolortwo >= ddColorsTwo.Items.Count || selcolortwo < 0 ? 0 : selcolortwo;

            int selcolorthree = UserINISettings.Instance.UTColorThree;
            ddColorsThree.SelectedIndex = selcolorthree >= ddColorsThree.Items.Count || selcolorthree < 0 ? 0 : selcolorthree;

            int selcolorfour = UserINISettings.Instance.UTColorFour;
            ddColorsFour.SelectedIndex = selcolorfour >= ddColorsFour.Items.Count || selcolorfour < 0 ? 0 : selcolorfour;
        }

        private void SetUTColors()
        {
            UserINISettings.Instance.UTColorOne.Value = ddColorsOne.SelectedIndex;
            UserINISettings.Instance.UTColorTwo.Value = ddColorsTwo.SelectedIndex;
            UserINISettings.Instance.UTColorThree.Value = ddColorsThree.SelectedIndex;
            UserINISettings.Instance.UTColorFour.Value = ddColorsFour.SelectedIndex;

            if (ddColorsOne.SelectedItem?.Tag is UnifiedTechnoColor color1)
            {
                string rgbvalue1 = $"{color1.XnaColor.R},{color1.XnaColor.G},{color1.XnaColor.B}";
                UTColorsINI.SetStringValue("AudioVisual", "UnifiedRadarColor.Self", rgbvalue1);
                UTColorsINI.SetStringValue("AudioVisual", "UnifiedTechnoColor.Self", (string)color1.Name);
            }

            if (ddColorsTwo.SelectedItem?.Tag is UnifiedTechnoColor color2)
            {
                string rgbvalue2 = $"{color2.XnaColor.R},{color2.XnaColor.G},{color2.XnaColor.B}";
                UTColorsINI.SetStringValue("AudioVisual", "UnifiedRadarColor.Ally", rgbvalue2);
                UTColorsINI.SetStringValue("AudioVisual", "UnifiedTechnoColor.Ally", (string)color2.Name);
            }

            if (ddColorsThree.SelectedItem?.Tag is UnifiedTechnoColor color3)
            {
                string rgbvalue3 = $"{color3.XnaColor.R},{color3.XnaColor.G},{color3.XnaColor.B}";
                UTColorsINI.SetStringValue("AudioVisual", "UnifiedRadarColor.Enemy", rgbvalue3);
                UTColorsINI.SetStringValue("AudioVisual", "UnifiedTechnoColor.Enemy", (string)color3.Name);
            }

            if (ddColorsFour.SelectedItem?.Tag is UnifiedTechnoColor color4)
            {
                string rgbvalue4 = $"{color4.XnaColor.R},{color4.XnaColor.G},{color4.XnaColor.B}";
                UTColorsINI.SetStringValue("AudioVisual", "UnifiedRadarColor.Neutral", rgbvalue4);
                UTColorsINI.SetStringValue("AudioVisual", "UnifiedTechnoColor.Neutral", (string)color4.Name);
            }
        }

        private void BtnSave_LeftClick(object sender, EventArgs e)
        {
            UserINISettings.Instance.SaveSettings();
            UTColorsINI.WriteIniFile();
            Disable();
        }

        private void BtnResetAllColors_LeftClick(object sender, EventArgs e)
        {
            ddColorsOne.SelectedIndex = 13;
            ddColorsTwo.SelectedIndex = 11;
            ddColorsThree.SelectedIndex = 7;
            ddColorsFour.SelectedIndex = 15;
            SetUTColors();
        }

        private void BtnCancel_LeftClick(object sender, EventArgs e)
        {
            Disable();
        }
    }
}
