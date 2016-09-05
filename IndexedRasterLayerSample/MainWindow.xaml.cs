using System.IO;
using System.Windows;
using System.Windows.Forms;
using ThinkGeo.MapSuite.Core;
using ThinkGeo.MapSuite.WpfDesktopEdition;

namespace IndexedRasterLayerSample
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            map.MapUnit = GeographyUnit.Meter;
        }

        private void LoadButtonClick(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                tbFolder.Text = folderBrowserDialog.SelectedPath;
            }

            if (!string.IsNullOrEmpty(tbFolder.Text))
            {
                IndexRasterLayer indexRasterLayer = new IndexRasterLayer(@"..\..\index.idx");
                string[] files = Directory.GetFiles(tbFolder.Text);

                foreach (var file in files)
                {
                    indexRasterLayer.RasterLayers.Add(new MrSidRasterLayer(file));
                }

                LayerOverlay layerOverlay = new LayerOverlay();
                layerOverlay.Layers.Add(indexRasterLayer);
                map.Overlays.Add(layerOverlay);

                indexRasterLayer.Open();
                map.CurrentExtent = indexRasterLayer.GetBoundingBox();

                map.Refresh();
            }
        }
    }
}
