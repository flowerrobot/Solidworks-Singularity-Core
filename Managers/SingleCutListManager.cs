using SingularityBase;
using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Linq;
using SolidWorks.Interop.swconst;

namespace SingularityCore
{
    internal class SingleCutListManager : ISingleCutListManager
    {
       
        public SingleCutListManager(ISinglePartDoc doc)
        {
            Document = doc;
        }
        public ISinglePartDoc Document { get; }

        
        public IList<ISingleCutListFolder> CutListFolders
        {
            get {
                List<ISingleCutListFolder> lst = new List<ISingleCutListFolder>();

                bool found = false;
                ISingleFeature feat = Document.FeatureManager.GetFirstFeature;

                while (feat != null)
                {
                    switch (feat.GetTypeName)
                    {
                        
                        case FeatureName.CutListFolder:
                            lst.Add((ISingleCutListFolder)feat.GetSpecificFeature);
                            found = true;
                            break;
                        default:
                            if (found) return lst;
                            break;
                    }

                    feat = feat.GetNextFeature;
                }
                return lst;
            }
        }

        public int CutListCount => ((List<ISingleCutListFolder>)CutListFolders).Count;


        public bool UpdateCutList() => UpdateCutList(false);
        public bool UpdateCutList(bool rebuildFlatPattern)
        {
            if (rebuildFlatPattern)
            {

                using (IDisposable a = Document.ActiveView.DisableGraphicsUpdate)
                {
                    foreach (IFeature fea in Document.FeatureManager.GetFeatures(false))
                    {
                        if (fea.GetTypeName2().Equals(FeatureName.FlatPattern.ToString(), StringComparison.CurrentCultureIgnoreCase))
                        {
                            if (Document.EditUnsuppress(new[] { fea }))
                            {
                                Document.ForceRebuild(true);
                                Document.EditSuppress(new[] { fea });
                            }
                        }
                    }
                }

            }
            bool res = true;
            foreach (ISingleCutListFolder var in CutListFolders)
            {
                res = res && var.UpdateCutList();
            }

            return res;
        }

        /// <summary>
        /// Will add a property for all cut list items
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <param name="overrideIfExisting"></param>
        public void AddCustomProperty(string name, swCustomInfoType_e type,string value, swCustomPropertyAddOption_e overrideIfExisting)
        {
            
            foreach (var fld in CutListFolders)
            {
                fld.Add(name, type, value, overrideIfExisting);
            }
        }
        public void DeleteCustomProperty(string name)
        {
            foreach (var fld in CutListFolders)
            {
                fld.Delete(name);
            }
        }
        public void DeleteAllCustomProperties()
        {
            foreach (var fld in CutListFolders)
            {
                fld.DeleteAll();
            }
        }

        public IDictionary<ISingleBody, ISingleCutListFolder> GetCutListFolders(IList<ISingleBody> bodies)
        {
            IDictionary<ISingleBody, ISingleCutListFolder> res = new Dictionary<ISingleBody, ISingleCutListFolder>();
            var flders = CutListFolders;

            foreach (ISingleBody bod in bodies)
            {
                foreach (ISingleCutListFolder folder in flders)
                {

                    if (folder.GetBodies.Any(t => t.Equals(bod)))
                    {
                        res.Add(bod, folder);
                    }
                }
            }

            return res;
        }
    }
}
