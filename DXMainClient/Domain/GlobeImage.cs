namespace DTAClient.Domain
{
    /// <summary>
    /// An image that is placed on the world map globe at a geographic position,
    /// defined by the <c>ImageN</c> key in a mission section of a battle INI
    /// (format "image.png,longitude,latitude[,size%]"). The image is shown baked onto
    /// the globe's surface while its mission is selected.
    /// </summary>
    public sealed class GlobeImage
    {
        public GlobeImage(string imagePath, double longitude, double latitude, float? heightPercent)
        {
            ImagePath = imagePath;
            Longitude = longitude;
            Latitude = latitude;
            HeightPercent = heightPercent;
        }

        /// <summary>
        /// The image file name, loaded from the Resources folder (same folder the
        /// world map images are loaded from).
        /// </summary>
        public string ImagePath { get; }

        /// <summary>
        /// The longitude of the image on the world map (degrees, positive east).
        /// </summary>
        public double Longitude { get; }

        /// <summary>
        /// The latitude of the image on the world map (degrees, positive north).
        /// </summary>
        public double Latitude { get; }

        /// <summary>
        /// The image height as a percentage of the world map texture height (e.g.
        /// 5 = 5%). Null means the image is drawn at its original pixel size.
        /// </summary>
        public float? HeightPercent { get; }
    }
}
