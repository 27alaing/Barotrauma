﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Barotrauma
{
    public class GUIStyle
    {
        private Dictionary<string, GUIComponentStyle> componentStyles;

        private XElement configElement;

        private GraphicsDevice graphicsDevice;

        private ScalableFont defaultFont;

        public ScalableFont Font { get; private set; }
        public ScalableFont GlobalFont { get; private set; }
        public ScalableFont UnscaledSmallFont { get; private set; }
        public ScalableFont SmallFont { get; private set; }
        public ScalableFont LargeFont { get; private set; }
        public ScalableFont SubHeadingFont { get; private set; }
        public ScalableFont DigitalFont { get; private set; }

        public Dictionary<ScalableFont, bool> ForceFontUpperCase
        {
            get;
            private set;
        } = new Dictionary<ScalableFont, bool>();

        public readonly Sprite[] CursorSprite = new Sprite[7];

        public UISprite UIGlow { get; private set; }
        public UISprite UIGlowCircular { get; private set; }

        public SpriteSheet FocusIndicator { get; private set; }

        /// <summary>
        /// General green color used for elements whose colors are set from code
        /// </summary>
        public Color Green { get; private set; } = Color.LightGreen;

        /// <summary>
        /// General red color used for elements whose colors are set from code
        /// </summary>
        public Color Orange { get; private set; } = Color.Orange;

        /// <summary>
        /// General red color used for elements whose colors are set from code
        /// </summary>
        public Color Red { get; private set; } = Color.Red;

        /// <summary>
        /// General blue color used for elements whose colors are set from code
        /// </summary>
        public Color Blue { get; private set; } = Color.Blue;

        public Color TextColor { get; private set; } = Color.White * 0.8f;
        public Color TextColorBright { get; private set; } = Color.White * 0.9f;
        public Color TextColorDark { get; private set; } = Color.Black * 0.9f;
        public Color TextColorDim { get; private set; } = Color.White * 0.6f;

        public static Point ItemFrameMargin = new Point(50, 56);
        public static Point ItemFrameOffset = new Point(0, 3);

        public GUIStyle(XElement element, GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;
            componentStyles = new Dictionary<string, GUIComponentStyle>();
            configElement = element;
            foreach (XElement subElement in configElement.Elements())
            {
                var name = subElement.Name.ToString().ToLowerInvariant();
                switch (name)
                {
                    case "cursor":
                        foreach (var children in subElement.Descendants())
                        {
                            var index = children.GetAttributeInt("state", (int) CursorState.Default);
                            CursorSprite[index] = new Sprite(children);
                        }
                        break;
                    case "green":
                        Green = subElement.GetAttributeColor("color", Green);
                        break;
                    case "orange":
                        Orange = subElement.GetAttributeColor("color", Orange);
                        break;
                    case "red":
                        Red = subElement.GetAttributeColor("color", Red);
                        break;
                    case "blue":
                        Blue = subElement.GetAttributeColor("color", Blue);
                        break;
                    case "textcolordark":
                        TextColorDark = subElement.GetAttributeColor("color", TextColorDark);
                        break;
                    case "TextColorBright":
                        TextColorBright = subElement.GetAttributeColor("color", TextColorBright);
                        break;
                    case "textcolordim":
                        TextColorDim = subElement.GetAttributeColor("color", TextColorDim);
                        break;
                    case "textcolornormal":
                    case "textcolor":
                        TextColor = subElement.GetAttributeColor("color", TextColor);
                        break;
                    case "uiglow":
                        UIGlow = new UISprite(subElement);
                        break;
                    case "uiglowcircular":
                        UIGlowCircular = new UISprite(subElement);
                        break;
                    case "focusindicator":
                        FocusIndicator = new SpriteSheet(subElement);
                        break;
                    case "font":
                        Font = LoadFont(subElement, graphicsDevice);
                        ForceFontUpperCase[Font] = subElement.GetAttributeBool("forceuppercase", false);
                        break;
                    case "globalfont":
                        GlobalFont = LoadFont(subElement, graphicsDevice);
                        ForceFontUpperCase[GlobalFont] = subElement.GetAttributeBool("forceuppercase", false);
                        break;
                    case "unscaledsmallfont":
                        UnscaledSmallFont = LoadFont(subElement, graphicsDevice);
                        ForceFontUpperCase[UnscaledSmallFont] = subElement.GetAttributeBool("forceuppercase", false);
                        break;
                    case "smallfont":
                        SmallFont = LoadFont(subElement, graphicsDevice);
                        ForceFontUpperCase[SmallFont] = subElement.GetAttributeBool("forceuppercase", false);
                        break;
                    case "largefont":
                        LargeFont = LoadFont(subElement, graphicsDevice);
                        ForceFontUpperCase[LargeFont] = subElement.GetAttributeBool("forceuppercase", false);
                        break;
                    case "digitalfont":
                        DigitalFont = LoadFont(subElement, graphicsDevice);
                        ForceFontUpperCase[DigitalFont] = subElement.GetAttributeBool("forceuppercase", false);
                        break;
                    case "objectivetitle":
                    case "subheading":
                        SubHeadingFont = LoadFont(subElement, graphicsDevice);
                        ForceFontUpperCase[SubHeadingFont] = subElement.GetAttributeBool("forceuppercase", false);
                        break;
                    default:
                        GUIComponentStyle componentStyle = new GUIComponentStyle(subElement, this);
                        componentStyles.Add(subElement.Name.ToString().ToLowerInvariant(), componentStyle);
                        break;
                }
            }

            if (GlobalFont == null)
            {
                GlobalFont = Font;
                DebugConsole.NewMessage("Global font not defined in the current UI style file. The global font is used to render western symbols when using Chinese/Japanese/Korean localization. Using default font instead...", Color.Orange);
            }

            GameMain.Instance.OnResolutionChanged += () => { RescaleElements(); };
        }

        /// <summary>
        /// Returns the default font of the currently selected language
        /// </summary>
        public ScalableFont LoadCurrentDefaultFont()
        {
            defaultFont?.Dispose();
            defaultFont = null;
            foreach (XElement subElement in configElement.Elements())
            {
                switch (subElement.Name.ToString().ToLowerInvariant())
                {
                    case "font":
                        defaultFont = LoadFont(subElement, graphicsDevice);
                        break;
                }
            }
            return defaultFont;
        }


        private void RescaleElements()
        {
            if (configElement == null) { return; }
            if (configElement.Elements() == null) { return; }
            foreach (XElement subElement in configElement.Elements())
            {
                switch (subElement.Name.ToString().ToLowerInvariant())
                {
                    case "font":
                        if (Font == null) { continue; }
                        Font.Size = GetFontSize(subElement);
                        break;
                    case "smallfont":
                        if (SmallFont == null) { continue; }
                        SmallFont.Size = GetFontSize(subElement);
                        break;
                    case "largefont":
                        if (LargeFont == null) { continue; }
                        LargeFont.Size = GetFontSize(subElement);
                        break;
                    case "objectivetitle":
                    case "subheading":
                        if (SubHeadingFont == null) { continue; }
                        SubHeadingFont.Size = GetFontSize(subElement);
                        break;
                }
            }

            foreach (var componentStyle in componentStyles.Values)
            {
                componentStyle.GetSize(componentStyle.Element);
                foreach (var childStyle in componentStyle.ChildStyles.Values)
                {
                    childStyle.GetSize(childStyle.Element);
                }
            }
        }

        private ScalableFont LoadFont(XElement element, GraphicsDevice graphicsDevice)
        {
            string file         = GetFontFilePath(element);
            uint size           = GetFontSize(element);
            bool dynamicLoading = GetFontDynamicLoading(element);
            bool isCJK          = GetIsCJK(element);
            return new ScalableFont(file, size, graphicsDevice, dynamicLoading, isCJK);
        }

        private uint GetFontSize(XElement element)
        {
            foreach (XElement subElement in element.Elements())
            {
                if (subElement.Name.ToString().ToLowerInvariant() != "size") { continue; }
                Point maxResolution = subElement.GetAttributePoint("maxresolution", new Point(int.MaxValue, int.MaxValue));
                if (GameMain.GraphicsWidth <= maxResolution.X && GameMain.GraphicsHeight <= maxResolution.Y)
                {
                    return (uint)subElement.GetAttributeInt("size", 14);
                }
            }
            return 14;
        }

        private string GetFontFilePath(XElement element)
        {
            foreach (XElement subElement in element.Elements())
            {
                if (subElement.Name.ToString().ToLowerInvariant() != "override") { continue; }
                string language = subElement.GetAttributeString("language", "").ToLowerInvariant();
                if (GameMain.Config.Language.ToLowerInvariant() == language)
                {
                    return subElement.GetAttributeString("file", "");
                }
            }
            return element.GetAttributeString("file", "");
        }

        private bool GetFontDynamicLoading(XElement element)
        {
            foreach (XElement subElement in element.Elements())
            {
                if (subElement.Name.ToString().ToLowerInvariant() != "override") { continue; }
                string language = subElement.GetAttributeString("language", "").ToLowerInvariant();
                if (GameMain.Config.Language.ToLowerInvariant() == language)
                {
                    return subElement.GetAttributeBool("dynamicloading", false);
                }
            }
            return element.GetAttributeBool("dynamicloading", false);
        }

        private bool GetIsCJK(XElement element)
        {
            foreach (XElement subElement in element.Elements())
            {
                if (subElement.Name.ToString().ToLowerInvariant() != "override") { continue; }
                string language = subElement.GetAttributeString("language", "").ToLowerInvariant();
                if (GameMain.Config.Language.ToLowerInvariant() == language)
                {
                    return subElement.GetAttributeBool("iscjk", false);
                }
            }
            return element.GetAttributeBool("iscjk", false);
        }

        public GUIComponentStyle GetComponentStyle(string name)
        {
            componentStyles.TryGetValue(name.ToLowerInvariant(), out GUIComponentStyle style);
            return style;
        }

        public void Apply(GUIComponent targetComponent, string styleName = "", GUIComponent parent = null)
        {
            GUIComponentStyle componentStyle = null;  
            if (parent != null)
            {
                GUIComponentStyle parentStyle = parent.Style;

                if (parent.Style == null)
                {
                    string parentStyleName = parent.GetType().Name.ToLowerInvariant();

                    if (!componentStyles.TryGetValue(parentStyleName, out parentStyle))
                    {
                        DebugConsole.ThrowError("Couldn't find a GUI style \""+ parentStyleName + "\"");
                        return;
                    }
                }
                
                string childStyleName = string.IsNullOrEmpty(styleName) ? targetComponent.GetType().Name : styleName;
                parentStyle.ChildStyles.TryGetValue(childStyleName.ToLowerInvariant(), out componentStyle);
            }
            else
            {
                if (string.IsNullOrEmpty(styleName))
                {
                    styleName = targetComponent.GetType().Name;
                }
                if (!componentStyles.TryGetValue(styleName.ToLowerInvariant(), out componentStyle))
                {
                    DebugConsole.ThrowError("Couldn't find a GUI style \""+ styleName+"\"");
                    return;
                }
            }
            
            targetComponent.ApplyStyle(componentStyle);            
        }
    }
}
