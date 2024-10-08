﻿using System;
using System.Threading.Tasks;
using ClientCore;
using ClientCore.CnCNet5;
using ClientGUI;
using ClientUpdater;
using DTAClient.Domain.Multiplayer;
using DTAClient.DXGUI.Multiplayer;
using DTAClient.DXGUI.Multiplayer.CnCNet;
using DTAClient.DXGUI.Multiplayer.GameLobby;
using DTAClient.Online;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Rampastring.Tools;
using Rampastring.XNAUI;

namespace DTAClient.DXGUI.Generic
{
    public class LoadingScreen : XNAWindow
    {
        public LoadingScreen(
            CnCNetManager cncnetManager,
            WindowManager windowManager,
            IServiceProvider serviceProvider,
            MapLoader mapLoader
        ) : base(windowManager)
        {
            this.cncnetManager = cncnetManager;
            this.serviceProvider = serviceProvider;
            this.mapLoader = mapLoader;
        }

        private static readonly object locker = new object();

        private MapLoader mapLoader;

        private PrivateMessagingPanel privateMessagingPanel;

        private bool visibleSpriteCursor;

        private Task updaterInitTask;
        private Task mapLoadTask;
        private readonly CnCNetManager cncnetManager;
        private readonly IServiceProvider serviceProvider;

        private string GetBackgroundName(int id)
        {
            if (id == 0)
                return "LoadingScreens/loadingscreen.png";
            else
                return "LoadingScreens/loadingscreen" + id.ToString(System.Globalization.CultureInfo.InvariantCulture) + ".png";
        }

        public override void Initialize()
        {
            ClientRectangle = new Rectangle(0, 0, 800, 600);
            Name = "LoadingScreen";

            string backgroundName = GetBackgroundName(0);

            int backgroundMaximum = 0;
            for (int i = 1; ; i++)
            {
                string currentName = GetBackgroundName(i);
                if (!AssetLoader.AssetExists(currentName))
                {
                    backgroundMaximum = i - 1;
                    break;
                }
            }
            if (backgroundMaximum > 0)
            {
                System.Random rand = new System.Random();
                int roll = rand.Next(backgroundMaximum + 1);
                backgroundName = GetBackgroundName(roll);
            }

            BackgroundTexture = AssetLoader.LoadTexture(backgroundName);

            base.Initialize();

            CenterOnParent();

            bool initUpdater = !ClientConfiguration.Instance.ModMode;

            if (initUpdater)
            {
                updaterInitTask = new Task(InitUpdater);
                updaterInitTask.Start();
            }

            mapLoadTask = mapLoader.LoadMapsAsync();

            if (Cursor.Visible)
            {
                Cursor.Visible = false;
                visibleSpriteCursor = true;
            }
        }

        private void InitUpdater()
        {
            Updater.OnLocalFileVersionsChecked += LogGameClientVersion;
            Updater.CheckLocalFileVersions();
        }

        private void LogGameClientVersion()
        {
            Logger.Log($"Game Client Version: {ClientConfiguration.Instance.LocalGame} {Updater.GameVersion}");
            Updater.OnLocalFileVersionsChecked -= LogGameClientVersion;
        }

        private void Finish()
        {
            ProgramConstants.GAME_VERSION = ClientConfiguration.Instance.ModMode ? 
                "N/A" : Updater.GameVersion;

            MainMenu mainMenu = serviceProvider.GetService<MainMenu>();

            WindowManager.AddAndInitializeControl(mainMenu);
            mainMenu.PostInit();

            if (UserINISettings.Instance.AutomaticCnCNetLogin &&
                NameValidator.IsNameValid(ProgramConstants.PLAYERNAME) == null)
            {
                cncnetManager.Connect();
            }

            if (!UserINISettings.Instance.PrivacyPolicyAccepted)
            {
                WindowManager.AddAndInitializeControl(new PrivacyNotification(WindowManager));
            }

            WindowManager.RemoveControl(this);

            Cursor.Visible = visibleSpriteCursor;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (updaterInitTask == null || updaterInitTask.Status == TaskStatus.RanToCompletion)
            {
                if (mapLoadTask.Status == TaskStatus.RanToCompletion)
                    Finish();
            }
        }
    }
}
