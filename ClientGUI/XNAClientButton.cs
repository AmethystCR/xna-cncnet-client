using Rampastring.XNAUI.XNAControls;
using Rampastring.XNAUI;
using Rampastring.Tools;
using System;
using ClientCore;
using ClientCore.Extensions;
using Microsoft.Xna.Framework;

namespace ClientGUI
{
    public class XNAClientButton : XNAButton, IToolTipContainer
    {
        public Point LocationCN;
        public Point SizeCN;
        public Point LocationEN;
        public Point SizeEN;

        public ToolTip ToolTip { get; private set; }

        private string _initialToolTipText;
        public string ToolTipText
        {
            get => Initialized ? ToolTip?.Text : _initialToolTipText;
            set
            {
                if (Initialized)
                    ToolTip.Text = value;
                else
                    _initialToolTipText = value;
            }
        }

        public XNAClientButton(WindowManager windowManager) : base(windowManager)
        {
            FontIndex = 1;
            Height = UIDesignConstants.BUTTON_HEIGHT;
        }

        public override void Initialize()
        {
            int width = Width;

            if (IdleTexture == null)
                IdleTexture = AssetLoader.LoadTexture(width + "pxbtn.png");

            if (HoverTexture == null)
                HoverTexture = AssetLoader.LoadTexture(width + "pxbtn_c.png");

            if (HoverSoundEffect == null)
                HoverSoundEffect = new EnhancedSoundEffect("button.wav");

            base.Initialize();

            if (Width == 0)
                Width = IdleTexture.Width;

            ToolTip = new ToolTip(WindowManager, this) { Text = _initialToolTipText };
        }

        protected override void ParseControlINIAttribute(IniFile iniFile, string key, string value)
        {
            if (key == "MatchTextureSize" && Conversions.BooleanFromString(value, false))
            {
                Width = IdleTexture.Width;
                Height = IdleTexture.Height;
                return;
            }
            else if (key == "ToolTip")
            {
                ToolTipText = value.FromIniString();
                return;
            }
            else if (key == "LocationEN")
            {
                string[] strPoint = value.Split(',');
                LocationEN.X = Conversions.IntFromString(strPoint[0], 0);
                LocationEN.Y = Conversions.IntFromString(strPoint[1], 0);
                return;
            }
            else if (key == "SizeEN")
            {
                string[] strPoint = value.Split(',');
                SizeEN.X = Conversions.IntFromString(strPoint[0], 0);
                SizeEN.Y = Conversions.IntFromString(strPoint[1], 0);
                return;
            }
            else if (key == "LocationCN")
            {
                string[] strPoint = value.Split(',');
                LocationCN.X = Conversions.IntFromString(strPoint[0], 0);
                LocationCN.Y = Conversions.IntFromString(strPoint[1], 0);
                return;
            }
            else if (key == "SizeCN")
            {
                string[] strPoint = value.Split(',');
                SizeCN.X = Conversions.IntFromString(strPoint[0], 0);
                SizeCN.Y = Conversions.IntFromString(strPoint[1], 0);
                return;
            }

            base.ParseControlINIAttribute(iniFile, key, value);
        }
    }
}
