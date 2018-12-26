using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using SingularityBase.UI.Icons;

namespace SingleCore.UI.Icons
{
    public sealed class IconManager : IIconManager
    {
        private static IconManager _iconManager;
        public static IconManager GetIconManager => _iconManager ?? (_iconManager = new IconManager());

        public static string Dir = ModuleLoader.ModuleLoader.WorkingPath;
        public NLog.Logger Logger { get; } = NLog.LogManager.GetLogger("IconManager");
        /// <inheritdoc />
        public string ImagePathSize32 { get; private set; }
        /// <inheritdoc />
        public string ImagePathSize16 { get; private set; }

        /// <inheritdoc />
        public string AddinIconSize32 { get; private set; }
        /// <inheritdoc />
        public string AddinIconSize16 { get; private set; }

        public List<IIconDef> Images { get; } = new List<IIconDef>();

        private IconManager()
        {

            ImagePathSize32 = Path.Combine(Dir, "SolidworksAddinLargeImage" + ext);
            ImagePathSize16 = Path.Combine(Dir, "SolidworksAddinSmallImage" + ext);

            Logger.Trace("Large image {0}", ImagePathSize32);
            Logger.Trace("Small Image {0}", ImagePathSize16);


        }

        const int largeSize = 24;
        const int smallSize = 16;
        const string ext = ".bmp";

        /// <inheritdoc />
        public bool AppendImages()
        {
            try
            {
                Logger.Trace("Appending {0} images", Images.Count());
                //Merge images together side by side by both image sizes
                Bitmap lrg = new Bitmap(largeSize * (1 + Images.Count()), largeSize);
                Bitmap sml = new Bitmap(smallSize * (1 + Images.Count()), smallSize);

                int count = 0;
                using (Graphics lrgGra = Graphics.FromImage(lrg))
                {
                    using (Graphics smlGra = Graphics.FromImage(sml))
                    {
                        foreach (IconDef img in Images)
                        {
                            try
                            {
                                img.Index = count;
                                Bitmap cmdsm = img.ImageSize16 ?? DefaultSize16;
                                Bitmap cmdlrg = img.ImageSize32 ?? DefaultSize32;

                                lrgGra.DrawImage(cmdsm, count * largeSize, 0, largeSize, largeSize);
                                smlGra.DrawImage(cmdlrg, count * smallSize, 0, smallSize, smallSize);

                                count += 1;
                            }
                            catch (Exception ex)
                            {
                                Logger.Error(ex, "ID :{0}", img.Owner.Id);
                            }
                        }
                    }
                }
                lrg.Save(ImagePathSize32);
                sml.Save(ImagePathSize16);

                return true;
            }
            catch (Exception ex)
            {
                //TO DO output to log
                Logger.Error(ex);
            }
            return false;
        }
        /// <inheritdoc />
        public Bitmap DefaultSize16 => Properties.Resources.OutotecO_16;
        /// <inheritdoc />
        public Bitmap DefaultSize32 => Properties.Resources.OutotecO_24;
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
