using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Parser.PathOfExile
{
    public class ItemGrid
    {
        // Slots in grid.
        public List<List<ItemSlot>> Slots { get; set; } = new List<List<ItemSlot>>();
        // Name of the grid.
        public string Name { get; set; }
        // Start point of the grid.
        public Point StartPoint { get; set; }
        // Amount of slots in the grid.
        public Point Size { get; set; }
        // Size of the slots.
        public Point SlotSize { get; set; }
        // List of points and their colors to check if grid is visible.
        public Dictionary<Point, ParserColor> VisibilityPoints { get; set; } = new Dictionary<Point, ParserColor>();
        // Could be used for pricing an entire stash tab.
        public dynamic Data { get; set; }

        [JsonConstructor]
        public ItemGrid(string InName, Point InStartPoint, Point InSize, Point InSlotSize, Dictionary<Point, ParserColor> InVisibilityPoints, dynamic InData = null)
        {
            Name = InName;
            StartPoint = InStartPoint;
            Size = InSize;
            SlotSize = InSlotSize;
            VisibilityPoints = InVisibilityPoints;
            Data = InData;//InData != null ? JsonConvert.DeserializeObject(InData) : null;
        }

        public ItemGrid(string InName, Point InStartPoint, Point InSize, Point InSlotSize, List<Point> InVisibilityPoints, dynamic InData = null)
        {
            Name = InName;
            StartPoint = InStartPoint;
            Size = InSize;
            SlotSize = InSlotSize;
            VisibilityPoints = InVisibilityPoints.ToDictionary(x => x, y => new ParserColor());
            Data = InData;
        }
    //    public ItemGrid(string InName, Point InStartPoint, Point InSize, Point InSlotSize, List<Point> InVisibilityPoints, dynamic InData = null)
    //: this(InName, InStartPoint, InSize, InSlotSize, InVisibilityPoints.ToDictionary(x => x, y => new ParserColor()), (dynamic)InData)
    //    { }
    }
}
