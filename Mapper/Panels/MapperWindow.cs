using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ColossalFramework;
using ColossalFramework.Math;
using ColossalFramework.UI;
using System.Reflection;
using System.Timers;
using UnityEngine;
using System.IO;

namespace Mapper
{
    public class MapperWindow7 : UIPanel
    {
        UILabel title;

        UITextField pathTextBox;
        UILabel pathTextBoxLabel;
        UIButton loadMapButton;

        UITextField coordinates;
        UILabel coordinatesLabel;
        UIButton loadAPIButton;

        UILabel informationLabel;

        UIButton pedestriansCheck;
        UILabel pedestrianLabel;

        UIButton roadsCheck;
        UILabel roadsLabel;

        UIButton highwaysCheck;
        UILabel highwaysLabel;

        UITextField scaleTextBox;
        UILabel scaleTextBoxLabel;

        UITextField tolerance;
        UILabel toleranceLabel;

        UITextField curveTolerance;
        UILabel curveToleranceLabel;

        UITextField tiles;
        UILabel tilesLabel;

        UILabel errorLabel;

        UIButton okButton;

        UIButton exportButton;

        public ICities.LoadMode mode;
        RoadMaker2 roadMaker;
        bool createRoads;
        int currentIndex = 0;
         bool peds = true;
         bool roads = true;
         bool highways = true;

        public override void Awake()
        {
            this.isInteractive = true;
            this.enabled = true;
            
            width = 500;

            title = AddUIComponent<UILabel>();

            coordinates = AddUIComponent<UITextField>();
            coordinatesLabel = AddUIComponent<UILabel>();
            loadAPIButton = AddUIComponent<UIButton>();

            pathTextBox = AddUIComponent<UITextField>();
            pathTextBoxLabel = AddUIComponent<UILabel>();
            loadMapButton = AddUIComponent<UIButton>();

            informationLabel = AddUIComponent<UILabel>();

            pedestriansCheck = AddUIComponent<UIButton>();
            pedestrianLabel = AddUIComponent<UILabel>();
            roadsCheck = AddUIComponent<UIButton>();
            roadsLabel = AddUIComponent<UILabel>();
            highwaysCheck = AddUIComponent<UIButton>();
            highwaysLabel = AddUIComponent<UILabel>();

            scaleTextBox = AddUIComponent<UITextField>();
            scaleTextBoxLabel = AddUIComponent<UILabel>();


            tolerance = AddUIComponent<UITextField>();
            toleranceLabel = AddUIComponent<UILabel>();

            curveTolerance = AddUIComponent<UITextField>();
            curveToleranceLabel = AddUIComponent<UILabel>();

            tiles = AddUIComponent<UITextField>();
            tilesLabel = AddUIComponent<UILabel>();

            errorLabel = AddUIComponent<UILabel>();

            okButton = AddUIComponent<UIButton>();

            exportButton = AddUIComponent<UIButton>();
            base.Awake();

        }
        public override void Start()
        {
            base.Start();

            relativePosition = new Vector3(396, 58);
            backgroundSprite = "MenuPanel2";
            isInteractive = true;
            //this.CenterToParent();
            SetupControls();
        }

        public void SetupControls()
        {
            

            title.text = "Open Street Map Import";
            title.relativePosition = new Vector3(15, 15);
            title.textScale = 0.9f;
            title.size = new Vector2(200, 30);
            var vertPadding = 30;
            var x = 15;
            var y = 50;

            SetLabel(pedestrianLabel, "Pedestrian Paths", x, y);
            SetButton(pedestriansCheck, "True", x + 114, y);
            pedestriansCheck.eventClick +=pedestriansCheck_eventClick;
            x += 190;
            SetLabel(roadsLabel, "Roads", x, y);
            SetButton(roadsCheck, "True", x + 80, y);
            roadsCheck.eventClick += roadsCheck_eventClick;
            x += 140;
            SetLabel(highwaysLabel, "Highways", x, y);
            SetButton(highwaysCheck, "True", x + 80, y);
            highwaysCheck.eventClick += highwaysCheck_eventClick;

            x = 15;
            y += vertPadding;

            SetLabel(scaleTextBoxLabel, "Scale", x, y);
            SetTextBox(scaleTextBox, "1", x + 120, y);
            y += vertPadding;


            SetLabel(toleranceLabel, "Tolerance", x, y);
            SetTextBox(tolerance, "6", x + 120, y);
            y += vertPadding;

            SetLabel(curveToleranceLabel, "Curve Tolerance", x, y);
            SetTextBox(curveTolerance, "6", x + 120, y);
            y += vertPadding;

            SetLabel(tilesLabel, "Tiles to Boundary", x, y);
            SetTextBox(tiles, "3.5", x + 120, y);
            y += vertPadding + 12;

            SetLabel(pathTextBoxLabel, "Path", x, y);
            SetTextBox(pathTextBox, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "map"), x + 120, y);
            y += vertPadding - 5;
            SetButton(loadMapButton, "Load OSM From File", y);
            loadMapButton.eventClick += loadMapButton_eventClick;
            y += vertPadding + 5;

