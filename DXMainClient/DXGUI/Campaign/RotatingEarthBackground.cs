using System;
using System.Collections.Generic;

#if XNA
using Image = SixLabors.ImageSharp.Image;
using SixLabors.ImageSharp.Processing;
#endif

using ClientCore;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Rampastring.Tools;
using Rampastring.XNAUI;
using Rampastring.XNAUI.XNAControls;

using DTAClient.Domain;

namespace DTAClient.DXGUI.Campaign
{
    /// <summary>
    /// Renders a real 3D rotating earth globe (world map texture wrapped on a sphere)
    /// that slowly spins from the western hemisphere to the eastern hemisphere and
    /// back, looping like an animated gif. The globe is drawn to a render target in
    /// Update (when no sprite batch is active) and blitted in Draw.
    /// </summary>
    public class RotatingEarthBackground : XNAControl
    {
#if XNA
        // XNA's Reach profile only supports 16-bit indices, which limits the sphere
        // to 65536 vertices. 288 x 224 = 65025 vertices stays within that limit.
        private const int LON_SEGMENTS = 288;
        private const int LAT_SEGMENTS = 224;
#else
        private const int LON_SEGMENTS = 432;
        private const int LAT_SEGMENTS = 288;
#endif

        // Rotation speed (radians/second). One full 360-degree turn takes 36 seconds.
        private const double ROTATION_SPEED = MathHelper.TwoPi / 36.0;

        // Camera distance from the globe center (globe radius is 1).
        private const float CAMERA_DISTANCE = 4.0f;

        // Vertical field of view. Smaller values zoom the globe in so it fills more
        // of the render target; 37 degrees makes the globe larger on screen.
        private static readonly float CAMERA_FOV = MathHelper.ToRadians(37f);

        // The projected globe disc's diameter as a fraction of the render target size.
        // The sphere has radius 1 at CAMERA_DISTANCE, so its angular radius is
        // asin(1 / CAMERA_DISTANCE); projected onto the view plane that becomes
        // tan(angularRadius) / tan(FOV / 2) of the render target's height. Used to size
        // the static chapter overlay so it fits exactly against the globe's edge.
        private static readonly float GLOBE_DISC_FRACTION =
            (float)Math.Tan(Math.Asin(1.0f / CAMERA_DISTANCE)) / (float)Math.Tan(CAMERA_FOV / 2.0f);

        // The chapter overlay's bright ring edge, as a fraction of the image's half-width.
        // HALO-style overlay images are square canvases where the ring's bright edge sits
        // at ~89.4% of the half-width (the remainder is a faint glow reaching the canvas
        // edge). The overlay is drawn at GLOBE_DISC_FRACTION / OVERLAY_RING_EDGE_FRACTION
        // of the render target so the bright ring lands on the globe's circumference.
        private const float OVERLAY_RING_EDGE_FRACTION = 0.894f;

        // Duration in seconds of the FOV / camera-orientation transition animation.
        private const float LOCATION_TRANSITION_DURATION = 1.0f;

        private Texture2D worldMapTexture;
        private Texture2D previousWorldMapTexture;
        private string currentWorldMapName = "worldmap.png";

        // Cache of already loaded / flattened world maps, keyed by asset name. Without
        // this, MakeWorldMapOpaque would perform a full GPU readback (and CPU scan) of
        // the (potentially very large) world map texture on every chapter switch.
        private readonly Dictionary<string, Texture2D> worldMapCache = new(StringComparer.OrdinalIgnoreCase);

        private float transitionProgress = 1.0f;

        private VertexPositionNormalTexture[] sphereVertices;
        private int[] sphereIndices;

        // The sphere geometry uploaded once into GPU buffers. The geometry never
        // changes, so re-submitting it via DrawUserIndexedPrimitives every frame would
        // waste CPU/bandwidth; drawing from these cached buffers keeps rendering cheap
        // even while the cross-fade draws the globe twice.
        private VertexBuffer sphereVertexBuffer;
        private IndexBuffer sphereIndexBuffer;
        private BasicEffect effect;
        private RenderTarget2D globeRenderTarget;
        private bool initialized3D;
        private bool globeUnavailable;

        // An extra texture shown on top of the globe (per chapter), plus its scale
        // relative to the globe's disc (1.0 = the overlay's bright ring fits exactly the
        // globe's circumference). Unlike the world map it is drawn as a flat screen-space
        // sprite, so it stays fixed and does not rotate with the earth map itself.
        private Texture2D worldMapOverlayTexture;
        private float worldMapOverlayScale = 1.0f;
        private string worldMapOverlayName = string.Empty;

        // The previous chapter's overlay, cross-faded out over the world map transition
        // instead of being removed instantly when the chapter's overlay changes (e.g.
        // when switching to a chapter without an overlay).
        private Texture2D previousWorldMapOverlayTexture;
        private float previousWorldMapOverlayScale = 1.0f;

