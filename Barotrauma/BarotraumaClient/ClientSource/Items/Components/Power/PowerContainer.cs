﻿using Barotrauma.Networking;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Barotrauma.Items.Components
{
    partial class PowerContainer : Powered, IDrawableComponent, IServerSerializable, IClientSerializable
    {
        private GUIProgressBar chargeIndicator;
        private GUIScrollBar rechargeSpeedSlider;

        public Vector2 DrawSize
        {
            //use the extents of the item as the draw size
            get { return Vector2.Zero; }
        }

        partial void InitProjSpecific()
        {
            if (GuiFrame == null) { return; }

            var paddedFrame = new GUIFrame(new RectTransform(new Vector2(0.75f, 0.75f), GuiFrame.RectTransform, Anchor.Center)
            {
                //RelativeOffset = new Vector2(0, 0.05f)
            }, style: null);

            var upperArea = new GUIFrame(new RectTransform(new Vector2(1, 0.4f), paddedFrame.RectTransform, Anchor.TopCenter), style: null);
            var lowerArea = new GUIFrame(new RectTransform(new Vector2(1, 0.6f), paddedFrame.RectTransform, Anchor.BottomCenter), style: null);


            string rechargeStr = TextManager.Get("PowerContainerRechargeRate");
            new GUITextBlock(new RectTransform(new Vector2(1.0f, 0.5f), upperArea.RectTransform, Anchor.TopCenter),
                "RechargeRate", textColor: GUI.Style.TextColor, font: GUI.SubHeadingFont, textAlignment: Alignment.Center)
            {
                TextGetter = () =>
                {
                    return rechargeStr.Replace("[rate]", ((int)((rechargeSpeed / maxRechargeSpeed) * 100.0f)).ToString());
                }
            };

            rechargeSpeedSlider = new GUIScrollBar(new RectTransform(new Vector2(0.9f, 0.4f), upperArea.RectTransform, Anchor.BottomCenter), 
                barSize: 0.15f, style: "DeviceSlider")
            {
                Step = 0.1f,
                OnMoved = (GUIScrollBar scrollBar, float barScroll) =>
                {
                    float newRechargeSpeed = maxRechargeSpeed * barScroll;
                    if (Math.Abs(newRechargeSpeed - rechargeSpeed) < 0.1f) { return false; }

                    RechargeSpeed = newRechargeSpeed;
                    if (GameMain.Client != null)
                    {
                        item.CreateClientEvent(this);
                        correctionTimer = CorrectionDelay;
                    }
                    return true;
                }
            };
            rechargeSpeedSlider.Bar.RectTransform.MaxSize = new Point(rechargeSpeedSlider.Bar.Rect.Height);
            
            // lower area --------------------------

            var textArea = new GUIFrame(new RectTransform(new Vector2(1, 0.4f), lowerArea.RectTransform), style: null);
            var chargeLabel = new GUITextBlock(new RectTransform(new Vector2(0.4f, 0.0f), textArea.RectTransform, Anchor.CenterLeft),
                TextManager.Get("charge"), textColor: GUI.Style.TextColor, font: GUI.SubHeadingFont, textAlignment: Alignment.CenterLeft)
            {
                ToolTip = TextManager.Get("PowerTransferTipPower")
            };
            string kWmin = TextManager.Get("kilowattminute");
            var chargeText = new GUITextBlock(new RectTransform(new Vector2(0.6f, 1), textArea.RectTransform, Anchor.CenterRight), 
                "", textColor: GUI.Style.TextColor, font: GUI.Font, textAlignment: Alignment.CenterRight)
            {
                TextGetter = () => $"{(int)charge}/{(int)capacity} {kWmin} ({((int)MathUtils.Percentage(charge, capacity)).ToString()} %)"
            };
            if (chargeText.TextSize.X > chargeText.Rect.Width) { chargeText.Font = GUI.SmallFont; }

            chargeIndicator = new GUIProgressBar(new RectTransform(new Vector2(1.1f, 0.5f), lowerArea.RectTransform, Anchor.BottomCenter)
            {
                RelativeOffset = new Vector2(0, 0.1f)
            }, barSize: 0.0f, style: "DeviceProgressBar")
            {
                ProgressGetter = () =>
                {
                    return capacity <= 0.0f ? 1.0f : charge / capacity;
                }
            };
        }

        public override void OnItemLoaded()
        {
            base.OnItemLoaded();
            if (rechargeSpeedSlider != null)
            {
                rechargeSpeedSlider.BarScroll = rechargeSpeed / MaxRechargeSpeed;
            }
        }
        
        public override void UpdateHUD(Character character, float deltaTime, Camera cam)
        {
            float chargeRatio = charge / capacity;
            chargeIndicator.Color = ToolBox.GradientLerp(chargeRatio, Color.Red, Color.Orange, Color.Green);
        }

        public void Draw(SpriteBatch spriteBatch, bool editing = false, float itemDepth = -1)
        {
            if (indicatorSize.X <= 1.0f || indicatorSize.Y <= 1.0f) { return; }

            GUI.DrawRectangle(spriteBatch,
                new Vector2(
                    item.DrawPosition.X - item.Sprite.SourceRect.Width / 2 * item.Scale + indicatorPosition.X * item.Scale,
                    -item.DrawPosition.Y - item.Sprite.SourceRect.Height / 2 * item.Scale + indicatorPosition.Y * item.Scale),
                indicatorSize * item.Scale, Color.Black, depth: item.SpriteDepth - 0.00001f);

            if (charge > 0)
            {
                Color indicatorColor = ToolBox.GradientLerp(charge / capacity, Color.Red, Color.Orange, Color.Green);
                if (!isHorizontal)
                {
                    GUI.DrawRectangle(spriteBatch,
                    new Vector2(
                        item.DrawPosition.X - item.Sprite.SourceRect.Width / 2 * item.Scale + indicatorPosition.X * item.Scale + 1,
                        -item.DrawPosition.Y - item.Sprite.SourceRect.Height / 2 * item.Scale + indicatorPosition.Y * item.Scale + 1 + ((indicatorSize.Y * item.Scale) * (1.0f - charge / capacity))),
                    new Vector2(indicatorSize.X * item.Scale - 2, (indicatorSize.Y * item.Scale - 2) * (charge / capacity)), indicatorColor, true, 
                    depth: item.SpriteDepth - 0.00001f);
                }
                else
                {
                    GUI.DrawRectangle(spriteBatch,
                    new Vector2(
                        item.DrawPosition.X - item.Sprite.SourceRect.Width / 2 * item.Scale + indicatorPosition.X * item.Scale + 1 ,
                        -item.DrawPosition.Y - item.Sprite.SourceRect.Height / 2 * item.Scale + indicatorPosition.Y * item.Scale + 1),
                    new Vector2((indicatorSize.X * item.Scale - 2) * (charge / capacity), indicatorSize.Y * item.Scale - 2), indicatorColor, true, 
                    depth: item.SpriteDepth - 0.00001f);
                }
            }

        }
        
        public void ClientWrite(IWriteMessage msg, object[] extraData)
        {
            msg.WriteRangedInteger((int)(rechargeSpeed / MaxRechargeSpeed * 10), 0, 10);
        }

        public void ClientRead(ServerNetObject type, IReadMessage msg, float sendingTime)
        {
            if (correctionTimer > 0.0f)
            {
                StartDelayedCorrection(type, msg.ExtractBits(4 + 8), sendingTime);
                return;
            }

            float rechargeRate = msg.ReadRangedInteger(0, 10) / 10.0f;
            RechargeSpeed = rechargeRate * MaxRechargeSpeed;
#if CLIENT
            if (rechargeSpeedSlider != null)
            {
                rechargeSpeedSlider.BarScroll = rechargeRate;
            }
#endif
            Charge = msg.ReadRangedSingle(0.0f, 1.0f, 8) * capacity;
        }
    }
}
