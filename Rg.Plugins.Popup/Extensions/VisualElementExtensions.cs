﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;

using Microsoft.Maui;

namespace Rg.Plugins.Popup.Extensions
{
    internal static class VisualElementExtensions
    {
        [Obsolete("Use " + nameof(Element) + "." + nameof(Element.Descendants))]
        internal static IEnumerable<Element> RgDescendants(this Element element)
        {
            var queue = new Queue<Element>(16);
            queue.Enqueue(element);

            while (queue.Count > 0)
            {
                var children = ((IElementController)queue.Dequeue()).LogicalChildren;
                for (var i = 0; i < children.Count; i++)
                {
                    Element child = children[i];
                    yield return child;
                    queue.Enqueue(child);
                }
            }
        }
    }
}