        // Cache of chapter overlay textures, owned by this control. They are loaded once
        // and reused across chapter switches; disposing them on switch would evict them
        // from the cache and force a full re-decode of the (large) PNG on the next visit,
        // which caused a visible stutter.
        private readonly Dictionary<string, Texture2D> overlayCache = new(StringComparer.OrdinalIgnoreCase);

        // Opacity the chapter overlay is drawn at (fixed; the 5th ChapterButtons column
        // controls the overlay's scale instead of the opacity).
        private const float WORLD_MAP_OVERLAY_ALPHA = 0.5f;

        // Marks the globe's circular silhouette (stencil = 1) while the rotating world
        // map is drawn, so the static chapter overlay can be clipped to the globe's disc.
        private readonly DepthStencilState sphereStencilState = new()
        {
            DepthBufferEnable = true,
            DepthBufferWriteEnable = true,
            DepthBufferFunction = CompareFunction.LessEqual,
            StencilEnable = true,
            StencilWriteMask = 0xFF,
            StencilFunction = CompareFunction.Always,
            StencilPass = StencilOperation.Replace,
            ReferenceStencil = 1
        };

        // Used to draw the static (non-rotating) chapter overlay, clipped to the globe's
        // disc via the stencil marked by sphereStencilState. Depth testing is disabled so
        // the overlay always faces the camera and is never occluded by the sphere.
        private readonly DepthStencilState overlayStencilState = new()
        {
            DepthBufferEnable = false,
            StencilEnable = true,
            StencilFunction = CompareFunction.Equal,
            StencilMask = 0xFF,
            StencilWriteMask = 0,
            ReferenceStencil = 1
        };

        // Cross-fade duration in seconds.
        private const float TRANSITION_DURATION = 0.8f;

        // Images placed on the globe at geographic coordinates (mission ImageN keys),
        // shown while the owning mission is selected. The images are baked into a
        // transparent overlay texture at the world map's resolution, which is drawn as
        // a second sphere pass so the images sit on the globe's surface and rotate
        // with it.
        private RenderTarget2D overlayTexture;
        private readonly List<Texture2D> overlayImageTextures = new();

        // SpriteBatch used to bake the overlay texture.
        private SpriteBatch overlaySpriteBatch;

        /// <summary>
        /// The diameter (in window pixels) the globe is drawn at, centered in the window.
        /// </summary>
        public int GlobeDiameter { get; set; } = 500;

        /// <summary>
        /// The top Y coordinate of the globe within the control. When null, the globe
        /// is vertically centered. Horizontal placement is always centered.
        /// </summary>
        public int? GlobeY { get; set; }

        /// <summary>
        /// The globe is rendered into a render target scaled up by this factor and then
        /// drawn downscaled to the on-screen size, acting as supersampling anti-aliasing
        /// for crisper edges when zoomed in.
        /// </summary>
        private const float RENDER_SCALE = 2.0f;

        /// <summary>
        /// When set, the globe stops auto-rotating and instead keeps the given
        /// longitude/latitude (degrees) facing the camera. Null to resume rotating.
        /// </summary>
        private double? fixedLongitude;
        private double? fixedLatitude;

        // Current and target vertical field of view (radians) for the zoom animation.
        private float currentFov = CAMERA_FOV;
        private float targetFov = CAMERA_FOV;

        // FOV at the start of the current zoom transition, interpolated together with
        // the camera orientation so the zoom speed matches the camera movement speed.
        private float fovStart;

        // The globe's yaw while auto-rotating, kept up to date each frame so a
        // SetLocation call can start from the current rotation angle.
        private float autoRotationYaw;

        // Time offset applied to auto-rotation so it resumes seamlessly from the
        // returned position instead of jumping to the raw elapsed-time angle.
        private double autoRotationTimeOffset;

        // The auto-rotation yaw to smoothly fly back to when a location lock is
        // released, and whether such a return transition is in progress.
        private float returnYaw;
        private bool isReturning;

        // Current camera orientation, used as the starting point of a transition.
        private float currentYaw;
        private float currentLatitude;

        // Target camera orientation (radians) for an in-progress transition.
        private float targetYaw;
        private float targetLatitude;
        private bool isTransitioning;
        private float transitionElapsed;

