using Microsoft.Win32;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace MARGO
{
    class MsgBox
    {
        private string myLastPath =
#if DEBUG
        @"D:\Segment\"
#else
        "::{20D04FE0-3AEA-1069-A2D8-08002B30309D}"
#endif
;
        public IEnumerable<string> GetLayerFiles()
        {
            var ids = Enumerable.Empty<string>();

            OpenFileDialog ofd =
                           new OpenFileDialog()
                           {
                               InitialDirectory = myLastPath,
                               Filter = @"GeoTIFF files(*.tif)|*.tif",
                               Multiselect = true,
                               Title = "Open"
                           };

            if (ofd.ShowDialog().Value)
            {
                myLastPath = Path.GetDirectoryName(ofd.FileName);

                ids = ofd.FileNames;
            }

            return ids;
        }

        public string GetSavePath()
        {
            string path = string.Empty;

            SaveFileDialog sfd
                 = new SaveFileDialog()
                 {
                     InitialDirectory = myLastPath,
                     Filter = @"(*.tif)|*.tif",
                     Title = "Save"
                 };

            if (sfd.ShowDialog().Value)
            {
                myLastPath = Path.GetDirectoryName(sfd.FileName);

                path = sfd.FileName;
            }

            return path;
        }

        public void ShowInfo(string messageBoxText, string caption = "Info")
            => MessageBox.Show(messageBoxText, caption, MessageBoxButton.OK, MessageBoxImage.Information);
    }
}