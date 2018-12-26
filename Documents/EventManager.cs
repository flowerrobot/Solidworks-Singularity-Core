using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SingularityBase;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;

namespace SingleCore.Events
{
    public static class EventManager
    {
        /// <summary>
        /// Gets the document events.
        /// </summary>
        /// <param name="swModel">The sw model.</param>
        /// <returns></returns>
        public static CommonDocumentEvents GetDocumentEvents(ModelDoc2 swModel)
        {
            if (!DocumentEvents.ContainsKey(swModel))
                DocumentEvents.Add(swModel, new CommonDocumentEvents(ref swModel));
            
            return DocumentEvents[swModel];
        }
        private static Dictionary<ModelDoc2,CommonDocumentEvents> DocumentEvents { get; } = new Dictionary<ModelDoc2, CommonDocumentEvents>(); 


        private static readonly List<EventSupressor> Events = new List<EventSupressor>();
        public static ReadOnlyCollection<EventSupressor> AllEventsSupressors { get; private set; } = Events.AsReadOnly();


        /// <summary>
        /// Will inform if any events are blocked at the All or SWapp Level.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is suppressed; otherwise, <c>false</c>.
        /// </value>
        public static bool ShouldRaise => AllEventsSupressors.Any(item => item.Level == EventLevel.All || item.Level == EventLevel.SwEvents);

        public static EventSupressor SuppressEvents(EventLevel level)
        {
            EventSupressor item = new EventSupressor() { Level = level };
            Events.Add(item);
            AllEventsSupressors = Events.AsReadOnly();
            return item;
        }

        internal static void Unsupress(EventSupressor item)
        {
            Events.Remove(item);
            AllEventsSupressors = Events.AsReadOnly();
        }

        public static void Initlise(SldWorks swApp)
        {
            swApp.CommandOpenPreNotify += ComandStartPreNotify;
            swApp.CommandCloseNotify += CommandEndNotify;

            swApp.FileCloseNotify += FileClosing_Notify;
            swApp.ActiveDocChangeNotify += ActiveDocChanged;

            swApp.FileNewNotify2 += NewFileOpened;
            swApp.FileNewPreNotify += NewFilePreopened;

            swApp.FileOpenPreNotify += FileOpenPre;
            swApp.FileOpenNotify2 += FileOpen;
            swApp.FileOpenPostNotify += FileOpenPostNotify;

        }

        #region SWapp Events
        public static event NewFilePre newfilePre;
        public delegate int NewFilePre(swDocumentTypes_e doctype, string templatename);
        private static int NewFilePreopened(int doctype, string templatename)
        {
            if (ShouldRaise)
                return newfilePre?.Invoke((swDocumentTypes_e)doctype, templatename) ?? 0;
            return 0;
        }

        public static event ActiveDocChange ActivedoChanged;
        public delegate int ActiveDocChange();
        private static int ActiveDocChanged()
        {
            if (ShouldRaise)
                return ActivedoChanged?.Invoke() ?? 0;
            return 0;

        }

        public static event FileOpeningPost fileOpeningPost;
        public delegate int FileOpeningPost(ModelDoc2 newDoc, swDocumentTypes_e docType, string templateName);
        private static int NewFileOpened(object newdoc, int doctype, string templatename)
        {
            if (ShouldRaise)
                return fileOpeningPost?.Invoke((ModelDoc2)newdoc, (swDocumentTypes_e)doctype, templatename) ??0;
            return 0;
        }

        public static event FileClosing fileClosing;
        public delegate int FileClosing(string fileName, swFileCloseNotifyReason_e reason);
        private static int FileClosing_Notify(string filename, int reason)
        {
            if (ShouldRaise)
                return fileClosing?.Invoke(filename, (swFileCloseNotifyReason_e)reason) ?? 1;
            return 1;
        }



        public static event ComandStartPre comandStartPre;
        public delegate int ComandStartPre(swCommand_e command, int usercommand);
        private static int ComandStartPreNotify(int command, int usercommand)
        {
            if (ShouldRaise)
                return comandStartPre?.Invoke((swCommand_e)command, usercommand) ?? 0;
            return 0;
        }

        public static event CommandClose commandClosePost;
        public delegate int CommandClose(swCommand_e command, swPropertyManagerPageCloseReasons_e reason);
        private static int CommandEndNotify(int command, int reason)
        {
            if (ShouldRaise)
                return commandClosePost?.Invoke((swCommand_e)command, (swPropertyManagerPageCloseReasons_e)reason) ?? 0;
            return 0;
        }

        internal static EventSupressor SuppressEvents(EventLevel level)
        {
            throw new NotImplementedException();
        }

        public static event FileOpeningPre fileOpenPre;
        public delegate int FileOpeningPre(string fileName);
        private static int FileOpenPre(string filename)
        {
            if (ShouldRaise)
                return fileOpenPre?.Invoke(filename) ?? 1;
            return 1;
        }

        public static event FileOpening fileOpening;
        public delegate int FileOpening(string filename);
        private static int FileOpen(string filename)
        {
            if (ShouldRaise)
                return fileOpening?.Invoke(filename) ?? 1;
            return 1;
        }

        public static event FileOpenPost fileOpenPost;
        public delegate int FileOpenPost(string filename);
        private static int FileOpenPostNotify(string filename)
        {
            if (ShouldRaise)
                return fileOpenPost?.Invoke(filename) ?? 1;
            return 1;
        }
        #endregion

       
    }
   
}
