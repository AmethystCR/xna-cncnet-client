using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using Rampastring.Tools;
using Rampastring.XNAUI;
using Rampastring.XNAUI.XNAControls;

namespace ClientGUI
{
	public class XNADynamicLabel : XNALabel
	{
		public XNADynamicLabel(WindowManager windowManager) : base(windowManager)
		{
		}

		public override void Initialize()
		{
			base.Initialize();
		}

		public int Interval { get; set; } = 1000;

		public List<string> Texts { get; set; } = new List<string>();

		protected override void ParseControlINIAttribute(IniFile iniFile, string key, string value)
		{
			if (key == "TextList")
			{
				this.Texts = value.Split(new char[]
				{
					'%'
				}).ToList<string>();
				this.Text = this.GetRandomText();
				this.Text = this.Text.Replace("@", Environment.NewLine);
				return;
			}
			if (!(key == "Interval"))
			{
				base.ParseControlINIAttribute(iniFile, key, value);
				return;
			}
			this.Interval = Convert.ToInt32(value);
		}

		private string GetRandomText()
		{
			if (this.Texts == null)
			{
				return "";
			}
			if (this.Texts.Count<string>() <= 0)
			{
				return "";
			}
			return this.Texts[this.random.Next(0, this.Texts.Count<string>())];
		}

		public override void Update(GameTime gameTime)
		{
			if (this.sw.ElapsedMilliseconds >= (long)this.Interval)
			{
				this.sw.Restart();
				this.Text = this.GetRandomText();
				this.Text = this.Text.Replace("@", Environment.NewLine);
			}
			base.Update(gameTime);
		}

		public override void Draw(GameTime gameTime)
		{
			base.Draw(gameTime);
		}

		private Stopwatch sw = Stopwatch.StartNew();

		private Random random = new Random();
	}
}
