using ColossalFramework;
using ColossalFramework.Math;
using ColossalFramework.UI;
using ICities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Mapper
{
    public class MapperLoading : LoadingExtensionBase
    {
        GameObject buildingWindowGameObject;
        MapperWindow buildingWindow;
        private LoadMode _mode;

        public override void OnLevelLoaded(LoadMode mode)
        {
            if (mode != LoadMode.LoadGame && mode != LoadMode.NewGame)
                return;
            _mode = mode;

            buildingWindowGameObject = new GameObject("buildingWindowObject");

            var view = UIView.GetAView();
            this.buildingWindow = buildingWindowGameObject.AddComponent<MapperWindow>();
            this.buildingWindow.transform.parent = view.transform;
            this.buildingWindow.position = new Vector3(300, 122);
            this.buildingWindow.Hide();

            var strip = ToolsModifierControl.mainToolbar.component as UITabstrip;
            GameObject asGameObject = UITemplateManager.GetAsGameObject("MainToolbarButtonTemplate");
            GameObject asGameObject2 = UITemplateManager.GetAsGameObject("ScrollablePanelTemplate");
            var uiButton = strip.AddTab("mapperMod", asGameObject, asGameObject2, new Type[] {});
            uiButton.eventClick += uiButton_eventClick;

        }

        private void uiButton_eventClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            if (!this.buildingWindow.isVisible)
            {
                this.buildingWindow.isVisible = true;
                this.buildingWindow.BringToFront();
                this.buildingWindow.Show();
            }
            else
            {
                this.buildingWindow.isVisible = false;
                this.buildingWindow.Hide();
            }            
        }

        public override void OnLevelUnloading()
        {
            if (_mode != LoadMode.LoadGame && _mode != LoadMode.NewGame)
                return;


            if (buildingWindowGameObject != null)
            {
                GameObject.Destroy(buildingWindowGameObject);
            }
        }

    }
}
