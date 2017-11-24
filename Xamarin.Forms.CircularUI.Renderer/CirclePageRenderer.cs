﻿using ElmSharp.Wearable;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Xamarin.Forms.CircularUI;
using Xamarin.Forms.Platform.Tizen;
using System.ComponentModel;

[assembly: ExportRenderer(typeof(CirclePage), typeof(Xamarin.Forms.CircularUI.Renderer.CirclePageRenderer))]

namespace Xamarin.Forms.CircularUI.Renderer
{
    public class CirclePageRenderer : VisualElementRenderer<CirclePage>
    {
        CirclePageWidget _widget;
        IRotaryFocusable _currentRotaryFocusObject;

        public CirclePageRenderer()
        {
            RegisterPropertyHandler(Page.BackgroundImageProperty, UpdateBackgroundImage);
            RegisterPropertyHandler(CirclePage.ActionButtonProperty, UpdateActionButton);
            RegisterPropertyHandler(CirclePage.RotaryFocusObjectProperty, UpdateRotaryFocusObject);
        }

        public ElmSharp.Wearable.CircleSurface CircleSurface => _widget.CircleSurface;

        protected override void OnElementChanged(ElementChangedEventArgs<CirclePage> args)
        {
            if (null == _widget)
            {
                _widget = new CirclePageWidget(Xamarin.Forms.Platform.Tizen.Forms.Context.MainWindow, Element);
                _widget.LayoutUpdated += OnLayoutUpdated;
                _widget.ToolbarOpened += OnToolbarOpened;
                _widget.ToolbarClosed += OnToolbarClosed;
                SetNativeView(_widget);
            }
            if (args.NewElement != null)
            {
                args.NewElement.Appearing += OnCirclePageAppearing;
                args.NewElement.Disappearing += OnCirclePageDisappearing;
            }
            if (args.OldElement != null)
            {
                args.OldElement.Appearing -= OnCirclePageAppearing;
                args.OldElement.Disappearing -= OnCirclePageDisappearing;
            }
            base.OnElementChanged(args);
        }

        protected void OnCirclePageAppearing(object sender, EventArgs e)
        {
            GetRotaryWidget(Element?.RotaryFocusObject)?.Activate();
        }

        protected void OnCirclePageDisappearing(object sender, EventArgs e)
        {
            GetRotaryWidget(Element?.RotaryFocusObject)?.Deactivate();
        }

        protected override void UpdateBackgroundColor(bool initialize)
        {
            if (initialize && Element.BackgroundColor.IsDefault) return;

            if (Element.BackgroundColor.A == 0)
            {
                _widget.BackgroundColor = ElmSharp.Color.Transparent;
            }
            else
            {
                _widget.BackgroundColor = Element.BackgroundColor.ToNative();
            }
        }

        void OnToolbarClosed(object sender, EventArgs e)
        {
            GetRotaryWidget(_currentRotaryFocusObject)?.Activate();
        }

        void OnToolbarOpened(object sender, EventArgs e)
        {
            GetRotaryWidget(_currentRotaryFocusObject)?.Deactivate();
        }

        IRotaryActionWidget GetRotaryWidget(IRotaryFocusable focusable)
        {
            var consumer = focusable as BindableObject;
            if (consumer != null)
            {
                var consumerRenderer = Xamarin.Forms.Platform.Tizen.Platform.GetRenderer(consumer);
                IRotaryActionWidget rotaryWidget = null;
                if (consumerRenderer != null)
                {
                    var nativeView = consumerRenderer.NativeView;
                    rotaryWidget = nativeView as ElmSharp.Wearable.IRotaryActionWidget;
                }
                else if (consumer is Xamarin.Forms.CircularUI.CircleSliderSurfaceItem)
                {
                    ICircleSurfaceItem item = consumer as ICircleSurfaceItem;
                    rotaryWidget = _widget.GetCircleWidget(item) as IRotaryActionWidget;
                }

                if (rotaryWidget != null)
                {
                    return rotaryWidget;
                }
            }
            return null;
        }

        void UpdateRotaryFocusObject(bool initialize)
        {
            if (!initialize)
                GetRotaryWidget(_currentRotaryFocusObject)?.Deactivate();
            _currentRotaryFocusObject = Element.RotaryFocusObject;
            if (!initialize)
                GetRotaryWidget(_currentRotaryFocusObject)?.Activate();
        }

        void OnLayoutUpdated(object sender, Platform.Tizen.Native.LayoutEventArgs args)
        {
            DoLayout(args);
        }

        void UpdateBackgroundImage(bool initialize)
        {
            if (initialize && string.IsNullOrWhiteSpace(Element.BackgroundImage))
                return;
            if (string.IsNullOrWhiteSpace(Element.BackgroundImage))
            {
                _widget.File = null;
            }
            else
            {
                _widget.File = ResourcePath.GetPath(Element.BackgroundImage);
            }
        }

        void UpdateActionButton(bool initialize)
        {
            if (initialize && Element.ActionButton == null) return;

            if (Element.ActionButton != null)
            {
                var item = Element.ActionButton;
                _widget.ShowActionButton(item.Text, item.Icon, () => item.Activate());
            }
            else
            {
                _widget.HideActionButton();
            }
        }
    }
}