            SetLabel(coordinatesLabel, "Bounding Box", x, y);
            SetTextBox(coordinates, "35.949310,34.522050,35.753054,34.360353", x + 120, y);
            y += vertPadding - 5;
            SetButton(loadAPIButton, "Load From http://overpass-api.de/", y);
            loadAPIButton.eventClick += loadAPIButton_eventClick;
            y += vertPadding + 5;


            SetLabel(errorLabel, "No OSM data loaded.", x, y);
            y += vertPadding + 12;

            SetButton(okButton, "Make Roads", y);
            okButton.eventClick += okButton_eventClick;
            okButton.Disable();
            y += vertPadding;

            SetButton(exportButton, "Export City to .osm", y);
            exportButton.eventClick +=exportButton_eventClick;
            height = y + vertPadding + 6;
        }

        private void exportButton_eventClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            var export = new OSMExport();
            export.Export();
        }

        private void highwaysCheck_eventClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            highways = !highways;
            highwaysCheck.text = highways.ToString();
        }

        private void roadsCheck_eventClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            roads = !roads;
            roadsCheck.text = roads.ToString();
        }

        private void pedestriansCheck_eventClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            peds = !peds;
            pedestriansCheck.text = peds.ToString();
        }

        private void loadAPIButton_eventClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            try
            {
                var text = coordinates.text.Trim();
                var split = text.Split(new string[]{","},StringSplitOptions.RemoveEmptyEntries);
                decimal startLat;
                decimal startLon;
                decimal endLat;
                decimal endLon;
                if (split.Count() != 4 || !decimal.TryParse(split[0], out startLon) || !decimal.TryParse(split[1], out startLat) || !decimal.TryParse(split[2], out endLon) || !decimal.TryParse(split[3], out endLat))
                {
                    errorLabel.text = "Coordinate format wrong!";
                    return;
                }
                if (startLat > endLat)
                {
                    var temp = startLat;
                    startLat = endLat;
                    endLat = temp;
                }

                if (startLon > endLon)
                {
                    var temp = startLon;
                    startLon = endLon;
                    endLon = temp;
                }


                var ob = new osmBounds();
                ob.minlon = startLon;
                ob.minlat = startLat;
                ob.maxlon = endLon;
                ob.maxlat = endLat;

                var osm = new OSMInterface(ob, double.Parse(scaleTextBox.text.Trim()), double.Parse(tolerance.text.Trim()), double.Parse(curveTolerance.text.Trim()), double.Parse(tiles.text.Trim()));
                roadMaker = new RoadMaker2(osm);
                errorLabel.text = "Data Loaded.";
                okButton.Enable();
                loadMapButton.Disable();
                loadAPIButton.Disable();
            }
            catch (Exception ex)
            {
                errorLabel.text = ex.ToString();
            }
        }

        private void loadMapButton_eventClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            var path = pathTextBox.text.Trim();
            if (!File.Exists(path))
            {
                path += ".osm";
                if (!File.Exists(path))
                {
                    errorLabel.text = "Cannot find osm file: " + path;
                    return;
                }
            }
            try
            {
                var osm = new OSMInterface(pathTextBox.text.Trim(), double.Parse(scaleTextBox.text.Trim()), double.Parse(tolerance.text.Trim()), double.Parse(curveTolerance.text.Trim()), double.Parse(tiles.text.Trim()));
                roadMaker = new RoadMaker2(osm);
                errorLabel.text = "File Loaded.";
                okButton.Enable();
                loadMapButton.Disable();
                loadAPIButton.Disable();
            }
            catch (Exception ex)
            {
                errorLabel.text = ex.ToString();
            }
        }

        private void SetButton(UIButton okButton, string p1,int x, int y)
        {
            okButton.text = p1;
            okButton.normalBgSprite = "ButtonMenu";
            okButton.hoveredBgSprite = "ButtonMenuHovered";
            okButton.disabledBgSprite = "ButtonMenuDisabled";
            okButton.focusedBgSprite = "ButtonMenuFocused";
            okButton.pressedBgSprite = "ButtonMenuPressed";
            okButton.size = new Vector2(50, 18);
            okButton.relativePosition = new Vector3(x, y - 3);
            okButton.textScale = 0.8f;
        }

        private void SetButton(UIButton okButton, string p1, int y)
        {
            okButton.text = p1;
            okButton.normalBgSprite = "ButtonMenu";
            okButton.hoveredBgSprite = "ButtonMenuHovered";
            okButton.disabledBgSprite = "ButtonMenuDisabled";
            okButton.focusedBgSprite = "ButtonMenuFocused";
            okButton.pressedBgSprite = "ButtonMenuPressed";
            okButton.size = new Vector2(260, 24);
            okButton.relativePosition = new Vector3((int)(width - okButton.size.x) / 2,y);
            okButton.textScale = 0.8f;
        }

        private void SetCheckBox(UICustomCheckbox3 pedestriansCheck, int x, int y)
        {

            pedestriansCheck.IsChecked = true;
            pedestriansCheck.relativePosition = new Vector3(x, y);
            pedestriansCheck.size = new Vector2(13, 13);
            pedestriansCheck.Show();
            pedestriansCheck.color = new Color32(185, 221, 254, 255);
            pedestriansCheck.enabled = true;            
            pedestriansCheck.spriteName = "AchievementCheckedFalse";
            pedestriansCheck.eventClick += (component, param) =>
            {
                pedestriansCheck.IsChecked = !pedestriansCheck.IsChecked;
            };
        }

        private void SetTextBox(UITextField scaleTextBox, string p, int x, int y)
        {
            scaleTextBox.relativePosition = new Vector3(x, y - 4);
            scaleTextBox.horizontalAlignment = UIHorizontalAlignment.Left;
            scaleTextBox.text = p;
            scaleTextBox.textScale = 0.8f;
            scaleTextBox.color = Color.black;
            scaleTextBox.cursorBlinkTime = 0.45f;
            scaleTextBox.cursorWidth = 1;
            scaleTextBox.verticalAlignment = UIVerticalAlignment.Middle;
            scaleTextBox.padding = new RectOffset(5, 0, 5, 0);
            scaleTextBox.foregroundSpriteMode = UIForegroundSpriteMode.Fill;
            scaleTextBox.normalBgSprite = "TextFieldPanel";
            scaleTextBox.hoveredBgSprite = "TextFieldPanelHovered";
            scaleTextBox.focusedBgSprite = "TextFieldPanel";
            scaleTextBox.size = new Vector3(width - 120 - 30, 20);
            scaleTextBox.isInteractive = true;
            scaleTextBox.enabled = true;
            scaleTextBox.readOnly = false;
            scaleTextBox.builtinKeyNavigation = true;            
        }

        private void SetLabel(UILabel pedestrianLabel, string p, int x, int y)
        {
            pedestrianLabel.relativePosition = new Vector3(x, y);
            pedestrianLabel.text = p;
            pedestrianLabel.textScale = 0.8f;
            pedestrianLabel.size = new Vector3(120,20);
        }

        private void okButton_eventClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            if (roadMaker != null)
            {
                currentIndex = 0;
                createRoads = !createRoads;
            }
        }

        public override void Update()
        {
            if (createRoads)
            {
                var pp = peds;
                var rr = roads;
                var hh = highways;                
                if (currentIndex < roadMaker.osm.ways.Count())
                {
                    //roadMaker.MakeRoad(currentIndex);
                    SimulationManager.instance.AddAction(roadMaker.MakeRoad(currentIndex,pp,rr,hh));
                    currentIndex += 1;
                }

                if (currentIndex < roadMaker.osm.ways.Count())
                {
                    //roadMaker.MakeRoad(currentIndex);
                    SimulationManager.instance.AddAction(roadMaker.MakeRoad(currentIndex, pp, rr, hh));
                    currentIndex += 1;
                }

                if (currentIndex < roadMaker.osm.ways.Count())
                {
                    //roadMaker.MakeRoad(currentIndex);
                    SimulationManager.instance.AddAction(roadMaker.MakeRoad(currentIndex, pp, rr, hh));
                    currentIndex += 1;
                    var instance = Singleton<NetManager>.instance;
                    errorLabel.text = String.Format("Making road {0} out of {1}. Nodes: {2}. Segments: {3}", currentIndex, roadMaker.osm.ways.Count(), instance.m_nodeCount, instance.m_segmentCount);
                }
            }

            if (roadMaker != null && currentIndex == roadMaker.osm.ways.Count())
            {
                errorLabel.text = "Done.";
                createRoads = false;
            }
            base.Update();
        }
    }

    public class UICustomCheckbox3 : UISprite
    {
        public bool IsChecked { get; set; }

        public override void Start()
        {
            base.Start();
            IsChecked = true;
            spriteName = "AchievementCheckedTrue";
        }

        public override void Update()
        {
            base.Update();
            spriteName = IsChecked ? "AchievementCheckedTrue" : "AchievementCheckedFalse";
        }
    }    
}
