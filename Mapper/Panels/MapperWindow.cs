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
    public class MapperWindow : UIPanel
    {
        
        UIButton okButton;
        UILabel title;
        UICustomCheckbox3 pedestriansCheck;
        UILabel pedestrianLabel;

        UITextField scaleTextBox;
        UILabel scaleTextBoxLabel;

        UITextField pathTextBox;
        UILabel pathTextBoxLabel;

        UITextField tolerance;
        UILabel toleranceLabel;

        UITextField curveTolerance;
        UILabel curveToleranceLabel;

        UITextField tiles;
        UILabel tilesLabel;

        UILabel errorLabel;

        public ICities.LoadMode mode;
        RoadMaker roadMaker;
        bool createRoads;
        int currentIndex = 0;

        public override void Awake()
        {
            title = AddUIComponent<UILabel>();
            pedestriansCheck = AddUIComponent<UICustomCheckbox3>();
            pedestrianLabel = AddUIComponent<UILabel>();

            scaleTextBox = AddUIComponent<UITextField>();
            scaleTextBoxLabel = AddUIComponent<UILabel>();

            pathTextBox = AddUIComponent<UITextField>();
            pathTextBoxLabel = AddUIComponent<UILabel>();

            tolerance = AddUIComponent<UITextField>();
            toleranceLabel = AddUIComponent<UILabel>();

            curveTolerance = AddUIComponent<UITextField>();
            curveToleranceLabel = AddUIComponent<UILabel>();

            tiles = AddUIComponent<UITextField>();
            tilesLabel = AddUIComponent<UILabel>();

            errorLabel = AddUIComponent<UILabel>();

            okButton = AddUIComponent<UIButton>();

            width = 400;
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

            pedestriansCheck.IsChecked = true;
            pedestriansCheck.relativePosition = new Vector3(x + 100, y);
            pedestriansCheck.size = new Vector2(13, 13);
            pedestriansCheck.Show();
            pedestriansCheck.color = new Color32(185, 221, 254, 255);
            pedestriansCheck.enabled = true;
            pedestriansCheck.eventClick += (component, param) =>
            {
                pedestriansCheck.IsChecked = !pedestriansCheck.IsChecked;
            };
            SetLabel(pedestrianLabel, "Pedestrian Paths", x, y);
            y += vertPadding;            

            SetLabel(scaleTextBoxLabel, "Scale", x, y);
            SetTextBox(scaleTextBox, "1", x + 120, y);
            y += vertPadding;

            SetLabel(pathTextBoxLabel, "Path", x, y);
            SetTextBox(pathTextBox, Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\map", x + 120, y);
            y += vertPadding;

            SetLabel(toleranceLabel, "Tolerance", x, y);
            SetTextBox(tolerance, "20", x + 120, y);
            y += vertPadding;

            SetLabel(curveToleranceLabel, "Curve Tolerance", x, y);
            SetTextBox(curveTolerance, "6", x + 120, y);
            y += vertPadding;

            SetLabel(tilesLabel, "Tiles to Boundary", x, y);
            SetTextBox(tiles, "4", x + 120, y);
            y += vertPadding;

            SetLabel(errorLabel, "", x, y);
            y += vertPadding;

            okButton.text = "Import";
            okButton.normalBgSprite = "ButtonMenu";
            okButton.hoveredBgSprite = "ButtonMenuHovered";
            okButton.focusedBgSprite = "ButtonMenuFocused";
            okButton.pressedBgSprite = "ButtonMenuPressed";
            okButton.size = new Vector2(100, 30);
            okButton.relativePosition = new Vector3( (width - okButton.size.x) / 2, y);
            okButton.eventClick += okButton_eventClick;
            okButton.textScale = 0.8f;

            height = y + vertPadding + 6;
        }

        private void SetTextBox(UITextField scaleTextBox, string p, int x, int y)
        {
            scaleTextBox.relativePosition = new Vector3(x, y);
            scaleTextBox.text = p;
            scaleTextBox.textScale = 0.8f;
            scaleTextBox.normalBgSprite = "TextFieldPanel";
            scaleTextBox.hoveredBgSprite = "TextFieldPanelHovered";
            scaleTextBox.focusedBgSprite = "TextFieldUnderline";
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
                roadMaker = new RoadMaker(pathTextBox.text.Trim(), pedestriansCheck.IsChecked, double.Parse(scaleTextBox.text.Trim()), double.Parse(tolerance.text.Trim()), double.Parse(curveTolerance.text.Trim()), double.Parse(tiles.text.Trim()));
                currentIndex = 0;
                createRoads = !createRoads;   
            }
            catch (FormatException ex) {
                errorLabel.text = "Parameter must be valid number.";
            }
            catch (Exception ex)
            {
                errorLabel.text = ex.ToString();
                throw ex;
            }            
        }

        public override void Update()
        {
            if (createRoads)
            {
                if (currentIndex < roadMaker.osm.processedWays.Count())
                {
                    SimulationManager.instance.AddAction(roadMaker.MakeRoad(currentIndex));
                    currentIndex += 1;
                }

                if (currentIndex < roadMaker.osm.processedWays.Count())
                {
                    SimulationManager.instance.AddAction(roadMaker.MakeRoad(currentIndex));
                    currentIndex += 1;
                }

                if (currentIndex < roadMaker.osm.processedWays.Count())
                {
                    SimulationManager.instance.AddAction(roadMaker.MakeRoad(currentIndex));
                    currentIndex += 1;
                    var instance = Singleton<NetManager>.instance;                    
                    errorLabel.text = String.Format("Making road {0} out of {1}. Nodes: {2}. Segments: {3}", currentIndex, roadMaker.osm.processedWays.Count(),instance.m_nodeCount, instance.m_segmentCount);
                }
            }

            if (roadMaker  != null && currentIndex == roadMaker.osm.processedWays.Count())
            {
                errorLabel.text = "Done.";
                createRoads = false;
                roadMaker = null;
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