        /// <summary>
        /// Stops the globe's auto-rotation and smoothly flies the camera to the given
        /// longitude and latitude (in degrees, east positive for longitude, north
        /// positive for latitude) while zooming in.
        /// </summary>
        public void SetLocation(double longitude, double latitude)
        {
            float newTargetYaw = MathHelper.ToRadians(90.0f - (float)longitude);
            float newTargetLatitude = MathHelper.ToRadians((float)latitude);

            if (isTransitioning)
            {
                // Continue from the current transition position rather than jumping.
                float eased = GetTransitionEasedProgress();
                currentYaw = currentYaw + MathHelper.WrapAngle(targetYaw - currentYaw) * eased;
                currentLatitude += (targetLatitude - currentLatitude) * eased;
            }
            else if (fixedLongitude.HasValue && fixedLatitude.HasValue)
            {
                // Already locked; resume from the locked position.
                currentYaw = targetYaw;
                currentLatitude = targetLatitude;
            }
            else
            {
                // Auto-rotating; start from the current rotation angle at the equator.
                currentYaw = autoRotationYaw;
                currentLatitude = 0.0f;
                returnYaw = autoRotationYaw;
            }

            fixedLongitude = longitude;
            fixedLatitude = latitude;
            targetYaw = newTargetYaw;
            targetLatitude = newTargetLatitude;
            fovStart = currentFov;
            targetFov = CAMERA_FOV;
            transitionElapsed = 0.0f;
            isTransitioning = true;
            isReturning = false;
        }

        /// <summary>
        /// Resumes the globe's normal continuous auto-rotation, smoothly flying back
        /// to the rotation angle it had before the location was set and smoothly
        /// restoring the default zoom level.
        /// </summary>
        public void ResetLocation()
        {
            if (isTransitioning)
            {
                // A transition to a location is in progress; finish it first so the
                // return starts from the exact locked position.
                float eased = GetTransitionEasedProgress();
                currentYaw = currentYaw + MathHelper.WrapAngle(targetYaw - currentYaw) * eased;
                currentLatitude += (targetLatitude - currentLatitude) * eased;
                currentFov = fovStart + (targetFov - fovStart) * eased;
            }
            else if (fixedLongitude.HasValue && fixedLatitude.HasValue)
            {
                // Already locked; start the return from the locked position.
                currentYaw = targetYaw;
                currentLatitude = targetLatitude;
                currentFov = targetFov;
            }
            else
            {
                // Already auto-rotating; nothing to transition.
                return;
            }

            fixedLongitude = null;
            fixedLatitude = null;
            targetYaw = returnYaw;
            targetLatitude = 0.0f;
            fovStart = currentFov;
            targetFov = CAMERA_FOV;
            transitionElapsed = 0.0f;
            isTransitioning = true;
            isReturning = true;
        }

        /// <summary>
        /// Returns the eased (smoothstep) progress of the transition from the elapsed
        /// time, clamped to [0, 1].
        /// </summary>
        private float GetTransitionEasedProgress()
        {
            float progress = Math.Min(1.0f, transitionElapsed / LOCATION_TRANSITION_DURATION);
            return progress * progress * (3.0f - 2.0f * progress);
        }

        public RotatingEarthBackground(WindowManager windowManager, string worldMapName = "worldmap.png") : base(windowManager)
        {
            currentWorldMapName = string.IsNullOrWhiteSpace(worldMapName) ? "worldmap.png" : worldMapName;
            InputEnabled = false;
            DrawOrder = -200;
        }

        public override void Initialize()
        {
            worldMapTexture = LoadWorldMap(currentWorldMapName);

            CreateSphereGeometry();

            base.Initialize();
        }

        /// <summary>
        /// Loads a world map texture by name (e.g. "worldmap.png"). Falls back to the
        /// default world map if the requested one is missing. When no world map exists
        /// at all, returns null and the globe is not drawn.
        /// The texture is flattened to fully opaque so the globe's surface is always
        /// solid (a semi-transparent map would otherwise make the cross-fade snap, with
        /// transparent regions suddenly flipping from the previous map to black when the
        /// transition completes).
        /// </summary>
        private Texture2D LoadWorldMap(string name)
        {
            if (string.IsNullOrEmpty(name))
                name = "worldmap.png";

            if (worldMapCache.TryGetValue(name, out Texture2D cached))
                return cached;

            Texture2D result = null;

            string path = SafePath.CombineFilePath(ProgramConstants.GetBaseResourcePath(), name);

            if (SafePath.GetFile(path).Exists)
            {
                Texture2D texture = LoadWorldMapTexture(path, name);
                if (texture != null)
                    result = MakeWorldMapOpaque(texture);
            }
            else if (name != "worldmap.png")
            {
                result = LoadWorldMap("worldmap.png");
            }
            else
            {
                Logger.Log("RotatingEarthBackground: world map texture 'worldmap.png' was not found. The globe will not be drawn.");
            }

            worldMapCache[name] = result;
            return result;
        }

