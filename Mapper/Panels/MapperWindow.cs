using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ColossalFramework;
using ColossalFramework.Math;
using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Timers;
using UnityEngine;

namespace Mapper
{
    public class MapperWindow : UIPanel
    {
        UIButton descriptionButton;
        UILabel descriptionLabel;
        RoadMaker osm;
        bool createRoads = false;
        int currentIndex = 0;

        public override void Awake()
        {           
            descriptionLabel = AddUIComponent<UILabel>();
            descriptionButton = AddUIComponent<UIButton>();

            base.Awake();

        }

        public override void Start()
        {
            base.Start();

            backgroundSprite = "MenuPanel2";
            opacity = 0.8f;
            isVisible = true;
            canFocus = true;
            isInteractive = true;
            SetupControls();
        }

        public void SetupControls()
        {
            base.Start();

            SetLabel(descriptionLabel, "Import");
            descriptionLabel.textScale = 0.65f;
            descriptionLabel.wordWrap = true;
            //descriptionLabel.size = new Vector2(barWidth - 20, 140);
            descriptionLabel.autoSize = false;
            descriptionLabel.width = 120;
            descriptionLabel.wordWrap = true;
            descriptionLabel.autoHeight = true;
            descriptionLabel.anchor = (UIAnchorStyle.Top | UIAnchorStyle.Left | UIAnchorStyle.Right);
            descriptionButton.normalBgSprite = "IconDownArrow";
            descriptionButton.hoveredBgSprite = "IconDownArrowHovered";
            descriptionButton.focusedBgSprite = "IconDownArrowFocused";
            descriptionButton.pressedBgSprite = "IconDownArrow";
            descriptionButton.size = new Vector3(80, 20);
            descriptionButton.color = Color.white;
            descriptionButton.colorizeSprites = true;

            descriptionButton.eventClick += descriptionButton_eventClick;

        }

        private void descriptionButton_eventClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            if (osm == null)
            {
                osm = new RoadMaker("map");
            }
            createRoads = !createRoads;            
        }
        
        private void SetLabel(UILabel title, string p)
        {
            title.text = p;
            title.textScale = 0.7f;
            title.size = new Vector2(120, 30);
        }

        private void SetPos(UILabel title, UIProgressBar bar, float x, float y, bool visible)
        {
            bar.relativePosition = new Vector3(x + 120, y - 3);
            title.relativePosition = new Vector3(x, y);
            if (visible)
            {
                bar.Show();
                title.Show();
            }
            else
            {
                bar.Hide();
                title.Hide();
            }
        }

        public override void Update()
        {
            if (createRoads && currentIndex < osm.osm.processedWays.Count())
            {
                SimulationManager.instance.AddAction(osm.MakeRoad(currentIndex));
                currentIndex += 1;
            }

            base.Update();
        }
    }
}
