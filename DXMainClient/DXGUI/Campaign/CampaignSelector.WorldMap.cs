using System;

using DTAClient.Domain;

using Microsoft.Xna.Framework;

namespace DTAClient.DXGUI.Campaign
{
    public partial class CampaignSelector
    {
        private const string WORLD_MAP_TEXTURE = "worldmaptexture.png";
        private const string WORLD_MAP_OVERLAY_TEXTURE = "worldmapoverlay.png";
        private const float WORLD_MAP_OVERLAY_SCALE = 1.05f;

        // The demo sizes the globe as min(window width, window height) - GLOBE_SIZE_MARGIN.
        private const int GLOBE_SIZE_MARGIN = 150;

        // Vertical placement of the globe. The globe is always horizontally centered.
        // Leave both at -1 to center it vertically.
        // Set exactly one to >= 0 to anchor the globe that many pixels from the
        // top or bottom edge of the campaign window. Top takes precedence if both are set.
        private static readonly int GLOBE_TOP_MARGIN = -1;
        private static readonly int GLOBE_BOTTOM_MARGIN = 160;

        private RotatingEarthBackground rotatingEarth;

        private void InitializeWorldMap()
        {
            int globeDiameter = Math.Max(100, Math.Min(Width, Height) - GLOBE_SIZE_MARGIN);

            rotatingEarth = new RotatingEarthBackground(WindowManager, WORLD_MAP_TEXTURE)
            {
                ClientRectangle = new Rectangle(0, 0, Width, Height),
                InputEnabled = false,
                DrawOrder = 100,
				UpdateOrder = 100,
                GlobeDiameter = globeDiameter
            };

            if (GLOBE_TOP_MARGIN >= 0)
                rotatingEarth.GlobeY = GLOBE_TOP_MARGIN;
            else if (GLOBE_BOTTOM_MARGIN >= 0)
                rotatingEarth.GlobeY = Height - globeDiameter - GLOBE_BOTTOM_MARGIN;

            AddChild(rotatingEarth);

            rotatingEarth.SetWorldMapOverlay(WORLD_MAP_OVERLAY_TEXTURE, WORLD_MAP_OVERLAY_SCALE);
        }

        /// <summary>
        /// Updates the globe for the currently selected mission. Called from the
        /// campaign list's SelectedIndexChanged handler before its regular logic.
        /// </summary>
        private void UpdateWorldMap()
        {
            if (rotatingEarth == null)
                return;

            int index = lbCampaignList.SelectedIndex;
            if (index < 0 || index >= selectedMissions.Count)
            {
                rotatingEarth.ClearImages();
                rotatingEarth.ResetLocation();
                return;
            }

            Mission mission = selectedMissions[index];
            rotatingEarth.SetImages(mission.GlobeImages);

            if (mission.LocationLongitude.HasValue && mission.LocationLatitude.HasValue)
                rotatingEarth.SetLocation(mission.LocationLongitude.Value, mission.LocationLatitude.Value);
            else
                rotatingEarth.ResetLocation();
        }
    }
}