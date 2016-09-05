using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace ThinkGeo.MapSuite.Core
{
    public class IndexRasterLayer : RasterLayer
    {
        private bool isOpen;
        private string idxPathFileName;
        private RtreeSpatialIndex rTreeIndex;
        private Collection<MrSidRasterLayer> rasterLayers;

        public IndexRasterLayer()
            : this(new Collection<MrSidRasterLayer>(), string.Empty)
        { }

        public IndexRasterLayer(string idxPathFileNam)
     : this(new Collection<MrSidRasterLayer>(), idxPathFileNam)
        { }

        public IndexRasterLayer(IEnumerable<MrSidRasterLayer> rasterLayers)
            : this(rasterLayers, string.Empty)
        { }

        public IndexRasterLayer(IEnumerable<MrSidRasterLayer> rasterLayers, string idxPathFileName)
            : base()
        {
            this.idxPathFileName = idxPathFileName;

            this.rasterLayers = new Collection<MrSidRasterLayer>();
            foreach (var layer in rasterLayers)
            {
                this.rasterLayers.Add(layer);
            }
        }

        public string IndexFilePath
        {
            set { this.idxPathFileName = value; }
            get { return this.idxPathFileName; }
        }

        public Collection<MrSidRasterLayer> RasterLayers
        {
            set { this.rasterLayers = value; }
            get { return this.rasterLayers; }
        }


        public override bool HasBoundingBox
        {
            get { return true; }
        }

        protected override bool IsOpenCore
        {
            get { return isOpen; }
        }

        protected override void OpenCore()
        {
            if (!IsOpenCore)
            {
                isOpen = true;

                if (!File.Exists(idxPathFileName))
                {
                    RtreeSpatialIndex.CreateRectangleSpatialIndex(idxPathFileName, RtreeSpatialIndexPageSize.EightKilobytes, RtreeSpatialIndexDataFormat.Float);

                    rTreeIndex = new RtreeSpatialIndex(idxPathFileName, RtreeSpatialIndexReadWriteMode.ReadWrite);
                    rTreeIndex.Open();

                    foreach (var rasterLayer in rasterLayers)
                    {
                        rasterLayer.Open();
                        RectangleShape boudingBox = rasterLayer.GetBoundingBox();

                        Feature feature = new Feature(boudingBox);
                        feature.Id = rasterLayer.PathFilename;
                        rTreeIndex.Add(feature);
                    }
                }
                else
                {
                    rTreeIndex = new RtreeSpatialIndex(idxPathFileName, RtreeSpatialIndexReadWriteMode.ReadOnly);
                    rTreeIndex.Open();
                }
            }
        }

        protected override void DrawCore(GeoCanvas canvas, Collection<SimpleCandidate> labelsInAllLayers)
        {
            Collection<string> ids = rTreeIndex.GetFeatureIdsIntersectingBoundingBox(canvas.CurrentWorldExtent);

            var layers = rasterLayers.Where(a => ids.Contains(a.PathFilename));

            foreach (var layer in layers)
            {
                layer.Open();
                layer.Draw(canvas, labelsInAllLayers);
            }
        }

        protected override RectangleShape GetBoundingBoxCore()
        {
            Collection<RectangleShape> boundingBoxs = new Collection<RectangleShape>();

            foreach (var rasterLayer in rasterLayers)
            {
                rasterLayer.Open();
                boundingBoxs.Add(rasterLayer.GetBoundingBox());
            }

            return ExtentHelper.GetBoundingBoxOfItems(boundingBoxs);
        }
    }
}
