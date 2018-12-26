
using System.Drawing;
using SingularityBase;
using SingularityBase.UI.Icons;

namespace SingleCore.UI.Icons
{
    /// <summary>
    /// Defines image definition for a button class
    /// Best to have the icons stored as Bitmap resouces and simply pass that here
    /// </summary>
    public sealed class IconDef : IIconDef
    {
        public IconDef(ISwBaseFunction owner, Bitmap lrgImage = null, Bitmap smlImage = null)
        {

            ImageSize32 = lrgImage ?? IconManager.GetIconManager.DefaultSize32;
            ImageSize16 = smlImage ?? IconManager.GetIconManager.DefaultSize16;
            Owner = owner;
        }
        /// <summary>
        /// The small icon size 16. ie Properties.Resouces.MySmallIcon;
        /// </summary>
        public Bitmap ImageSize16 { get;  set; }  
        /// <summary>
        /// The large icon size 24. ie Properties.Resouces.MyLargeIcon;
        /// </summary>
        public Bitmap ImageSize32 { get;  set; } 
        /// <summary>
        /// THe postion found on the combined tool bar
        /// </summary>
        public int Index { get; internal set; }
        /// <summary>
        /// Module who owns this icon profile
        /// </summary>
        public ISwBaseFunction Owner { get; internal set; }

        
    }
}