        /// <summary>
        /// Loads a world map texture from the given file. On XNA's Reach profile the
        /// texture is downscaled to fit the 2048-pixel texture size limit; every other
        /// platform loads it through AssetLoader at its original resolution.
        /// </summary>
        private Texture2D LoadWorldMapTexture(string path, string name)
        {
#if XNA
            if (WindowManager.GraphicsDevice.GraphicsProfile == GraphicsProfile.Reach)
            {
                try
                {
                    using Image image = Image.Load(path);

                    if (image.Width > 2048 || image.Height > 2048)
                    {
                        float scale = Math.Min(2048f / image.Width, 2048f / image.Height);
                        int width = Math.Max(1, (int)(image.Width * scale));
                        int height = Math.Max(1, (int)(image.Height * scale));
                        image.Mutate(x => x.Resize(width, height));
                        Logger.Log($"RotatingEarthBackground: downscaled world map '{name}' to {width}x{height} for the XNA Reach profile.");
                    }

                    Texture2D texture = AssetLoader.TextureFromImage(image);
                    if (texture != null)
                        texture.Name = name;
                    return texture;
                }
                catch (Exception ex)
                {
                    Logger.Log($"RotatingEarthBackground: failed to load world map '{name}'. Reason: {ex.Message}");
                    return null;
                }
            }
#endif
            return AssetLoader.LoadTexture(name);
        }

