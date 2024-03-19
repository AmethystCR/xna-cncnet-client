using System;
using ClientGUI;
using Microsoft.Xna.Framework;
using Rampastring.XNAUI;
using Rampastring.XNAUI.XNAControls;
using ClientCore.Extensions;

namespace DTAClient.DXGUI.Generic
{
	public class CreditsWindow : XNAWindow
	{
		public CreditsWindow(WindowManager windowManager) : base(windowManager)
		{
		}

		public override void Initialize()
		{
			Name = "CreditsWindow";
			ClientRectangle = new Rectangle(0, 0, 400, 300);
			BackgroundTexture = AssetLoader.LoadTexture("CreditsWindow.png");
			WindowManager.CenterControlOnScreen(this);
			
			var lblMember1 = new XNALabel(WindowManager);
			lblMember1.Name = "lblMember1";
			lblMember1.ClientRectangle = new Rectangle(12, 14, 0, 0);
			lblMember1.Text = "Member1".L10N("Client:Main:Member1");
			
			var lblMember2 = new XNALabel(WindowManager);
			lblMember2.Name = "lblMember2";
			lblMember2.ClientRectangle = new Rectangle(0, lblMember1.Y + 20, 0, 0);
			lblMember2.Text = "Member2".L10N("Client:Main:Member2");
			
			var lblMember3 = new XNALabel(WindowManager);
			lblMember3.Name = "lblMember3";
			lblMember3.ClientRectangle = new Rectangle(0, lblMember2.Y + 20, 0, 0);
			lblMember3.Text = "Member3".L10N("Client:Main:Member3");
			
			var lblMember4 = new XNALabel(WindowManager);
			lblMember4.Name = "lblMember4";
			lblMember4.ClientRectangle = new Rectangle(0, lblMember3.Y + 20, 0, 0);
			lblMember4.Text = "Member4".L10N("Client:Main:Member4");
			
			var lblMember5 = new XNALabel(WindowManager);
			lblMember5.Name = "lblMember5";
			lblMember5.ClientRectangle = new Rectangle(0, lblMember4.Y + 20, 0, 0);
			lblMember5.Text = "Member5".L10N("Client:Main:Member5");
			
			var lblMember6 = new XNALabel(WindowManager);
			lblMember6.Name = "lblMember6";
			lblMember6.ClientRectangle = new Rectangle(0, lblMember5.Y + 20, 0, 0);
			lblMember6.Text = "Member6".L10N("Client:Main:Member6");
			
			var lblMember7 = new XNALabel(WindowManager);
			lblMember7.Name = "lblMember7";
			lblMember7.ClientRectangle = new Rectangle(0, lblMember6.Y + 20, 0, 0);
			lblMember7.Text = "Member7".L10N("Client:Main:Member7");
			
			var lblMember8 = new XNALabel(WindowManager);
			lblMember8.Name = "lblMember8";
			lblMember8.ClientRectangle = new Rectangle(0, lblMember7.Y + 20, 0, 0);
			lblMember8.Text = "Member8".L10N("Client:Main:Member8");
			
			var lblMember9 = new XNALabel(WindowManager);
			lblMember9.Name = "lblMember9";
			lblMember9.ClientRectangle = new Rectangle(0, lblMember8.Y + 20, 0, 0);
			lblMember9.Text = "Member9".L10N("Client:Main:Member9");
			
			var lblMember10 = new XNALabel(WindowManager);
			lblMember10.Name = "lblMember10";
			lblMember10.ClientRectangle = new Rectangle(0, lblMember9.Y + 20, 0, 0);
			lblMember10.Text = "Member10".L10N("Client:Main:Member10");
			
			var lblMember11 = new XNALabel(WindowManager);
			lblMember11.Name = "lblMember11";
			lblMember11.ClientRectangle = new Rectangle(0, lblMember10.Y + 20, 0, 0);
			lblMember11.Text = "Member11".L10N("Client:Main:Member11");
			
			var lblMember12 = new XNALabel(WindowManager);
			lblMember12.Name = "lblMember12";
			lblMember12.ClientRectangle = new Rectangle(0, lblMember11.Y + 20, 0, 0);
			lblMember12.Text = "Member12".L10N("Client:Main:Member12");
			
			var lblVersion = new XNALabel(WindowManager);
			lblVersion.Name = "lblVersion";
			lblVersion.ClientRectangle = new Rectangle(0, lblMember12.Y + 20, 0, 0);
			lblVersion.Text = "Version".L10N("Client:Main:ExVersion");
			
			var btnExCancel = new XNAClientButton(WindowManager);
			btnExCancel.Name = "btnExCancel";
			btnExCancel.ClientRectangle = new Rectangle(0, lblVersion.Y + 20, 133, 23);
			btnExCancel.Text = "ExCancel".L10N("Client:Main:ExCancel");
			btnExCancel.LeftClick += BtnExCancel_LeftClick;		
 
			AddChild(lblMember1);
			AddChild(lblMember2);
			AddChild(lblMember3);
			AddChild(lblMember4);
			AddChild(lblMember5);
			AddChild(lblMember6);
			AddChild(lblMember7);
			AddChild(lblMember8);
			AddChild(lblMember9);
			AddChild(lblMember10);
			AddChild(lblMember11);
			AddChild(lblMember12);
			AddChild(lblVersion);
			AddChild(btnExCancel);
			base.Initialize();
			CenterOnParent();
		}

		private void BtnExCancel_LeftClick(object sender, EventArgs e)
		{
			Enabled = false;
		}
	}
}