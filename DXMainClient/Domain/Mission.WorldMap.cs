using System.Collections.Generic;
using System.Globalization;

using Rampastring.Tools;

namespace DTAClient.Domain
{
    public partial class Mission
    {
        /// <summary>
        /// The longitude of the mission's location on the world map (degrees, positive east),
        /// parsed from the <c>Location</c> key. Null if not set or invalid.
        /// </summary>
        public double? LocationLongitude { get; private set; }

        /// <summary>
        /// The latitude of the mission's location on the world map (degrees, positive north),
        /// parsed from the <c>Location</c> key. Null if not set or invalid.
        /// </summary>
        public double? LocationLatitude { get; private set; }

        /// <summary>
        /// The images placed on the world map globe for this mission, parsed from the
        /// <c>ImageN</c> keys (format "image.png,longitude,latitude[,size%]"). Shown
        /// baked onto the globe while this mission is selected.
        /// </summary>
        public List<GlobeImage> GlobeImages { get; } = new();

        /// <summary>
        /// Parses the world map related keys of the mission section: <c>Location</c>
        /// and <c>Image0</c>...<c>ImageN</c>.
        /// </summary>
        private void ParseWorldMapData(IniSection missionSection)
        {
            ParseLocation(missionSection.GetStringValue("Location", string.Empty));
            ParseGlobeImages(missionSection);
        }

        /// <summary>
        /// Parses the <c>Location</c> key, which is expected to be "longitude,latitude"
        /// (e.g. "-77.0,38.9"). On failure both coordinates are left as null.
        /// </summary>
        private void ParseLocation(string location)
        {
            if (string.IsNullOrWhiteSpace(location))
                return;

            string[] parts = location.Split(',');
            if (parts.Length < 2)
                return;

            if (!double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out double longitude))
                return;
            if (!double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out double latitude))
                return;

            LocationLongitude = longitude;
            LocationLatitude = latitude;
        }

        /// <summary>
        /// Parses the <c>Image0</c>...<c>ImageN</c> keys into globe overlay images.
        /// Entries with a missing key end the list; malformed entries are skipped.
        /// </summary>
        private void ParseGlobeImages(IniSection missionSection)
        {
            GlobeImages.Clear();

            for (int i = 0; i < 32; i++)
            {
                string value = missionSection.GetStringValue("Image" + i, string.Empty);
                if (string.IsNullOrWhiteSpace(value))
                    break;

                string[] parts = value.Split(',');
                if (parts.Length < 3)
                    continue;

                string imagePath = parts[0].Trim();
                if (string.IsNullOrWhiteSpace(imagePath))
                    continue;

                if (!double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out double longitude))
                    continue;
                if (!double.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out double latitude))
                    continue;

                float? heightPercent = null;
                if (parts.Length >= 4 && !string.IsNullOrWhiteSpace(parts[3]))
                {
                    string sizeText = parts[3].Trim().TrimEnd('%');
                    if (float.TryParse(sizeText, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsedPercent))
                        heightPercent = parsedPercent;
                }

                GlobeImages.Add(new GlobeImage(imagePath, longitude, latitude, heightPercent));
            }
        }
    }
}