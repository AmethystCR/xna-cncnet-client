using ClientCore;
using ClientCore.Extensions;
using Microsoft.Xna.Framework;
using Rampastring.Tools;
using System;
using System.Collections.Generic;

namespace ClientGUI
{
    /// <summary>
    /// A color for gameoptions.
    /// </summary>
    public class UnifiedTechnoColor
    {
        public int GameColorIndex { get; private set; }
        public string Name { get; private set; }
        public Color XnaColor { get; private set; }

        private static List<UnifiedTechnoColor> colorList;

        /// <summary>
        /// Creates a new unified color from data in a string array.
        /// </summary>
        /// <param name="name">The name of the color.</param>
        /// <param name="data">The input data. Needs to be in the format R,G,B,(game color index).</param>
        /// <returns>A new unified color created from the given string array.</returns>
        public static UnifiedTechnoColor CreateFromStringArray(string name, string[] data)
        {
            return new UnifiedTechnoColor()
            {
                Name = name,
                XnaColor = new Color(Math.Min(255, Int32.Parse(data[0])),
                Math.Min(255, Int32.Parse(data[1])),
                Math.Min(255, Int32.Parse(data[2])), 255),
                GameColorIndex = Int32.Parse(data[3])
            };
        }

        /// <summary>
        /// Returns the available unified colors.
        /// </summary>
        public static List<UnifiedTechnoColor> LoadColors()
        {
            if (colorList != null)
                return new List<UnifiedTechnoColor>(colorList);

            IniFile gameOptionsIni = new IniFile(SafePath.CombineFilePath(ProgramConstants.GetBaseResourcePath(), "GameOptions.ini"));

            List<UnifiedTechnoColor> utColors = new List<UnifiedTechnoColor>();

            List<string> colorKeys = gameOptionsIni.GetSectionKeys("UTColors");

            if (colorKeys == null)
                throw new ClientConfigurationException("[UTColors] not found in GameOptions.ini!");

            foreach (string key in colorKeys)
            {
                string[] values = gameOptionsIni.GetStringValue("UTColors", key, "255,255,255,0").Split(',');

                try
                {
                    UnifiedTechnoColor utColor = UnifiedTechnoColor.CreateFromStringArray(key, values);
                    utColors.Add(utColor);
                }
                catch
                {
                    throw new ClientConfigurationException("Invalid UTColor specified in GameOptions.ini: " + key);
                }
            }

            colorList = utColors;
            return new List<UnifiedTechnoColor>(colorList);
        }
    }
}
