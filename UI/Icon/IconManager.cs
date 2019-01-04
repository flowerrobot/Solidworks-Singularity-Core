using SingularityBase.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace SingularityCore.UI
{
    internal sealed class IconManager : IIconManager
    {
        //private static IconManager _iconManager;
        //public static IconManager GetIconManager => _iconManager ?? (_iconManager = new IconManager());

        public static string Dir = PluginLoader.WorkingPath;
        public NLog.Logger Logger { get; } = NLog.LogManager.GetLogger("IconManager");

        #region Image Path
        /// <inheritdoc />
        public string ImagePathSize20 { get; }
        /// <inheritdoc />
        public string ImagePathSize32 { get; }
        /// <inheritdoc />
        public string ImagePathSize40 { get; }
        /// <inheritdoc />
        public string ImagePathSize64 { get; }
        /// <inheritdoc />
        public string ImagePathSize96 { get; }
        /// <inheritdoc />
        public string ImagePathSize128 { get; }

        public string[] CmdImagePaths => new[] { ImagePathSize20, ImagePathSize32, ImagePathSize40, ImagePathSize64, ImagePathSize96, ImagePathSize128 };
        #endregion

        #region Addin Icon
        /// <inheritdoc />
        public string AddinIconSize20 { get; }

        /// <inheritdoc />
        public string AddinIconSize32 { get; }

        /// <inheritdoc />
        public string AddinIconSize40 { get; }

        /// <inheritdoc />
        public string AddinIconSize64 { get; }

        /// <inheritdoc />
        public string AddinIconSize96 { get; }

        /// <inheritdoc />
        public string AddinIconSize128 { get; }

        public string[] AddinIconPaths => new[] { AddinIconSize20, AddinIconSize32, AddinIconSize40, AddinIconSize64, AddinIconSize96, AddinIconSize128 };
        #endregion

        #region Default Images
        /// <inheritdoc />
        public Bitmap DefaultSize20 => SingularityCore.Properties.Resources.Singularity_20;
        /// <inheritdoc />
        public Bitmap DefaultSize32 => SingularityCore.Properties.Resources.Singularity_32;
        /// <inheritdoc />
        public Bitmap DefaultSize40 => SingularityCore.Properties.Resources.Singularity_40;
        /// <inheritdoc />
        public Bitmap DefaultSize64 => SingularityCore.Properties.Resources.Singularity_64;
        /// <inheritdoc />
        public Bitmap DefaultSize96 => SingularityCore.Properties.Resources.Singularity_96;
        /// <inheritdoc />
        public Bitmap DefaultSize128 => SingularityCore.Properties.Resources.Singularity_128;
        #endregion


        public List<ISingleCommandDef> Images { get; } = new List<ISingleCommandDef>();

        private const string IconName = "SingularityAddin";
        private const string AddInIconName = "SingularityIcon";
        internal IconManager()
        {
            try
            {
                if (!Directory.Exists(Dir))
                {
                    Directory.CreateDirectory(Dir);
                    if (!Directory.Exists(Dir)) { Logger.Error("Can not create working directory"); }
                }
                ImagePathSize20 = Path.Combine(Dir, IconName + "20" + ext);
                ImagePathSize32 = Path.Combine(Dir, IconName + "32" + ext);
                ImagePathSize40 = Path.Combine(Dir, IconName + "40" + ext);
                ImagePathSize64 = Path.Combine(Dir, IconName + "64" + ext);
                ImagePathSize96 = Path.Combine(Dir, IconName + "96" + ext);
                ImagePathSize128 = Path.Combine(Dir, IconName + "128" + ext);

                //Addin icon
                AddinIconSize20 = Path.Combine(Dir, AddInIconName + 20 + ext);
                AddinIconSize32 = Path.Combine(Dir, AddInIconName + 32 + ext);
                AddinIconSize40 = Path.Combine(Dir, AddInIconName + 40 + ext);
                AddinIconSize64 = Path.Combine(Dir, AddInIconName + 64 + ext);
                AddinIconSize96 = Path.Combine(Dir, AddInIconName + 96 + ext);
                AddinIconSize128 = Path.Combine(Dir, AddInIconName + 128 + ext);

                //Extract icons
                Properties.Resources.Singularity_20.Save(AddinIconSize20, ImageFormat.Bmp);
                Properties.Resources.Singularity_32.Save(AddinIconSize32, ImageFormat.Bmp);
                Properties.Resources.Singularity_40.Save(AddinIconSize40, ImageFormat.Bmp);
                Properties.Resources.Singularity_64.Save(AddinIconSize64, ImageFormat.Bmp);
                Properties.Resources.Singularity_96.Save(AddinIconSize96, ImageFormat.Bmp);
                Properties.Resources.Singularity_128.Save(AddinIconSize128, ImageFormat.Bmp);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error creating icons");
            }


            Logger.Trace("Large image {0}", ImagePathSize32);


        }


        private const string ext = ".bmp";
        //http://www.iconarchive.com/show/concave-icons-by-gakuseisean/Black-Internet-icon.html

        /// <inheritdoc />
        public bool AppendImages()
        {
            try
            {
                Logger.Trace("Appending {0} images", Images.Count());

                //Merge images together side by side by both image sizes

                int[] sizes = new[] { 20, 32, 40, 64, 96, 128 };
                Bitmap[] bm = new Bitmap[sizes.Length];
                Graphics[] gra = new Graphics[sizes.Length];

                try
                {
                    //initialise bit maps
                    for (int i = 0; i < sizes.Length; i++)
                    {
                        bm[i] = new Bitmap(sizes[i] * (1 + Images.Count()), sizes[i]);
                        gra[i] = Graphics.FromImage(bm[i]);
                    }

                    int count = 0;

                    foreach (ISingleCommandDef cmd in Images)
                    {
                        try
                        {
                            if (cmd.Command is ISwCommand swCmd)
                            {
                                IIconDef img = swCmd.Icons ?? new IconDef(swCmd);

                                ((SingleBaseCommand)cmd).IconIndex = count;
                                Bitmap[] iconBm = new[]
                                {
                                    img.ImageSize20 ?? DefaultSize20, img.ImageSize32 ?? DefaultSize32,
                                    img.ImageSize40 ?? DefaultSize40, img.ImageSize64 ?? DefaultSize64,
                                    img.ImageSize96 ?? DefaultSize96, img.ImageSize128 ?? DefaultSize128
                                };

                                for (int i = 0; i < sizes.Length; i++)
                                {
                                    gra[i].DrawImage(iconBm[i], count * sizes[i], 0, sizes[i], sizes[i]);
                                }
                            }

                            
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex, "ID :{0}", cmd.Id);
                        }
                        count += 1;
                    }
                }
                finally
                {
                    for (int i = 0; i < sizes.Length; i++)
                    {
                        gra[i]?.Dispose();
                        gra[i] = null;
                    }
                }

                for (int i = 0; i < sizes.Length; i++)
                    bm[i].Save(CmdImagePaths[i]);


                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            return false;
        }


        /// <inheritdoc />
        public string ExtractImage(Bitmap img)
        {
            if (img == null) return "";
            try
            {
                string name = Dir + Guid.NewGuid().ToString() + ext;
                img.Save(Dir);
                return name;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return "";
            }
        }
    }
}
