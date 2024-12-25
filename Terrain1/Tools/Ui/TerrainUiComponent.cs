using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Games;
using Stride.Graphics;
using Stride.UI;
using Stride.UI.Controls;
using Stride.UI.Panels;
using System;
using System.Linq;

namespace Terrain.Tools.Ui;

[ComponentCategory("Terrain")]
public class TerrainUiComponent : TerrainUITool
{
    public override void Update(GameTime gameTime)
    {
        var ui = (UIComponent)Terrain.Entity.Components.FirstOrDefault((x) => x is UIComponent);

        if (ui is null)
        {
            return;
        }
        if (ui.Page == null)
        {
            return;
        }

        if (ui.Page.RootElement is null)
        {
            ui.Page.RootElement = CreateGrid();
        }
        var rootGrid = (Grid)ui.Page.RootElement;

        // Get all active tools
        var activeTools = Terrain.Entity.Components.OfType<TerrainEditorTool>().ToList();
        if (rootGrid.RowDefinitions.Count < activeTools.Count)
        {
            while (rootGrid.RowDefinitions.Count < activeTools.Count)
            {
                rootGrid.RowDefinitions.Add(new StripDefinition(StripType.Star));
            }
        }
        // Get the IDs of all active tools
        var activeToolIds = activeTools.Select(tool => tool.Id.ToString()).ToHashSet();

        rootGrid.DefaultHeight = (activeTools.Count * 30) + 20;
        // Remove buttons that no longer correspond to active tools
        foreach (var child in rootGrid.Children.ToList())
        {
            if (child is Button button)
            {
                // Check if the button's Name matches any active tool's Id
                if (!activeToolIds.Contains(button.Name))
                {
                    rootGrid.Children.Remove(child);
                }
            }
        }

        // Add buttons for tools that don't have one yet
        var counter = 0;
        foreach (var tool in activeTools)
        {
            // Check if a button for this tool already exists
            var existingButton = rootGrid.Children.OfType<Button>()
                .FirstOrDefault(button => button.Name == tool.Id.ToString());

            if (existingButton == null)
            {
                existingButton = new Button()
                {
                    Content = CreateTextBlock(tool.UIName),
                    Height = 30,
                    Name = tool.Id.ToString(),  // Set button Name to tool Id
                    Opacity = 1,
                    
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Center,
                    BackgroundColor = new Color(255, 255, 255, 255),
                };

                existingButton.Click += (a, e) =>
                {
                    var activeTools = Terrain.Entity.Components.OfType<TerrainEditorTool>().ToList();
                    foreach (var tool in activeTools)
                    {
                        tool.Active = false;
                    }
                    tool.Active = true;
                    e.Handled = true;
                };

                // Assign the button to the next available row, column 0 (stacked vertically)
                existingButton.SetGridColumn(0);

                rootGrid.Children.Add(existingButton);

            }
            ((TextBlock)existingButton.Content).Text = tool.UIName;
            existingButton.SetGridRow(counter);
            counter++;
        }

    }
    private Grid CreateGrid()
    {
        return new()
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left,
            BackgroundColor = Color.Blue,
            Opacity = 1,
            RowDefinitions = {

            },
            ColumnDefinitions =
            {
                new StripDefinition(StripType.Star),
            },

        };
    }

    private TextBlock CreateTextBlock(string? text = null, float textSize = 20, TextAlignment textAlignment = TextAlignment.Center)
    {
        return new()
        {
            Text = text,
            TextColor = Color.Red,
            Margin = new Thickness(3, 0, 3, 0),
            TextSize = textSize,
            Font = EditorFont,
            TextAlignment = textAlignment,
            VerticalAlignment = VerticalAlignment.Center
        };
    }
}