        /// <summary>
        /// Flattens the alpha channel of a world map texture to fully opaque when it
        /// contains any transparency, keeping its (premultiplied) RGB values unchanged.
        /// </summary>
        private static Texture2D MakeWorldMapOpaque(Texture2D texture)
        {
            Color[] data = new Color[texture.Width * texture.Height];
            texture.GetData(data);

            bool hasTransparency = false;
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i].A < 255)
                {
                    hasTransparency = true;
                    break;
                }
            }

            if (hasTransparency)
            {
                for (int i = 0; i < data.Length; i++)
                    data[i].A = 255;
                texture.SetData(data);
            }

            return texture;
        }

        /// <summary>
        /// Warms up the world map texture cache for the given asset name so that
        /// switching to it later doesn't stall on a synchronous disk load and decode.
        /// </summary>
        public void PreloadWorldMap(string textureName)
        {
            if (string.IsNullOrWhiteSpace(textureName))
                return;

            LoadWorldMap(textureName);
        }

        /// <summary>
        /// Warms up the chapter overlay texture cache for the given asset name so that
        /// switching to it later doesn't stall on a synchronous disk load and decode.
        /// </summary>
        public void PreloadWorldMapOverlay(string imageName)
        {
            if (string.IsNullOrWhiteSpace(imageName))
                return;

            GetOverlayTexture(imageName);
        }

        /// <summary>
        /// Returns the chapter overlay texture for the given asset name, loading and
        /// caching it on first use. The returned textures are owned by this control
        /// (see <see cref="Kill"/>) and must never be disposed individually - doing so
        /// would evict them from the cache and force a full re-decode of the (large)
        /// PNG on the next chapter switch.
        /// </summary>
        private Texture2D GetOverlayTexture(string imageName)
        {
            if (string.IsNullOrEmpty(imageName))
                return null;

            if (overlayCache.TryGetValue(imageName, out Texture2D cached))
                return cached;

            Texture2D result = AssetLoader.AssetExists(imageName)
                ? AssetLoader.LoadTextureUncached(imageName)
                : null;

            overlayCache[imageName] = result;
            return result;
        }

        /// <summary>
        /// Switches the globe's world map texture to the one with the given asset name
        /// (e.g. "worldmap.png"), cross-fading smoothly from the current texture.
        /// </summary>
        public void SetWorldMap(string textureName)
        {
            if (string.IsNullOrWhiteSpace(textureName))
                textureName = "worldmap.png";

            if (textureName == currentWorldMapName)
                return;

            Texture2D newTexture = LoadWorldMap(textureName);
            if (newTexture == null)
                return;

            previousWorldMapTexture = worldMapTexture;
            worldMapTexture = newTexture;
            currentWorldMapName = textureName;
            transitionProgress = 0.0f;
        }

        /// <summary>
        /// Sets the texture shown on top of the globe (given by an asset name) and its
        /// scale relative to the globe's disc (1.0 = the overlay's bright ring fits the
        /// globe's circumference). Pass null or an empty/missing asset to clear the
        /// overlay. The texture is drawn as a flat screen-space sprite clipped to the
        /// globe's disc, so it does not rotate with the world map.
        /// </summary>
        public void SetWorldMapOverlay(string imageName, float scale)
        {
            if (string.IsNullOrWhiteSpace(imageName))
                imageName = string.Empty;

            if (!string.Equals(imageName, worldMapOverlayName, StringComparison.OrdinalIgnoreCase))
            {
                // The overlay changed: keep the outgoing overlay as the cross-fading-out
                // "previous" overlay. The overlay textures are owned by overlayCache and
                // must NOT be disposed here - disposing them would evict them and force a
                // full re-decode of the (large) PNG on the next chapter switch.
                previousWorldMapOverlayTexture = worldMapOverlayTexture;
                previousWorldMapOverlayScale = worldMapOverlayScale;
                worldMapOverlayTexture = null;
                worldMapOverlayName = imageName;
            }

            worldMapOverlayScale = Math.Max(scale, 0.01f);

            worldMapOverlayTexture = GetOverlayTexture(imageName);
        }

        /// <summary>
        /// Sets the images to show on the globe at their geographic coordinates. Each
        /// image is baked into a transparent overlay texture (sized like the current
        /// world map) at the pixel position matching its longitude/latitude, using the
        /// same projection as the <c>Location</c> camera targeting so the images land
        /// exactly where the mission location points. Any previously set images are
        /// replaced. Images whose file is missing are skipped.
        /// </summary>
        public void SetImages(IEnumerable<GlobeImage> images)
        {
            if (worldMapTexture == null)
            {
                ClearImages();
                return;
            }

            // Resolve the images that actually exist before touching the current state so
            // that we never allocate an overlay render target just to end up with none.
            var resolved = new List<GlobeImage>();

            if (images != null)
            {
                foreach (GlobeImage image in images)
                {
                    if (string.IsNullOrWhiteSpace(image.ImagePath) || !AssetLoader.AssetExists(image.ImagePath))
                    {
                        Rampastring.Tools.Logger.Log($"RotatingEarthBackground: globe image '{image.ImagePath}' not found. Skipping.");
                        continue;
                    }

                    resolved.Add(image);
                }
            }

            // Release the previous source textures (loaded uncached, owned here).
            foreach (Texture2D texture in overlayImageTextures)
                texture?.Dispose();
            overlayImageTextures.Clear();

            if (resolved.Count == 0)
            {
                overlayTexture?.Dispose();
                overlayTexture = null;
                return;
            }

            GraphicsDevice gd = WindowManager.GraphicsDevice;
            int mapWidth = worldMapTexture.Width;
            int mapHeight = worldMapTexture.Height;

            if (overlaySpriteBatch == null)
                overlaySpriteBatch = new SpriteBatch(gd);

            // Reuse the existing overlay render target when its size already matches the
            // current world map, so selecting missions does not repeatedly allocate a
            // (potentially very large) render target.
            if (overlayTexture == null || overlayTexture.Width != mapWidth || overlayTexture.Height != mapHeight)
            {
                overlayTexture?.Dispose();
                overlayTexture = new RenderTarget2D(gd, mapWidth, mapHeight,
                    false, SurfaceFormat.Color, DepthFormat.None);
            }

            gd.SetRenderTarget(overlayTexture);
            gd.Clear(Color.Transparent);

            overlaySpriteBatch.Begin();

            foreach (GlobeImage image in resolved)
            {
                Texture2D texture = AssetLoader.LoadTextureUncached(image.ImagePath);
                overlayImageTextures.Add(texture);

                // Same equirectangular projection the globe's texture and Location use:
                // u = (longitude + 180) / 360, v = (90 - latitude) / 180.
                float px = (float)((image.Longitude + 180.0) / 360.0) * mapWidth;
                float py = (float)((90.0 - image.Latitude) / 180.0) * mapHeight;

                float drawWidth;
                float drawHeight;
                if (image.HeightPercent.HasValue)
                {
                    drawHeight = image.HeightPercent.Value / 100.0f * mapHeight;
                    drawWidth = texture.Width * (drawHeight / texture.Height);
                }
                else
                {
                    drawWidth = texture.Width;
                    drawHeight = texture.Height;
                }

                overlaySpriteBatch.Draw(texture,
                    new Rectangle((int)(px - drawWidth / 2), (int)(py - drawHeight / 2),
                        (int)drawWidth, (int)drawHeight), Color.White);
            }

            overlaySpriteBatch.End();
            gd.SetRenderTarget(null);
        }

        /// <summary>
        /// Removes all globe overlay images, releasing the baked overlay texture.
        /// </summary>
        public void ClearImages()
        {
            foreach (Texture2D texture in overlayImageTextures)
                texture?.Dispose();
            overlayImageTextures.Clear();

            overlayTexture?.Dispose();
            overlayTexture = null;
        }


        private void CreateSphereGeometry()
        {
            int latCount = LAT_SEGMENTS + 1;
            int lonCount = LON_SEGMENTS + 1;
            sphereVertices = new VertexPositionNormalTexture[latCount * lonCount];

            for (int lat = 0; lat < latCount; lat++)
            {
                // theta from -90 (south) to +90 (north)
                float theta = -MathHelper.PiOver2 + lat * (MathHelper.Pi / LAT_SEGMENTS);

                for (int lon = 0; lon < lonCount; lon++)
                {
                    // phi from 0 to 2*pi (around the globe)
                    float phi = lon * (MathHelper.TwoPi / LON_SEGMENTS);

                    float x = (float)Math.Cos(theta) * (float)Math.Cos(phi);
                    float y = (float)Math.Sin(theta);
                    float z = (float)Math.Cos(theta) * (float)Math.Sin(phi);

                    var position = new Vector3(x, y, z);

                    // Texture coordinate: u wraps around longitude. u=0 is the dateline
                    // and u increases eastward, so that when the camera faces +Z the
                    // right side of the globe shows eastern longitudes and the left side
                    // shows western longitudes (no east-west mirror).
                    float u = 1.0f - (float)lon / LON_SEGMENTS;
                    float v = 1.0f - (float)lat / LAT_SEGMENTS;

                    sphereVertices[lat * lonCount + lon] = new VertexPositionNormalTexture(position, position, new Vector2(u, v));
                }
            }

            // Build the triangle index list.
            int quadCount = LAT_SEGMENTS * LON_SEGMENTS;
            sphereIndices = new int[quadCount * 6];

            int index = 0;
            for (int lat = 0; lat < LAT_SEGMENTS; lat++)
            {
                for (int lon = 0; lon < LON_SEGMENTS; lon++)
                {
                    int topLeft = lat * lonCount + lon;
                    int topRight = topLeft + 1;
                    int bottomLeft = (lat + 1) * lonCount + lon;
                    int bottomRight = bottomLeft + 1;

                    sphereIndices[index++] = topLeft;
                    sphereIndices[index++] = bottomLeft;
                    sphereIndices[index++] = topRight;

                    sphereIndices[index++] = topRight;
                    sphereIndices[index++] = bottomLeft;
                    sphereIndices[index++] = bottomRight;
                }
            }
        }

        private void Initialize3D()
        {
            if (worldMapTexture == null)
            {
                globeUnavailable = true;
                Logger.Log("RotatingEarthBackground: no world map texture is available; globe rendering is disabled.");
                return;
            }

            GraphicsDevice gd = WindowManager.GraphicsDevice;

            int renderSize = (int)(GlobeDiameter * RENDER_SCALE);
#if XNA
            // XNA Reach limits render targets to 2048 pixels per dimension.
            renderSize = Math.Min(renderSize, 2048);
#endif
            globeRenderTarget = new RenderTarget2D(gd, renderSize, renderSize,
                false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);

            sphereVertexBuffer = new VertexBuffer(gd, typeof(VertexPositionNormalTexture),
                sphereVertices.Length, BufferUsage.WriteOnly);
            sphereVertexBuffer.SetData(sphereVertices);

#if XNA
            // XNA's Reach profile does not support 32-bit indices. LON/LAT_SEGMENTS
            // are chosen so the sphere has at most 65536 vertices on XNA builds.
            short[] shortIndices = new short[sphereIndices.Length];
            for (int i = 0; i < sphereIndices.Length; i++)
                shortIndices[i] = (short)sphereIndices[i];

            sphereIndexBuffer = new IndexBuffer(gd, IndexElementSize.SixteenBits,
                shortIndices.Length, BufferUsage.WriteOnly);
            sphereIndexBuffer.SetData(shortIndices);
#else
            sphereIndexBuffer = new IndexBuffer(gd, IndexElementSize.ThirtyTwoBits,
                sphereIndices.Length, BufferUsage.WriteOnly);
            sphereIndexBuffer.SetData(sphereIndices);
#endif

            effect = new BasicEffect(gd);
            effect.TextureEnabled = true;
            effect.Texture = worldMapTexture;
            effect.PreferPerPixelLighting = false;
            effect.AmbientLightColor = new Vector3(0.60f, 0.60f, 0.62f);
            effect.DirectionalLight0.Enabled = true;
            effect.DirectionalLight0.DiffuseColor = Vector3.One;
            effect.DirectionalLight0.Direction = Vector3.Normalize(new Vector3(0.35f, 0.2f, -0.92f));
            effect.DirectionalLight1.Enabled = true;
            effect.DirectionalLight1.DiffuseColor = new Vector3(0.25f, 0.28f, 0.35f);
            effect.DirectionalLight1.Direction = Vector3.Normalize(new Vector3(-0.6f, -0.3f, 0.7f));
            effect.Projection = Matrix.CreatePerspectiveFieldOfView(
                CAMERA_FOV, 1.0f, 0.1f, 20.0f);
            effect.View = Matrix.CreateLookAt(new Vector3(0.0f, 0.0f, CAMERA_DISTANCE),
                Vector3.Zero, Vector3.Up);

            if (overlaySpriteBatch == null)
                overlaySpriteBatch = new SpriteBatch(gd);

            initialized3D = true;
        }

        public override void Update(GameTime gameTime)
        {
            if (!initialized3D && !globeUnavailable && Initialized)
                Initialize3D();

            // Advance the cross-fade transition if one is in progress.
            if (transitionProgress < 1.0f)
            {
                // Cap the elapsed time used per frame so a single slow (stuttering)
                // frame can't advance the whole cross-fade at once and cut the old map
                // (or overlay) off abruptly.
                const float MAX_TRANSITION_FRAME_TIME = 1.0f / 20.0f;
                float elapsed = (float)Math.Min(gameTime.ElapsedGameTime.TotalSeconds, MAX_TRANSITION_FRAME_TIME);
                transitionProgress = Math.Min(1.0f, transitionProgress + elapsed / TRANSITION_DURATION);
                if (transitionProgress >= 1.0f)
                {
                    previousWorldMapTexture = null;
                    previousWorldMapOverlayTexture = null;
                }
            }

            // Keep the auto-rotation angle current so a SetLocation call can start
            // smoothly from wherever the globe is right now.
            autoRotationYaw = (float)(ROTATION_SPEED * (gameTime.TotalGameTime.TotalSeconds - autoRotationTimeOffset));

            // Advance the camera-orientation transition.
            if (isTransitioning)
            {
                transitionElapsed += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (transitionElapsed >= LOCATION_TRANSITION_DURATION)
                {
                    transitionElapsed = LOCATION_TRANSITION_DURATION;
                    isTransitioning = false;

                    if (isReturning)
                    {
                        // Resume auto-rotation seamlessly from the returned position.
                        isReturning = false;
                        autoRotationTimeOffset = gameTime.TotalGameTime.TotalSeconds
                            - returnYaw / ROTATION_SPEED;
                        // Align the current frame's rotation angle so the final
                        // interpolated frame and the resumed rotation are identical.
                        autoRotationYaw = returnYaw;
                    }
                }
            }

            // Render the globe here (Update) because Draw runs while the UI sprite
            // batch and its render target are active, where switching render targets
            // would conflict with the active batch.
            if (initialized3D)
                RenderGlobe(gameTime);

            base.Update(gameTime);
        }

        /// <summary>
        /// Renders the globe for the current rotation angle into the render target.
        /// </summary>
        private void RenderGlobe(GameTime gameTime)
        {
            if (!initialized3D)
                return;

            float yaw;
            float latitudeRadians;
            Vector3 cameraPosition;

            if (fixedLongitude.HasValue && fixedLatitude.HasValue || isTransitioning)
            {
                // A transition is in progress (either flying to a location or returning
                // to the auto-rotation position). Interpolate between the current and
                // target orientation, and interpolate the FOV with the same eased
                // progress so the zoom speed matches the camera movement speed.
                float eased = GetTransitionEasedProgress();
                yaw = currentYaw + MathHelper.WrapAngle(targetYaw - currentYaw) * eased;
                latitudeRadians = currentLatitude + (targetLatitude - currentLatitude) * eased;
                currentFov = fovStart + (targetFov - fovStart) * eased;

                cameraPosition = new Vector3(
                    0.0f,
                    (float)Math.Sin(latitudeRadians) * CAMERA_DISTANCE,
                    (float)Math.Cos(latitudeRadians) * CAMERA_DISTANCE);
            }
            else
            {
                // Continuous unidirectional rotation about the vertical (Y) axis. This keeps
                // the equator perfectly horizontal so the map's equator aligns with the
                // globe's equator while the continents spin eastward smoothly.
                yaw = autoRotationYaw;
                cameraPosition = new Vector3(0.0f, 0.0f, CAMERA_DISTANCE);
                currentFov = targetFov;
            }

            effect.World = Matrix.CreateRotationY(yaw);
            effect.View = Matrix.CreateLookAt(cameraPosition, Vector3.Zero, Vector3.Up);
            effect.Projection = Matrix.CreatePerspectiveFieldOfView(
                currentFov, 1.0f, 0.1f, 20.0f);

            GraphicsDevice gd = WindowManager.GraphicsDevice;

            gd.SetRenderTarget(globeRenderTarget);
            gd.Clear(ClearOptions.Target | ClearOptions.DepthBuffer | ClearOptions.Stencil,
                Color.Transparent, 1.0f, 0);

            // CullNone so the outer surface of the globe is rendered. The sphere's
            // triangles are wound counter-clockwise (outward normals); using
            // CullCounterClockwise here would cull the outer faces and render the
            // inside of the sphere (mirrored, poles appearing wrong). Depth testing
            // automatically hides the far side.
            gd.RasterizerState = RasterizerState.CullNone;
            gd.DepthStencilState = sphereStencilState;

            if (transitionProgress < 1.0f && previousWorldMapTexture != null)
            {
                // Draw the old texture at full opacity first, then blend the new
                // texture on top with an alpha equal to the transition progress.
                gd.BlendState = BlendState.Opaque;
                DrawSphere(effect, previousWorldMapTexture, 1.0f);

                gd.BlendState = BlendState.AlphaBlend;
                DrawSphere(effect, worldMapTexture, transitionProgress);
            }
            else
            {
                gd.BlendState = BlendState.Opaque;
                DrawSphere(effect, worldMapTexture, 1.0f);
            }

            if (overlayTexture != null)
            {
                // Draw the baked geographic images over the globe's surface. The overlay
                // sphere shares the exact geometry, so its front faces blend at equal
                // depth while the far side stays hidden by the depth buffer; the images
                // therefore rotate with the earth map itself.
                gd.BlendState = BlendState.AlphaBlend;
                gd.DepthStencilState = DepthStencilState.Default;
                DrawSphere(effect, overlayTexture, 1.0f);
            }

            // The previous chapter's overlay fades out while the new chapter's overlay
            // fades in, both in sync with the world map cross-fade.
            if (previousWorldMapOverlayTexture != null)
            {
                float previousAlpha = WORLD_MAP_OVERLAY_ALPHA * (1.0f - transitionProgress);
                if (previousAlpha > 0.0f)
                    DrawOverlaySprite(previousWorldMapOverlayTexture, previousWorldMapOverlayScale, previousAlpha);
            }

            if (worldMapOverlayTexture != null)
            {
                // Draw the chapter overlay as a flat screen-space sprite clipped to the
                // globe's disc (via the stencil marked by sphereStencilState). Because it
                // is not transformed by the sphere's rotation, it stays fixed while the
                // earth rotates beneath it. It is sized so the overlay's bright ring lands
                // on the globe's circumference (HALO-style overlay images are square
                // canvases whose ring's bright edge sits at ~89.4% of the half-width, the
                // rest being a faint glow to the canvas edge). It fades in together with
                // the world map during a cross-fade.
                float overlayAlpha = transitionProgress < 1.0f
                    ? WORLD_MAP_OVERLAY_ALPHA * transitionProgress
                    : WORLD_MAP_OVERLAY_ALPHA;
                DrawOverlaySprite(worldMapOverlayTexture, worldMapOverlayScale, overlayAlpha);
            }

            gd.RasterizerState = RasterizerState.CullNone;
            gd.DepthStencilState = DepthStencilState.None;
            gd.BlendState = BlendState.AlphaBlend;
            gd.SetRenderTarget(null);
        }

        /// <summary>
        /// Draws a chapter overlay as a flat screen-space sprite clipped to the globe's
        /// disc, sized to the overlay's scale relative to the globe's circumference.
        /// </summary>
        private void DrawOverlaySprite(Texture2D texture, float scale, float alpha)
        {
            int discSize = (int)Math.Round(globeRenderTarget.Width * GLOBE_DISC_FRACTION / OVERLAY_RING_EDGE_FRACTION * scale);
            var discRect = new Rectangle(
                (globeRenderTarget.Width - discSize) / 2,
                (globeRenderTarget.Height - discSize) / 2,
                discSize, discSize);

            GraphicsDevice gd = WindowManager.GraphicsDevice;
            gd.DepthStencilState = overlayStencilState;
            gd.BlendState = BlendState.AlphaBlend;
            overlaySpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            overlaySpriteBatch.Draw(texture, discRect, Color.White * alpha);
            overlaySpriteBatch.End();
        }

        private void DrawSphere(BasicEffect effect, Texture2D texture, float alpha)
        {
            effect.Texture = texture;
            effect.Alpha = alpha;

            GraphicsDevice gd = WindowManager.GraphicsDevice;
            gd.SetVertexBuffer(sphereVertexBuffer);
            gd.Indices = sphereIndexBuffer;

            // XNA Reach supports at most 65535 primitives per draw call. The sphere has
            // more triangles than that, so draw it in chunks. This costs one extra draw
            // call on DX/GL and is required on XNA. Use 65000 to stay clear of the limit.
            const int MAX_PRIMITIVES_PER_DRAW = 65000;
            int totalPrimitives = sphereIndices.Length / 3;

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                for (int startPrimitive = 0; startPrimitive < totalPrimitives; startPrimitive += MAX_PRIMITIVES_PER_DRAW)
                {
                    int primitiveCount = Math.Min(MAX_PRIMITIVES_PER_DRAW, totalPrimitives - startPrimitive);
                    gd.DrawIndexedPrimitives(PrimitiveType.TriangleList,
                        0, 0, sphereVertexBuffer.VertexCount, startPrimitive * 3, primitiveCount);
                }
            }
        }

        public override void Draw(GameTime gameTime)
        {
            if (initialized3D)
            {
                int size = GlobeDiameter;
                int y = GlobeY ?? (Height - size) / 2;
                var rect = new Rectangle(
                    (Width - size) / 2,
                    y,
                    size,
                    size);
                DrawTexture(globeRenderTarget, rect, Color.White);
            }

            base.Draw(gameTime);
        }

        public override void Kill()
        {
            ClearImages();
            worldMapOverlayTexture = null;
            previousWorldMapOverlayTexture = null;
            foreach (Texture2D overlay in overlayCache.Values)
                overlay?.Dispose();
            overlayCache.Clear();
            overlaySpriteBatch?.Dispose();
            overlaySpriteBatch = null;
            effect?.Dispose();
            sphereVertexBuffer?.Dispose();
            sphereVertexBuffer = null;
            sphereIndexBuffer?.Dispose();
            sphereIndexBuffer = null;
            globeRenderTarget?.Dispose();
            sphereStencilState?.Dispose();
            overlayStencilState?.Dispose();
            base.Kill();
        }
    }
}
