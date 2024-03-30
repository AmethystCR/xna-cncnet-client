using ClientCore;
using ClientCore.Statistics;
using ClientGUI;
using DTAClient.Domain.Multiplayer;
using ClientCore.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Rampastring.Tools;
using Rampastring.XNAUI;
using Rampastring.XNAUI.XNAControls;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DTAClient.DXGUI.Generic
{
    public class StatisticsWindow : XNAWindow
    {
        public StatisticsWindow(WindowManager windowManager, MapLoader mapLoader)
            : base(windowManager)
        {
            this.mapLoader = mapLoader;
        }

        private XNAPanel panelGameStatistics;
        private XNAPanel panelTotalStatistics;
        private XNAPanel panelAchStatistics;

        private XNAClientDropDown cmbGameModeFilter;
        private XNAClientDropDown cmbGameClassFilter;

        private XNAClientCheckBox chkIncludeSpectatedGames;

        private XNAClientTabControl tabControl;

        // Controls for game statistics

        private XNAMultiColumnListBox lbGameList;
        private XNAMultiColumnListBox lbGameStatistics;

        private Texture2D[] sideTextures;

        // *****************************

        private const int TOTAL_STATS_LOCATION_X1 = 40;
        private const int TOTAL_STATS_VALUE_LOCATION_X1 = 240;
        private const int TOTAL_STATS_LOCATION_X2 = 400;
        private const int TOTAL_STATS_VALUE_LOCATION_X2 = 600;
        private const int TOTAL_STATS_Y_INCREASE = 45;
        private const int TOTAL_STATS_FIRST_ITEM_Y = 20;

        private const string Success = "100%";

        // Controls for total statistics

        private XNALabel lblGamesStartedValue;
        private XNALabel lblGamesFinishedValue;
        private XNALabel lblWinsValue;
        private XNALabel lblLossesValue;
        private XNALabel lblWinLossRatioValue;
        private XNALabel lblAverageGameLengthValue;
        private XNALabel lblTotalTimePlayedValue;
        private XNALabel lblAverageEnemyCountValue;
        private XNALabel lblAverageAllyCountValue;
        private XNALabel lblTotalKillsValue;
        private XNALabel lblKillsPerGameValue;
        private XNALabel lblTotalLossesValue;
        private XNALabel lblLossesPerGameValue;
        private XNALabel lblKillLossRatioValue;
        private XNALabel lblTotalScoreValue;
        private XNALabel lblAverageEconomyValue;
        private XNALabel lblFavouriteSideValue;
        private XNALabel lblAverageAILevelValue;

        // *****************************

        private Double[,] Value;

        private XNAProgressBar PrghardenedValue;
        private XNAProgressBar PrgkillValue;
        private XNAProgressBar PrgVictorValue;
        private XNAProgressBar PrgSoldierValue;
        private XNAProgressBar PrgLongValue;
        private XNAProgressBar PrgShortValue;
        private XNAProgressBar PrgNavalValue;
        private XNAProgressBar PrgGermanyValue;
        private XNAProgressBar PrgOneValue;
        private XNAProgressBar PrgBulletsValue;
        private XNAProgressBar PrgFkyValue;
        private XNAProgressBar PrgMaxValue;
        private XNAProgressBar PrgMinValue;
        private XNAProgressBar PrgMaginotValue;
        private XNAProgressBar PrgBtValue;

        //*******************************

        private StatisticsManager sm;
        private MapLoader mapLoader;
        private List<int> listedGameIndexes = new List<int>();

        private (string Name, string UIName)[] sides;

        private List<MultiplayerColor> mpColors;

        protected IniFile StatisticsWindowIni { get; private set; }

        public override void Initialize()
        {
            sm = StatisticsManager.Instance;

            string strLblEconomy = "ECONOMY".L10N("Client:Main:StatisticEconomy");
            string strLblAvgEconomy = "Average economy:".L10N("Client:Main:StatisticEconomyAvg");
            if (ClientConfiguration.Instance.UseBuiltStatistic)
            {
                strLblEconomy = "BUILT".L10N("Client:Main:StatisticBuildCount");
                strLblAvgEconomy = "Avg. number of objects built:".L10N("Client:Main:StatisticBuildCountAvg");
            }

            StatisticsWindowIni = new IniFile(SafePath.CombineFilePath(ProgramConstants.GetResourcePath(), "StatisticsWindow.ini"));
            int totalstatslocationx1 = StatisticsWindowIni.GetIntValue("StatisticsWindow", "TotalStatsLocationX1", TOTAL_STATS_LOCATION_X1);
            int totalstatsvaluelocationx1 = StatisticsWindowIni.GetIntValue("StatisticsWindow", "TotalStatsValueLocationX1", TOTAL_STATS_VALUE_LOCATION_X1);
            int totalstatslocationx2 = StatisticsWindowIni.GetIntValue("StatisticsWindow", "TotalStatsLocationX2", TOTAL_STATS_LOCATION_X2);
            int totalstatsvaluelocationx2 = StatisticsWindowIni.GetIntValue("StatisticsWindow", "TotalStatsValueLocationX2", TOTAL_STATS_VALUE_LOCATION_X2);
            int totalstatsyincrease = StatisticsWindowIni.GetIntValue("StatisticsWindow", "TotalStatsYIncrease", TOTAL_STATS_Y_INCREASE);
            int totalstatsfirstitemy = StatisticsWindowIni.GetIntValue("StatisticsWindow", "TotalStatsFirstItemY", TOTAL_STATS_FIRST_ITEM_Y);

            Name = "StatisticsWindow";
            BackgroundTexture = AssetLoader.LoadTexture("scoreviewerbg.png");
            ClientRectangle = new Rectangle(0, 0, 700, 521);

            tabControl = new XNAClientTabControl(WindowManager);
            tabControl.Name = "tabControl";
            tabControl.ClientRectangle = new Rectangle(12, 10, 0, 0);
            tabControl.ClickSound = new EnhancedSoundEffect("button.wav");
            tabControl.FontIndex = 1;
            tabControl.AddTab("Game Statistics".L10N("Client:Main:GameStatistic"), UIDesignConstants.BUTTON_WIDTH_133);
            tabControl.AddTab("Total Statistics".L10N("Client:Main:TotalStatistic"), UIDesignConstants.BUTTON_WIDTH_133);
            tabControl.AddTab("Achievement".L10N("Client:Main:Achievement"), UIDesignConstants.BUTTON_WIDTH_133);
            tabControl.SelectedIndexChanged += TabControl_SelectedIndexChanged;

            XNALabel lblFilter = new XNALabel(WindowManager);
            lblFilter.Name = "lblFilter";
            lblFilter.FontIndex = 1;
            lblFilter.Text = "FILTER:".L10N("Client:Main:Filter");
            lblFilter.ClientRectangle = new Rectangle(527, 12, 0, 0);

            cmbGameClassFilter = new XNAClientDropDown(WindowManager);
            cmbGameClassFilter.ClientRectangle = new Rectangle(585, 11, 105, 21);
            cmbGameClassFilter.Name = "cmbGameClassFilter";
            cmbGameClassFilter.AddItem("All games".L10N("Client:Main:FilterAll"));
            cmbGameClassFilter.AddItem("Online games".L10N("Client:Main:FilterOnline"));
            cmbGameClassFilter.AddItem("Online PvP".L10N("Client:Main:FilterPvP"));
            cmbGameClassFilter.AddItem("Online Co-Op".L10N("Client:Main:FilterCoOp"));
            cmbGameClassFilter.AddItem("Skirmish".L10N("Client:Main:FilterSkirmish"));
            cmbGameClassFilter.SelectedIndex = 0;
            cmbGameClassFilter.SelectedIndexChanged += CmbGameClassFilter_SelectedIndexChanged;

            XNALabel lblGameMode = new XNALabel(WindowManager);
            lblGameMode.Name = nameof(lblGameMode);
            lblGameMode.FontIndex = 1;
            lblGameMode.Text = "GAME MODE:".L10N("Client:Main:GameMode");
            lblGameMode.ClientRectangle = new Rectangle(294, 12, 0, 0);

            cmbGameModeFilter = new XNAClientDropDown(WindowManager);
            cmbGameModeFilter.Name = nameof(cmbGameModeFilter);
            cmbGameModeFilter.ClientRectangle = new Rectangle(381, 11, 114, 21);
            cmbGameModeFilter.SelectedIndexChanged += CmbGameModeFilter_SelectedIndexChanged;

            var btnReturnToMenu = new XNAClientButton(WindowManager);
            btnReturnToMenu.Name = nameof(btnReturnToMenu);
            btnReturnToMenu.ClientRectangle = new Rectangle(270, 486, UIDesignConstants.BUTTON_WIDTH_160, UIDesignConstants.BUTTON_HEIGHT);
            btnReturnToMenu.Text = "Return to Main Menu".L10N("Client:Main:ReturnToMainMenu");
            btnReturnToMenu.LeftClick += BtnReturnToMenu_LeftClick;

            var btnClearStatistics = new XNAClientButton(WindowManager);
            btnClearStatistics.Name = nameof(btnClearStatistics);
            btnClearStatistics.ClientRectangle = new Rectangle(12, 486, UIDesignConstants.BUTTON_WIDTH_160, UIDesignConstants.BUTTON_HEIGHT);
            btnClearStatistics.Text = "Clear Statistics".L10N("Client:Main:ClearStatistics");
            btnClearStatistics.LeftClick += BtnClearStatistics_LeftClick;
            btnClearStatistics.Visible = false;

            chkIncludeSpectatedGames = new XNAClientCheckBox(WindowManager);

            AddChild(chkIncludeSpectatedGames);
            chkIncludeSpectatedGames.Name = nameof(chkIncludeSpectatedGames);
            chkIncludeSpectatedGames.Text = "Include spectated games".L10N("Client:Main:IncludeSpectated");
            chkIncludeSpectatedGames.Checked = true;
            chkIncludeSpectatedGames.ClientRectangle = new Rectangle(
                Width - chkIncludeSpectatedGames.Width - 12,
                cmbGameModeFilter.Bottom + 3,
                chkIncludeSpectatedGames.Width, 
                chkIncludeSpectatedGames.Height);
            chkIncludeSpectatedGames.CheckedChanged += ChkIncludeSpectatedGames_CheckedChanged;

            #region Match statistics

            panelGameStatistics = new XNAPanel(WindowManager);
            panelGameStatistics.Name = "panelGameStatistics";
            panelGameStatistics.BackgroundTexture = AssetLoader.LoadTexture("scoreviewerpanelbg.png");
            panelGameStatistics.ClientRectangle = new Rectangle(10, 55, 680, 425);

            AddChild(panelGameStatistics);

            XNALabel lblGames = new XNALabel(WindowManager);
            lblGames.Name = nameof(lblGames);
            lblGames.Text = "GAMES:".L10N("Client:Main:GameMatches");
            lblGames.FontIndex = 1;
            lblGames.ClientRectangle = new Rectangle(4, 2, 0, 0);

            lbGameList = new XNAMultiColumnListBox(WindowManager);
            lbGameList.Name = nameof(lbGameList);
            lbGameList.ClientRectangle = new Rectangle(2, 25, 676, 250);
            lbGameList.BackgroundTexture = AssetLoader.CreateTexture(new Color(0, 0, 0, 128), 1, 1);
            lbGameList.PanelBackgroundDrawMode = PanelBackgroundImageDrawMode.STRETCHED;
            lbGameList.AddColumn("DATE / TIME".L10N("Client:Main:GameMatchDateTimeColumnHeader"), 130);
            lbGameList.AddColumn("MAP".L10N("Client:Main:GameMatchMapColumnHeader"), 200);
            lbGameList.AddColumn("GAME MODE".L10N("Client:Main:GameMatchGameModeColumnHeader"), 130);
            lbGameList.AddColumn("FPS".L10N("Client:Main:GameMatchFPSColumnHeader"), 50);
            lbGameList.AddColumn("DURATION".L10N("Client:Main:GameMatchDurationColumnHeader"), 76);
            lbGameList.AddColumn("COMPLETED".L10N("Client:Main:GameMatchCompletedColumnHeader"), 90);
            lbGameList.SelectedIndexChanged += LbGameList_SelectedIndexChanged;
            lbGameList.AllowKeyboardInput = true;

            lbGameStatistics = new XNAMultiColumnListBox(WindowManager);
            lbGameStatistics.Name = nameof(lbGameStatistics);
            lbGameStatistics.ClientRectangle = new Rectangle(2, 280, 676, 143);
            lbGameStatistics.BackgroundTexture = AssetLoader.CreateTexture(new Color(0, 0, 0, 128), 1, 1);
            lbGameStatistics.PanelBackgroundDrawMode = PanelBackgroundImageDrawMode.STRETCHED;
            lbGameStatistics.AddColumn("NAME".L10N("Client:Main:StatisticsName"), 130);
            lbGameStatistics.AddColumn("KILLS".L10N("Client:Main:StatisticsKills"), 78);
            lbGameStatistics.AddColumn("LOSSES".L10N("Client:Main:StatisticsLosses"), 78);
            lbGameStatistics.AddColumn(strLblEconomy, 80);
            lbGameStatistics.AddColumn("SCORE".L10N("Client:Main:StatisticsScore"), 100);
            lbGameStatistics.AddColumn("WON".L10N("Client:Main:StatisticsWon"), 50);
            lbGameStatistics.AddColumn("SIDE".L10N("Client:Main:StatisticsSide"), 100);
            lbGameStatistics.AddColumn("TEAM".L10N("Client:Main:StatisticsTeam"), 60);

            panelGameStatistics.AddChild(lblGames);
            panelGameStatistics.AddChild(lbGameList);
            panelGameStatistics.AddChild(lbGameStatistics);

#endregion

#region Total statistics

            panelTotalStatistics = new XNAPanel(WindowManager);
            panelTotalStatistics.Name = "panelTotalStatistics";
            panelTotalStatistics.BackgroundTexture = AssetLoader.LoadTexture("scoreviewerpanelbg.png");
            panelTotalStatistics.ClientRectangle = new Rectangle(10, 55, 680, 425);

            AddChild(panelTotalStatistics);
            panelTotalStatistics.Visible = false;
            panelTotalStatistics.Enabled = false;

            int locationY = totalstatsfirstitemy;

            AddTotalStatisticsLabel("lblGamesStarted", "Games started:".L10N("Client:Main:StatisticsGamesStarted"), new Point(totalstatslocationx1, locationY));

            lblGamesStartedValue = new XNALabel(WindowManager);
            lblGamesStartedValue.Name = "lblGamesStartedValue";
            lblGamesStartedValue.ClientRectangle = new Rectangle(totalstatsvaluelocationx1, locationY, 0, 0);
            lblGamesStartedValue.RemapColor = UISettings.ActiveSettings.AltColor;
            locationY += totalstatsyincrease;

            AddTotalStatisticsLabel("lblGamesFinished", "Games finished:".L10N("Client:Main:StatisticsGamesFinished"), new Point(totalstatslocationx1, locationY));

            lblGamesFinishedValue = new XNALabel(WindowManager);
            lblGamesFinishedValue.Name = "lblGamesFinishedValue";
            lblGamesFinishedValue.ClientRectangle = new Rectangle(totalstatsvaluelocationx1, locationY, 0, 0);
            lblGamesFinishedValue.RemapColor = UISettings.ActiveSettings.AltColor;
            locationY += totalstatsyincrease;

            AddTotalStatisticsLabel("lblWins", "Wins:".L10N("Client:Main:StatisticsGamesWins"), new Point(totalstatslocationx1, locationY));

            lblWinsValue = new XNALabel(WindowManager);
            lblWinsValue.Name = "lblWinsValue";
            lblWinsValue.ClientRectangle = new Rectangle(totalstatsvaluelocationx1, locationY, 0, 0);
            lblWinsValue.RemapColor = UISettings.ActiveSettings.AltColor;
            locationY += totalstatsyincrease;

            AddTotalStatisticsLabel("lblLosses", "Losses:".L10N("Client:Main:StatisticsGamesLosses"), new Point(totalstatslocationx1, locationY));

            lblLossesValue = new XNALabel(WindowManager);
            lblLossesValue.Name = "lblLossesValue";
            lblLossesValue.ClientRectangle = new Rectangle(totalstatsvaluelocationx1, locationY, 0, 0);
            lblLossesValue.RemapColor = UISettings.ActiveSettings.AltColor;
            locationY += totalstatsyincrease;

            AddTotalStatisticsLabel("lblWinLossRatio", "Win / Loss ratio:".L10N("Client:Main:StatisticsGamesWinLossRatio"), new Point(totalstatslocationx1, locationY));

            lblWinLossRatioValue = new XNALabel(WindowManager);
            lblWinLossRatioValue.Name = "lblWinLossRatioValue";
            lblWinLossRatioValue.ClientRectangle = new Rectangle(totalstatsvaluelocationx1, locationY, 0, 0);
            lblWinLossRatioValue.RemapColor = UISettings.ActiveSettings.AltColor;
            locationY += totalstatsyincrease;

            AddTotalStatisticsLabel("lblAverageGameLength", "Average game length:".L10N("Client:Main:StatisticsGamesLengthAvg"), new Point(totalstatslocationx1, locationY));

            lblAverageGameLengthValue = new XNALabel(WindowManager);
            lblAverageGameLengthValue.Name = "lblAverageGameLengthValue";
            lblAverageGameLengthValue.ClientRectangle = new Rectangle(totalstatsvaluelocationx1, locationY, 0, 0);
            lblAverageGameLengthValue.RemapColor = UISettings.ActiveSettings.AltColor;
            locationY += totalstatsyincrease;

            AddTotalStatisticsLabel("lblTotalTimePlayed", "Total time played:".L10N("Client:Main:StatisticsTotalTimePlayed"), new Point(totalstatslocationx1, locationY));

            lblTotalTimePlayedValue = new XNALabel(WindowManager);
            lblTotalTimePlayedValue.Name = "lblTotalTimePlayedValue";
            lblTotalTimePlayedValue.ClientRectangle = new Rectangle(totalstatsvaluelocationx1, locationY, 0, 0);
            lblTotalTimePlayedValue.RemapColor = UISettings.ActiveSettings.AltColor;
            locationY += totalstatsyincrease;

            AddTotalStatisticsLabel("lblAverageEnemyCount", "Average number of enemies:".L10N("Client:Main:StatisticsEnemiesAvg"), new Point(totalstatslocationx1, locationY));

            lblAverageEnemyCountValue = new XNALabel(WindowManager);
            lblAverageEnemyCountValue.Name = "lblAverageEnemyCountValue";
            lblAverageEnemyCountValue.ClientRectangle = new Rectangle(totalstatsvaluelocationx1, locationY, 0, 0);
            lblAverageEnemyCountValue.RemapColor = UISettings.ActiveSettings.AltColor;
            locationY += totalstatsyincrease;

            AddTotalStatisticsLabel("lblAverageAllyCount", "Average number of allies:".L10N("Client:Main:StatisticsAlliesAvg"), new Point(totalstatslocationx1, locationY));

            lblAverageAllyCountValue = new XNALabel(WindowManager);
            lblAverageAllyCountValue.Name = "lblAverageAllyCountValue";
            lblAverageAllyCountValue.ClientRectangle = new Rectangle(totalstatsvaluelocationx1, locationY, 0, 0);
            lblAverageAllyCountValue.RemapColor = UISettings.ActiveSettings.AltColor;
            locationY += totalstatsyincrease;

            // SECOND COLUMN

            locationY = totalstatsfirstitemy;

            AddTotalStatisticsLabel("lblTotalKills", "Total kills:".L10N("Client:Main:StatisticsTotalKills"), new Point(totalstatslocationx2, locationY));

            lblTotalKillsValue = new XNALabel(WindowManager);
            lblTotalKillsValue.Name = "lblTotalKillsValue";
            lblTotalKillsValue.ClientRectangle = new Rectangle(totalstatsvaluelocationx2, locationY, 0, 0);
            lblTotalKillsValue.RemapColor = UISettings.ActiveSettings.AltColor;
            locationY += totalstatsyincrease;

            AddTotalStatisticsLabel("lblKillsPerGame", "Kills / game:".L10N("Client:Main:StatisticsKillsPerGame"), new Point(totalstatslocationx2, locationY));

            lblKillsPerGameValue = new XNALabel(WindowManager);
            lblKillsPerGameValue.Name = "lblKillsPerGameValue";
            lblKillsPerGameValue.ClientRectangle = new Rectangle(totalstatsvaluelocationx2, locationY, 0, 0);
            lblKillsPerGameValue.RemapColor = UISettings.ActiveSettings.AltColor;
            locationY += totalstatsyincrease;

            AddTotalStatisticsLabel("lblTotalLosses", "Total losses:".L10N("Client:Main:StatisticsTotalLosses"), new Point(totalstatslocationx2, locationY));

            lblTotalLossesValue = new XNALabel(WindowManager);
            lblTotalLossesValue.Name = "lblTotalLossesValue";
            lblTotalLossesValue.ClientRectangle = new Rectangle(totalstatsvaluelocationx2, locationY, 0, 0);
            lblTotalLossesValue.RemapColor = UISettings.ActiveSettings.AltColor;
            locationY += totalstatsyincrease;

            AddTotalStatisticsLabel("lblLossesPerGame", "Losses / game:".L10N("Client:Main:StatisticsLossesPerGame"), new Point(totalstatslocationx2, locationY));

            lblLossesPerGameValue = new XNALabel(WindowManager);
            lblLossesPerGameValue.Name = "lblLossesPerGameValue";
            lblLossesPerGameValue.ClientRectangle = new Rectangle(totalstatsvaluelocationx2, locationY, 0, 0);
            lblLossesPerGameValue.RemapColor = UISettings.ActiveSettings.AltColor;
            locationY += totalstatsyincrease;

            AddTotalStatisticsLabel("lblKillLossRatio", "Kill / loss ratio:".L10N("Client:Main:StatisticsKillLossRatio"), new Point(totalstatslocationx2, locationY));

            lblKillLossRatioValue = new XNALabel(WindowManager);
            lblKillLossRatioValue.Name = "lblKillLossRatioValue";
            lblKillLossRatioValue.ClientRectangle = new Rectangle(totalstatsvaluelocationx2, locationY, 0, 0);
            lblKillLossRatioValue.RemapColor = UISettings.ActiveSettings.AltColor;
            locationY += totalstatsyincrease;

            AddTotalStatisticsLabel("lblTotalScore", "Total score:".L10N("Client:Main:TotalScore"), new Point(totalstatslocationx2, locationY));

            lblTotalScoreValue = new XNALabel(WindowManager);
            lblTotalScoreValue.Name = "lblTotalScoreValue";
            lblTotalScoreValue.ClientRectangle = new Rectangle(totalstatsvaluelocationx2, locationY, 0, 0);
            lblTotalScoreValue.RemapColor = UISettings.ActiveSettings.AltColor;
            locationY += totalstatsyincrease;

            AddTotalStatisticsLabel("lblAverageEconomy", strLblAvgEconomy, new Point(totalstatslocationx2, locationY));

            lblAverageEconomyValue = new XNALabel(WindowManager);
            lblAverageEconomyValue.Name = "lblAverageEconomyValue";
            lblAverageEconomyValue.ClientRectangle = new Rectangle(totalstatsvaluelocationx2, locationY, 0, 0);
            lblAverageEconomyValue.RemapColor = UISettings.ActiveSettings.AltColor;
            locationY += totalstatsyincrease;

            AddTotalStatisticsLabel("lblFavouriteSide", "Favourite side:".L10N("Client:Main:FavouriteSide"), new Point(totalstatslocationx2, locationY));

            lblFavouriteSideValue = new XNALabel(WindowManager);
            lblFavouriteSideValue.Name = "lblFavouriteSideValue";
            lblFavouriteSideValue.ClientRectangle = new Rectangle(totalstatsvaluelocationx2, locationY, 0, 0);
            lblFavouriteSideValue.RemapColor = UISettings.ActiveSettings.AltColor;
            locationY += totalstatsyincrease;

            AddTotalStatisticsLabel("lblAverageAILevel", "Average AI level:".L10N("Client:Main:AvgAILevel"), new Point(totalstatslocationx2, locationY));

            lblAverageAILevelValue = new XNALabel(WindowManager);
            lblAverageAILevelValue.Name = "lblAverageAILevelValue";
            lblAverageAILevelValue.ClientRectangle = new Rectangle(totalstatsvaluelocationx2, locationY, 0, 0);
            lblAverageAILevelValue.RemapColor = UISettings.ActiveSettings.AltColor;
            locationY += totalstatsyincrease;

            panelTotalStatistics.AddChild(lblGamesStartedValue);
            panelTotalStatistics.AddChild(lblGamesFinishedValue);
            panelTotalStatistics.AddChild(lblWinsValue);
            panelTotalStatistics.AddChild(lblLossesValue);
            panelTotalStatistics.AddChild(lblWinLossRatioValue);
            panelTotalStatistics.AddChild(lblAverageGameLengthValue);
            panelTotalStatistics.AddChild(lblTotalTimePlayedValue);
            panelTotalStatistics.AddChild(lblAverageEnemyCountValue);
            panelTotalStatistics.AddChild(lblAverageAllyCountValue);

            panelTotalStatistics.AddChild(lblTotalKillsValue);
            panelTotalStatistics.AddChild(lblKillsPerGameValue);
            panelTotalStatistics.AddChild(lblTotalLossesValue);
            panelTotalStatistics.AddChild(lblLossesPerGameValue);
            panelTotalStatistics.AddChild(lblKillLossRatioValue);
            panelTotalStatistics.AddChild(lblTotalScoreValue);
            panelTotalStatistics.AddChild(lblAverageEconomyValue);
            panelTotalStatistics.AddChild(lblFavouriteSideValue);
            panelTotalStatistics.AddChild(lblAverageAILevelValue);

            panelAchStatistics = new XNAPanel(WindowManager);
            panelAchStatistics.Name = "panelAchStatistics";
            panelAchStatistics.BackgroundTexture = AssetLoader.LoadTexture("scoreviewerpanelbg.png");
            panelAchStatistics.ClientRectangle = new Rectangle(10, 55, 680, 425);
            Value = new Double[15, 2];
            AddChild(panelAchStatistics);
            panelAchStatistics.Visible = false;
            panelAchStatistics.Enabled = false;

            locationY = totalstatsfirstitemy;

            Value[0, 1] = 2000;
            PrghardenedValue = new XNAProgressBar(WindowManager);
            PrghardenedValue.Name = "PrghardenedValue";
            PrghardenedValue.Maximum = (int)Value[0, 1];
            PrghardenedValue.ClientRectangle = new Rectangle(totalstatsvaluelocationx1 - 110, locationY - 4, 200, 25);
            locationY += totalstatsyincrease;

            Value[1, 1] = 1000000;
            PrgkillValue = new XNAProgressBar(WindowManager);
            PrgkillValue.Name = "PrgkillValue";
            PrgkillValue.Maximum = (int)Value[1, 1];
            PrgkillValue.ClientRectangle = new Rectangle(totalstatsvaluelocationx1 - 110, locationY - 4, 200, 25);
            PrgkillValue.RemapColor = UISettings.ActiveSettings.AltColor;
            locationY += totalstatsyincrease;

            Value[2, 1] = 10;
            PrgVictorValue = new XNAProgressBar(WindowManager);
            PrgVictorValue.Name = "PrgVictorValue";
            PrgVictorValue.Maximum = (int)Value[2, 1];
            PrgVictorValue.ClientRectangle = new Rectangle(totalstatsvaluelocationx1 - 110, locationY - 4, 200, 25);
            PrgVictorValue.RemapColor = UISettings.ActiveSettings.AltColor;
            locationY += totalstatsyincrease;

            Value[3, 1] = 200;
            PrgLongValue = new XNAProgressBar(WindowManager);
            PrgLongValue.Name = "PrgLongValue";
            PrgLongValue.Maximum = (int)Value[3, 1];
            PrgLongValue.ClientRectangle = new Rectangle(totalstatsvaluelocationx1 - 110, locationY - 4, 200, 25);
            PrgLongValue.RemapColor = UISettings.ActiveSettings.AltColor;
            locationY += totalstatsyincrease;

            Value[4, 1] = 200;
            PrgShortValue = new XNAProgressBar(WindowManager);
            PrgShortValue.Name = "PrgShortValue";
            PrgShortValue.Maximum = (int)Value[4, 1];
            PrgShortValue.ClientRectangle = new Rectangle(totalstatsvaluelocationx1 - 110, locationY - 4, 200, 25);
            PrgShortValue.RemapColor = UISettings.ActiveSettings.AltColor;
            locationY += totalstatsyincrease;

            Value[5, 1] = 100;
            PrgSoldierValue = new XNAProgressBar(WindowManager);
            PrgSoldierValue.Name = "PrgSoldierValue";
            PrgSoldierValue.Maximum = (int)Value[5, 1];
            PrgSoldierValue.ClientRectangle = new Rectangle(totalstatsvaluelocationx1 - 110, locationY - 4, 200, 25);
            PrgSoldierValue.RemapColor = UISettings.ActiveSettings.AltColor;
            locationY += totalstatsyincrease;

            Value[6, 1] = 50;
            PrgNavalValue = new XNAProgressBar(WindowManager);
            PrgNavalValue.Name = "PrgNavalValue";
            PrgNavalValue.Maximum = (int)Value[6, 1];
            PrgNavalValue.ClientRectangle = new Rectangle(totalstatsvaluelocationx1 - 110, locationY - 4, 200, 25);
            PrgNavalValue.RemapColor = UISettings.ActiveSettings.AltColor;
            locationY += totalstatsyincrease;

            Value[7, 1] = 30;
            PrgGermanyValue = new XNAProgressBar(WindowManager);
            PrgGermanyValue.Name = "PrgGermanyValue";
            PrgGermanyValue.Maximum = (int)Value[7, 1];
            PrgGermanyValue.ClientRectangle = new Rectangle(totalstatsvaluelocationx1 - 110, locationY - 4, 200, 25);
            PrgGermanyValue.RemapColor = UISettings.ActiveSettings.AltColor;
            locationY += totalstatsyincrease;

            Value[8, 1] = 200;
            PrgOneValue = new XNAProgressBar(WindowManager);
            PrgOneValue.Name = " PrgOneValue";
            PrgOneValue.Maximum = (int)Value[8, 1];
            PrgOneValue.ClientRectangle = new Rectangle(totalstatsvaluelocationx1 - 110, locationY - 4, 200, 25);
            PrgOneValue.RemapColor = UISettings.ActiveSettings.AltColor;
            locationY += totalstatsyincrease;

            locationY = totalstatsfirstitemy;

            Value[9, 1] = 200;
            PrgBulletsValue = new XNAProgressBar(WindowManager);
            PrgBulletsValue.Name = "PrgBulletsValue";
            PrgBulletsValue.Maximum = (int)Value[9, 1];
            PrgBulletsValue.ClientRectangle = new Rectangle(totalstatsvaluelocationx2 - 110, locationY - 4, 200, 25);
            PrgBulletsValue.RemapColor = UISettings.ActiveSettings.AltColor;
            locationY += totalstatsyincrease;

            Value[10, 1] = 20;
            PrgFkyValue = new XNAProgressBar(WindowManager);
            PrgFkyValue.Name = "PrgFkyValue";
            PrgFkyValue.Maximum = (int)Value[10, 1];
            PrgFkyValue.ClientRectangle = new Rectangle(totalstatsvaluelocationx2 - 110, locationY - 4, 200, 25);
            PrgFkyValue.RemapColor = UISettings.ActiveSettings.AltColor;
            locationY += totalstatsyincrease;

            Value[11, 1] = 100;
            PrgMaxValue = new XNAProgressBar(WindowManager);
            PrgMaxValue.Name = "PrgMaxValue";
            PrgMaxValue.Maximum = (int)Value[11, 1];
            PrgMaxValue.ClientRectangle = new Rectangle(totalstatsvaluelocationx2 - 110, locationY - 4, 200, 25);
            PrgMaxValue.RemapColor = UISettings.ActiveSettings.AltColor;
            locationY += totalstatsyincrease;

            Value[12, 1] = 100;
            PrgMinValue = new XNAProgressBar(WindowManager);
            PrgMinValue.Name = "PrgMinValue";
            PrgMinValue.Maximum = (int)Value[12, 1];
            PrgMinValue.ClientRectangle = new Rectangle(totalstatsvaluelocationx2 - 110, locationY - 4, 200, 25);
            PrgMinValue.RemapColor = UISettings.ActiveSettings.AltColor;
            locationY += totalstatsyincrease;

            Value[13, 1] = 20;
            PrgMaginotValue = new XNAProgressBar(WindowManager);
            PrgMaginotValue.Name = "PrgMaginotValue";
            PrgMaginotValue.Maximum = (int)Value[13, 1];
            PrgMaginotValue.ClientRectangle = new Rectangle(totalstatsvaluelocationx2 - 110, locationY - 4, 200, 25);
            PrgMaginotValue.RemapColor = UISettings.ActiveSettings.AltColor;
            locationY += totalstatsyincrease;

            Value[14, 1] = 99;
            PrgBtValue = new XNAProgressBar(WindowManager);
            PrgBtValue.Name = "PrgBtValue";
            PrgBtValue.Maximum = (int)Value[14, 1];
            PrgBtValue.ClientRectangle = new Rectangle(totalstatsvaluelocationx2 - 110, locationY - 4, 200, 25);
            PrgBtValue.RemapColor = UISettings.ActiveSettings.AltColor;
            locationY += totalstatsyincrease;

            panelAchStatistics.AddChild(PrghardenedValue);
            panelAchStatistics.AddChild(PrgkillValue);
            panelAchStatistics.AddChild(PrgVictorValue);
            panelAchStatistics.AddChild(PrgSoldierValue);
            panelAchStatistics.AddChild(PrgLongValue);
            panelAchStatistics.AddChild(PrgShortValue);
            panelAchStatistics.AddChild(PrgNavalValue);
            panelAchStatistics.AddChild(PrgGermanyValue);
            panelAchStatistics.AddChild(PrgOneValue);
            panelAchStatistics.AddChild(PrgBulletsValue);
            panelAchStatistics.AddChild(PrgFkyValue);
            panelAchStatistics.AddChild(PrgMaxValue);
            panelAchStatistics.AddChild(PrgMinValue);
            panelAchStatistics.AddChild(PrgMaginotValue);
            panelAchStatistics.AddChild(PrgBtValue);

#endregion

            AddChild(tabControl);
            AddChild(lblFilter);
            AddChild(cmbGameClassFilter);
            AddChild(lblGameMode);
            AddChild(cmbGameModeFilter);
            AddChild(btnReturnToMenu);
            AddChild(btnClearStatistics);

            base.Initialize();

            CenterOnParent();

            sides = ClientConfiguration.Instance.Sides.Split(',')
                .Select(s => (Name: s, UIName: s.L10N($"INI:Sides:{s}"))).ToArray();

            sideTextures = new Texture2D[sides.Length + 1];
            for (int i = 0; i < sides.Length; i++)
                sideTextures[i] = AssetLoader.LoadTexture(sides[i].Name + "icon.png");

            sideTextures[sides.Length] = AssetLoader.LoadTexture("spectatoricon.png");

            mpColors = MultiplayerColor.LoadColors();

            ReadStatistics();
            ListGameModes();
            ListGames();

            StatisticsManager.Instance.GameAdded += Instance_GameAdded;

            locationY = totalstatsfirstitemy;
            AddAchBtn("btnhardened", "Battle-hardened".L10N("Client:Main:hardenedTitle"), "Ten years old players, what do not understand can ask me! : Complete 2000 matches".L10N("Client:Main:hardenedText"), new Point(totalstatslocationx1, locationY), 0);
            locationY += totalstatsyincrease;
            AddAchBtn("lblkillValue", "Kill like a stone".L10N("Client:Main:killTitle"), "I said I would kill. You asked me if my eyes were dry? Cumulative destruction reached 1,000,000".L10N("Client:Main:killText"), new Point(totalstatslocationx1, locationY), 1);
            locationY += totalstatsyincrease;
            AddAchBtn("PrgVictorValue", "Always victorious".L10N("Client:Main:VictorTitle"), "Just a stroke of luck! Win/loss ratio reached 10 (0.9 win percentage)".L10N("Client:Main:VictorText"), new Point(totalstatslocationx1, locationY), 2);
            locationY += totalstatsyincrease;
            AddAchBtn("PrgLongValue", "Boil eagle".L10N("Client:Main:LongTitle"), "I have two computers. Who's afraid of who? : Complete 200 45 minute matches".L10N("Client:Main:LongText"), new Point(totalstatslocationx1, locationY), 3);
            locationY += totalstatsyincrease;
            AddAchBtn("PrgShortValue", "Blitzkrieg".L10N("Client:Main:ShortTitle"), "Throw punches and kill the old master. : Complete 200 matches under 10 minutes".L10N("Client:Main:ShortText"), new Point(totalstatslocationx1, locationY), 4);
            locationY += totalstatsyincrease;
            AddAchBtn("PrgSoldierValue", "Love soldiers".L10N("Client:Main:SoldierTitle"), "We have very precise blood volume control. : Complete 200 matches with less than 10 losses".L10N("Client:Main:SoldierText"), new Point(totalstatslocationx1, locationY), 5);
            locationY += totalstatsyincrease;
            AddAchBtn("PrgNavalValue", "One Piece".L10N("Client:Main:NavalTitle"), "I want to be One Piece, not One Piece! Complete 10 naval battles".L10N("Client:Main:NavalText"), new Point(totalstatslocationx1, locationY), 6);
            locationY += totalstatsyincrease;
            AddAchBtn("PrgGermanyValue", "Convince by virtue".L10N("Client:Main:GermanyTitle"), "Persuade by virtue (physics). Win 20 matches with Germany".L10N("Client:Main:GermanyText"), new Point(totalstatslocationx1, locationY), 7);
            locationY += totalstatsyincrease;
            AddAchBtn("PrgOneValue", "King of singles".L10N("Client:Main:OneTitle"), "The single king is back. Win 200 singles matches".L10N("Client:Main:OneText"), new Point(totalstatslocationx1, locationY), 8);
            locationY = totalstatsfirstitemy;
            AddAchBtn("PrgBulletsValue", "Bullets rained down".L10N("Client:Main:BulletsTitle"), "Report! Pilot's back with a steering wheel! : Complete 200 matches with destruction greater than 100".L10N("Client:Main:BulletsText"), new Point(totalstatslocationx2, locationY), 9);
            locationY += totalstatsyincrease;
            AddAchBtn("PrgFkyValue", "Fuck Yuri".L10N("Client:Main:FkyTitle"), "This is destiny: Use France to win 20 games against Yuri".L10N("Client:Main:FkyText"), new Point(totalstatslocationx2, locationY), 10);
            locationY += totalstatsyincrease;
            AddAchBtn("PrgMaxValue", "Endeavour".L10N("Client:Main:MaxTitle"), "How can you play when all your teammates are cute? : Top rated but lost 100 games".L10N("Client:Main:MaxText"), new Point(totalstatslocationx2, locationY), 11);
            locationY += totalstatsyincrease;
            AddAchBtn("PrgMinValue", "Lie down to win".L10N("Client:Main:MinTitle"), "Beneficiaries of the ELO mechanism. Lowest rating yet 100 wins".L10N("Client:Main:MinText"), new Point(totalstatslocationx2, locationY), 12);
            locationY += totalstatsyincrease;
            AddAchBtn("PrgMaginotValue", "Maginot line".L10N("Client:Main:MaginotTitle"), "Why don't you come at me? : Use France and reach 400 to build 20 matches".L10N("Client:Main:MaginotText"), new Point(totalstatslocationx2, locationY), 13);
            locationY += totalstatsyincrease;
            AddAchBtn("PrgBtValue", "Know the trees".L10N("Client:Main:BtTitle"), "I know how many trees there are in the BT-maps: play (big)BT-maps 20 times".L10N("Client:Main:BtText"), new Point(totalstatslocationx2, locationY),14);
            locationY += totalstatsyincrease;
        }

        private void Instance_GameAdded(object sender, EventArgs e)
        {
            ListGames();
        }

        private void ChkIncludeSpectatedGames_CheckedChanged(object sender, EventArgs e)
        {
            ListGames();
        }

        private void AddTotalStatisticsLabel(string name, string text, Point location)
        {
            XNALabel label = new XNALabel(WindowManager);
            label.Name = name;
            label.Text = text;
            label.ClientRectangle = new Rectangle(location.X, location.Y, 0, 0);
            panelTotalStatistics.AddChild(label);
        }

        private void AddAchBtn(string name, string Title, string Content, Point location, int i)
        {
            XNAButton btn = new XNAClientButton(WindowManager);
            btn.Name = name;
            btn.Text = Title;
            btn.ClientRectangle = new Rectangle(location.X - 25, location.Y - 5, 100, 30);
            btn.IdleTexture = AssetLoader.LoadTexture("92pxbtn.png");
            btn.HoverTexture = AssetLoader.LoadTexture("92pxbtn_c.png");
            btn.LeftClick += (s, e) => Messagebox(Title, Content, i);
            panelAchStatistics.AddChild(btn);
        }

        private void Messagebox(string Title, string Content, int i)
        {
            if (Value[i, 0] < Value[i, 1])
                Content += "          " + Value[i, 0].ToString() + "/" + Value[i, 1].ToString();
            else
                Content += "          100%".L10N("Client:Main:100%");
            XNAMessageBox messageBox = new XNAMessageBox(WindowManager, Title, Content, XNAMessageBoxButtons.OK);
            messageBox.Show();
        }

        private void TabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl.SelectedTab == 1)
            {
                panelGameStatistics.Visible = false;
                panelGameStatistics.Enabled = false;
                panelTotalStatistics.Visible = true;
                panelTotalStatistics.Enabled = true;
                panelAchStatistics.Visible = false;
                panelAchStatistics.Enabled = false;
            }
            else if (tabControl.SelectedTab == 2)
            {
                panelGameStatistics.Visible = false;
                panelGameStatistics.Enabled = false;
                panelTotalStatistics.Visible = false;
                panelTotalStatistics.Enabled = false;
                panelAchStatistics.Visible = true;
                panelAchStatistics.Enabled = true;
            }
            else
            {
                panelGameStatistics.Visible = true;
                panelGameStatistics.Enabled = true;
                panelTotalStatistics.Visible = false;
                panelTotalStatistics.Enabled = false;
                panelAchStatistics.Visible = false;
                panelAchStatistics.Enabled = false;
            }
        }

        private void CmbGameClassFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListGames();
        }

        private void CmbGameModeFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListGames();
        }

        private void LbGameList_SelectedIndexChanged(object sender, EventArgs e)
        {
            lbGameStatistics.ClearItems();

            if (lbGameList.SelectedIndex == -1)
                return;

            MatchStatistics ms = sm.GetMatchByIndex(listedGameIndexes[lbGameList.SelectedIndex]);

            List<PlayerStatistics> players = new List<PlayerStatistics>();

            for (int i = 0; i < ms.GetPlayerCount(); i++)
            {
                players.Add(ms.GetPlayer(i));
            }

            players = players.OrderBy(p => p.Score).Reverse().ToList();

            Color textColor = UISettings.ActiveSettings.AltColor;

            for (int i = 0; i < ms.GetPlayerCount(); i++)
            {
                PlayerStatistics ps = players[i];

                //List<string> items = new List<string>();
                List<XNAListBoxItem> items = new List<XNAListBoxItem>();

                if (ps.Color > -1 && ps.Color < mpColors.Count)
                    textColor = mpColors[ps.Color].XnaColor;

                if (ps.IsAI)
                {
                    items.Add(new XNAListBoxItem(ProgramConstants.GetAILevelName(ps.AILevel), textColor));
                }
                else
                    items.Add(new XNAListBoxItem(ps.Name, textColor));

                if (ps.WasSpectator)
                {
                    // Player was a spectator
                    items.Add(new XNAListBoxItem("-", textColor));
                    items.Add(new XNAListBoxItem("-", textColor));
                    items.Add(new XNAListBoxItem("-", textColor));
                    items.Add(new XNAListBoxItem("-", textColor));
                    items.Add(new XNAListBoxItem("-", textColor));
                    XNAListBoxItem spectatorItem = new XNAListBoxItem();
                    spectatorItem.Text = "Spectator".L10N("Client:Main:Spectator");
                    spectatorItem.TextColor = textColor;
                    spectatorItem.Texture = sideTextures[sideTextures.Length - 1];
                    items.Add(spectatorItem);
                    items.Add(new XNAListBoxItem("-", textColor));
                }
                else
                { 
                    if (!ms.SawCompletion)
                    {
                        // The game wasn't completed - we don't know the stats
                        items.Add(new XNAListBoxItem("-", textColor));
                        items.Add(new XNAListBoxItem("-", textColor));
                        items.Add(new XNAListBoxItem("-", textColor));
                        items.Add(new XNAListBoxItem("-", textColor));
                        items.Add(new XNAListBoxItem("-", textColor));
                    }
                    else
                    {
                        // The game was completed and the player was actually playing
                        items.Add(new XNAListBoxItem(ps.Kills.ToString(), textColor));
                        items.Add(new XNAListBoxItem(ps.Losses.ToString(), textColor));
                        items.Add(new XNAListBoxItem(ps.Economy.ToString(), textColor));
                        items.Add(new XNAListBoxItem(ps.Score.ToString(), textColor));
                        items.Add(new XNAListBoxItem(
                            Conversions.BooleanToString(ps.Won, BooleanStringStyle.YESNO), textColor));
                    }

                    if (ps.Side == 0 || ps.Side > sides.Length)
                        items.Add(new XNAListBoxItem("Unknown".L10N("Client:Main:UnknownSide"), textColor));
                    else
                    {
                        XNAListBoxItem sideItem = new XNAListBoxItem();
                        sideItem.Text = sides[ps.Side - 1].UIName;
                        sideItem.TextColor = textColor;
                        sideItem.Texture = sideTextures[ps.Side - 1];
                        items.Add(sideItem);
                    }

                    items.Add(new XNAListBoxItem(TeamIndexToString(ps.Team), textColor));
                }

                if (!ps.IsLocalPlayer)
                {
                    lbGameStatistics.AddItem(items);

                    items.ForEach(item => item.Selectable = false);
                }
                else
                {
                    lbGameStatistics.AddItem(items);
                    lbGameStatistics.SelectedIndex = i;
                }
            }
        }

        private string TeamIndexToString(int teamIndex)
        {
            if (teamIndex < 1 || teamIndex >= ProgramConstants.TEAMS.Count)
                return "-";

            return ProgramConstants.TEAMS[teamIndex - 1];
        }

        #region Statistics reading / game listing code

        private void ReadStatistics()
        {
            StatisticsManager sm = StatisticsManager.Instance;

            sm.ReadStatistics(ProgramConstants.GamePath);
        }

        private void ListGameModes()
        {
            int gameCount = sm.GetMatchCount();

            List<string> gameModes = new List<string>();

            cmbGameModeFilter.Items.Clear();

            cmbGameModeFilter.AddItem("All".L10N("Client:Main:AllGameModes"));

            for (int i = 0; i < gameCount; i++)
            {
                MatchStatistics ms = sm.GetMatchByIndex(i);
                if (!gameModes.Contains(ms.GameMode))
                    gameModes.Add(ms.GameMode);
            }

            gameModes.Sort();

            foreach (string gm in gameModes)
                cmbGameModeFilter.AddItem(new XNADropDownItem { Text = gm.L10N($"INI:GameModes:{gm}:UIName"), Tag = gm });

            cmbGameModeFilter.SelectedIndex = 0;
        }

        private void ListGames()
        {
            lbGameList.SelectedIndex = -1;
            lbGameList.SetTopIndex(0);

            lbGameStatistics.ClearItems();
            lbGameList.ClearItems();
            listedGameIndexes.Clear();

            switch (cmbGameClassFilter.SelectedIndex)
            {
                case 0:
                    ListAllGames();
                    break;
                case 1:
                    ListOnlineGames();
                    break;
                case 2:
                    ListPvPGames();
                    break;
                case 3:
                    ListCoOpGames();
                    break;
                case 4:
                    ListSkirmishGames();
                    break;
            }

            listedGameIndexes.Reverse();

            SetTotalStatistics();

            SetAchStatistics();

            foreach (int gameIndex in listedGameIndexes)
            {
                MatchStatistics ms = sm.GetMatchByIndex(gameIndex);
                string dateTime = ms.DateAndTime.ToShortDateString() + " " + ms.DateAndTime.ToShortTimeString();
                List<string> info = new List<string>();
                info.Add(Renderer.GetSafeString(dateTime, lbGameList.FontIndex));
                info.Add(mapLoader.TranslatedMapNames.ContainsKey(ms.MapName)
                    ? mapLoader.TranslatedMapNames[ms.MapName]
                    : ms.MapName);
                info.Add(ms.GameMode.L10N($"INI:GameModes:{ms.GameMode}:UIName"));
                if (ms.AverageFPS == 0)
                    info.Add("-");
                else
                    info.Add(ms.AverageFPS.ToString());
                info.Add(Renderer.GetSafeString(TimeSpan.FromSeconds(ms.LengthInSeconds).ToString(), lbGameList.FontIndex));
                info.Add(Conversions.BooleanToString(ms.SawCompletion, BooleanStringStyle.YESNO));
                lbGameList.AddItem(info, true);
            }
        }

        private void ListAllGames()
        {
            int gameCount = sm.GetMatchCount();

            for (int i = 0; i < gameCount; i++)
            {
                ListGameIndexIfPrerequisitesMet(i);
            }
        }

        private void ListOnlineGames()
        {
            int gameCount = sm.GetMatchCount();

            for (int i = 0; i < gameCount; i++)
            {
                MatchStatistics ms = sm.GetMatchByIndex(i);

                int pCount = ms.GetPlayerCount();
                int hpCount = 0;

                for (int j = 0; j < pCount; j++)
                {
                    PlayerStatistics ps = ms.GetPlayer(j);

                    if (!ps.IsAI)
                    {
                        hpCount++;

                        if (hpCount > 1)
                        {
                            ListGameIndexIfPrerequisitesMet(i);
                            break;
                        }
                    }
                }
            }
        }

        private void ListPvPGames()
        {
            int gameCount = sm.GetMatchCount();

            for (int i = 0; i < gameCount; i++)
            {
                MatchStatistics ms = sm.GetMatchByIndex(i);

                int pCount = ms.GetPlayerCount();
                int pTeam = -1;

                for (int j = 0; j < pCount; j++)
                {
                    PlayerStatistics ps = ms.GetPlayer(j);

                    if (!ps.IsAI && !ps.WasSpectator)
                    {
                        // If we find a single player on a different team than another player,
                        // we'll count the game as a PvP game
                        if (pTeam > -1 && (ps.Team != pTeam || ps.Team == 0))
                        {
                            ListGameIndexIfPrerequisitesMet(i);
                            break;
                        }

                        pTeam = ps.Team;
                    }
                }
            }
        }

        private void ListCoOpGames()
        {
            int gameCount = sm.GetMatchCount();

            for (int i = 0; i < gameCount; i++)
            {
                MatchStatistics ms = sm.GetMatchByIndex(i);

                int pCount = ms.GetPlayerCount();
                int hpCount = 0;
                int pTeam = -1;
                bool add = true;

                for (int j = 0; j < pCount; j++)
                {
                    PlayerStatistics ps = ms.GetPlayer(j);

                    if (!ps.IsAI && !ps.WasSpectator)
                    {
                        hpCount++;

                        if (pTeam > -1 && (ps.Team != pTeam || ps.Team == 0))
                        {
                            add = false;
                            break;
                        }

                        pTeam = ps.Team;
                    }
                }

                if (add && hpCount > 1)
                {
                    ListGameIndexIfPrerequisitesMet(i);
                }
            }
        }

        private void ListSkirmishGames()
        {
            int gameCount = sm.GetMatchCount();

            for (int i = 0; i < gameCount; i++)
            {
                MatchStatistics ms = sm.GetMatchByIndex(i);

                int pCount = ms.GetPlayerCount();
                int hpCount = 0;
                bool add = true;

                foreach (PlayerStatistics ps in ms.Players)
                {
                    if (!ps.IsAI)
                    {
                        hpCount++;

                        if (hpCount > 1)
                        {
                            add = false;
                            break;
                        }
                    }
                }

                if (add)
                {
                    ListGameIndexIfPrerequisitesMet(i);
                }
            }
        }

        private void ListGameIndexIfPrerequisitesMet(int gameIndex)
        {
            MatchStatistics ms = sm.GetMatchByIndex(gameIndex);

            if (cmbGameModeFilter.SelectedIndex != 0)
            {
                // "All" doesn't have a tag but that doesn't matter since 0 is not checked
                var gameMode = (string)cmbGameModeFilter.Items[cmbGameModeFilter.SelectedIndex].Tag;

                if (ms.GameMode != gameMode)
                    return;
            }

            PlayerStatistics ps = ms.Players.Find(p => p.IsLocalPlayer);

            if (ps != null && !chkIncludeSpectatedGames.Checked)
            {
                if (ps.WasSpectator)
                    return;
            }

            listedGameIndexes.Add(gameIndex);
        }

        /// <summary>
        /// Adjusts the labels on the "Total statistics" tab.
        /// </summary>
        private void SetTotalStatistics()
        {
            int gamesStarted = 0;
            int gamesFinished = 0;
            int gamesPlayed = 0;
            int wins = 0;
            int gameLosses = 0;
            TimeSpan timePlayed = TimeSpan.Zero;
            int numEnemies = 0;
            int numAllies = 0;
            int totalKills = 0;
            int totalLosses = 0;
            int totalScore = 0;
            int totalEconomy = 0;
            int[] sideGameCounts = new int[sides.Length];
            int numEasyAIs = 0;
            int numMediumAIs = 0;
            int numHardAIs = 0;

            foreach (int gameIndex in listedGameIndexes)
            {
                MatchStatistics ms = sm.GetMatchByIndex(gameIndex);

                gamesStarted++;

                if (ms.SawCompletion)
                    gamesFinished++;

                timePlayed += TimeSpan.FromSeconds(ms.LengthInSeconds);

                PlayerStatistics localPlayer = FindLocalPlayer(ms);

                if (!localPlayer.WasSpectator)
                {
                    totalKills += localPlayer.Kills;
                    totalLosses += localPlayer.Losses;
                    totalScore += localPlayer.Score;
                    totalEconomy += localPlayer.Economy;

                    if (localPlayer.Side > 0 && localPlayer.Side <= sides.Length)
                        sideGameCounts[localPlayer.Side - 1]++;

                    if (!ms.SawCompletion)
                        continue;

                    if (localPlayer.Won)
                        wins++;
                    else
                        gameLosses++;

                    gamesPlayed++;

                    for (int i = 0; i < ms.GetPlayerCount(); i++)
                    {
                        PlayerStatistics ps = ms.GetPlayer(i);

                        if (!ps.WasSpectator && (!ps.IsLocalPlayer || ps.IsAI))
                        {
                            if (ps.Team == 0 || localPlayer.Team != ps.Team)
                                numEnemies++;
                            else
                                numAllies++;

                            if (ps.IsAI)
                            {
                                if (ps.AILevel == 0)
                                    numEasyAIs++;
                                else if (ps.AILevel == 1)
                                    numMediumAIs++;
                                else
                                    numHardAIs++;
                            }
                        }
                    }
                }
            }

            lblGamesStartedValue.Text = gamesStarted.ToString();
            lblGamesFinishedValue.Text = gamesFinished.ToString();
            lblWinsValue.Text = wins.ToString();
            lblLossesValue.Text = gameLosses.ToString();

            if (gameLosses > 0)
            {
                lblWinLossRatioValue.Text = Math.Round(wins / (double)gameLosses, 2).ToString();
                Value[2, 0] = Math.Round(wins / (double)gameLosses, 2);
            }
            else
            {
                Value[2, 0] = 0;
                lblWinLossRatioValue.Text = "-";
            }

            if (gamesStarted > 0)
            {
                lblAverageGameLengthValue.Text = TimeSpan.FromSeconds((int)timePlayed.TotalSeconds / gamesStarted).ToString();
            }
            else
                lblAverageGameLengthValue.Text = "-";

            if (gamesPlayed > 0)
            {
                lblAverageEnemyCountValue.Text = Math.Round(numEnemies / (double)gamesPlayed, 2).ToString();
                lblAverageAllyCountValue.Text = Math.Round(numAllies / (double)gamesPlayed, 2).ToString();
                lblKillsPerGameValue.Text = (totalKills / gamesPlayed).ToString();
                lblLossesPerGameValue.Text = (totalLosses / gamesPlayed).ToString();
                lblAverageEconomyValue.Text = (totalEconomy / gamesPlayed).ToString();
            }
            else
            {
                lblAverageEnemyCountValue.Text = "-";
                lblAverageAllyCountValue.Text = "-";
                lblKillsPerGameValue.Text = "-";
                lblLossesPerGameValue.Text = "-";
                lblAverageEconomyValue.Text = "-";
            }

            if (totalLosses > 0)
            {
                lblKillLossRatioValue.Text = Math.Round(totalKills / (double)totalLosses, 2).ToString();
            }
            else
                lblKillLossRatioValue.Text = "-";

            Value[0, 0] = gamesStarted;
            Value[1, 0] = totalKills;

            lblTotalTimePlayedValue.Text = timePlayed.ToString();
            lblTotalKillsValue.Text = totalKills.ToString();
            lblTotalLossesValue.Text = totalLosses.ToString();
            lblTotalScoreValue.Text = totalScore.ToString();
            lblFavouriteSideValue.Text = sides[GetHighestIndex(sideGameCounts)].UIName;

            if (numEasyAIs >= numMediumAIs && numEasyAIs >= numHardAIs)
                lblAverageAILevelValue.Text = "Easy".L10N("Client:Main:EasyAI");
            else if (numMediumAIs >= numEasyAIs && numMediumAIs >= numHardAIs)
                lblAverageAILevelValue.Text = "Medium".L10N("Client:Main:MediumAI");
            else
                lblAverageAILevelValue.Text = "Hard".L10N("Client:Main:HardAI");
        }

        private void SetAchStatistics()
        {
            TimeSpan timePlayed = TimeSpan.Zero;
            Value[3, 0] = 0;
            Value[4, 0] = 0;
            Value[5, 0] = 0;
            Value[6, 0] = 0;
            Value[7, 0] = 0;
            Value[8, 0] = 0;
            Value[9, 0] = 0;
            Value[10, 0] = 0;
            Value[11, 0] = 0;
            Value[12, 0] = 0;
            Value[13, 0] = 0;
            Value[14, 0] = 0;

            foreach (int gameIndex in listedGameIndexes)
            {
                MatchStatistics ms = sm.GetMatchByIndex(gameIndex);
                PlayerStatistics localPlayer = FindLocalPlayer(ms);
                if (ms.GameMode == "Navalwar")
                {
                    Value[6, 0]++;
                }
                if (string.Compare(TimeSpan.FromSeconds(ms.LengthInSeconds).ToString(), "00:45:00") > 0)
                    Value[3, 0]++;
                if (string.Compare(TimeSpan.FromSeconds(ms.LengthInSeconds).ToString(), "00:10:00") < 0)
                    Value[4, 0]++;
                if (!localPlayer.WasSpectator)
                {
                    if (localPlayer.Won)
                    {
                        if (localPlayer.Side == 4)
                            Value[7, 0]++;
                        if (ms.GetPlayerCount() == 2)
                            Value[8, 0]++;
                        if (ms.MapName == "XMP32S8" || ms.MapName == "xmp32mw" || ms.MapName == "mp32du" || ms.MapName == "heckfreezesover")
                            Value[14, 0]++;
                        if (localPlayer.Side == 3)
                        {
                            if ((ms.GetPlayer(0).Side == 3 && ms.GetPlayer(1).Side == 10) || (ms.GetPlayer(1).Side == 3 && ms.GetPlayer(0).Side == 10))
                                Value[10, 0]++;
                            if (localPlayer.Economy > 500)
                                Value[13, 0]++;
                        }
                        int Max = 0, Min = 9999999;
                        for (int c = 0; c < ms.GetPlayerCount(); c++)
                        {
                            if (ms.GetPlayer(c).Score > Max)
                                Max = ms.GetPlayer(c).Score;
                            if (ms.GetPlayer(c).Score < Min)
                                Min = ms.GetPlayer(c).Score;
                        }
                        if (localPlayer.Score == Max && !localPlayer.Won)
                            Value[11, 0]++;
                        if (localPlayer.Score == Min && localPlayer.Won)
                            Value[12, 0]++;
                        if (localPlayer.Losses <= 10) 
                        Value[5, 0]++;
                        if (localPlayer.Losses + localPlayer.Kills > 1000)
                            Value[9, 0]++;
                    }
                }
            }
            PrghardenedValue.Value = (int)Value[0, 0];
            PrgkillValue.Value = (int)Value[1, 0];
            PrgVictorValue.Value = (int)Value[2, 0];
            PrgLongValue.Value = (int)Value[3, 0];
            PrgShortValue.Value = (int)Value[4, 0];
            PrgSoldierValue.Value = (int)Value[5, 0];
            PrgNavalValue.Value = (int)Value[6, 0];
            PrgGermanyValue.Value = (int)Value[7, 0];
            PrgOneValue.Value = (int)Value[8, 0];
            PrgBulletsValue.Value = (int)Value[9, 0];
            PrgFkyValue.Value = (int)Value[10, 0];
            PrgMaxValue.Value = (int)Value[11, 0];
            PrgMinValue.Value = (int)Value[12, 0];
            PrgMaginotValue.Value = (int)Value[13, 0];
            PrgBtValue.Value = (int)Value[14, 0];
        }

        private PlayerStatistics FindLocalPlayer(MatchStatistics ms)
        {
            int pCount = ms.GetPlayerCount();

            for (int pId = 0; pId < pCount; pId++)
            {
                PlayerStatistics ps = ms.GetPlayer(pId);

                if (!ps.IsAI && ps.IsLocalPlayer)
                    return ps;
            }

            return null;
        }

        private int GetHighestIndex(int[] t)
        {
            int highestIndex = -1;
            int highest = Int32.MinValue;

            for (int i = 0; i < t.Length; i++)
            {
                if (t[i] > highest)
                {
                    highest = t[i];
                    highestIndex = i;
                }
            }

            return highestIndex;
        }

        private void ClearAllStatistics()
        {
            StatisticsManager.Instance.ClearDatabase();
            ReadStatistics();
            ListGameModes();
            ListGames();
        }

        #endregion

        private void BtnReturnToMenu_LeftClick(object sender, EventArgs e)
        {
            // To hide the control, just set Enabled=false
            // and MainMenuDarkeningPanel will deal with the rest
            Enabled = false;
        }

        private void BtnClearStatistics_LeftClick(object sender, EventArgs e)
        {
            var msgBox = new XNAMessageBox(WindowManager, "Clear all statistics".L10N("Client:Main:ClearStatisticsTitle"),
                ("All statistics data will be cleared from the database.\n\nAre you sure you want to continue?").L10N("Client:Main:ClearStatisticsText"), XNAMessageBoxButtons.YesNo);
            msgBox.Show();
            msgBox.YesClickedAction = ClearStatisticsConfirmation_YesClicked;
        }

        private void ClearStatisticsConfirmation_YesClicked(XNAMessageBox messageBox)
        {
            ClearAllStatistics();
        }
    }
}
