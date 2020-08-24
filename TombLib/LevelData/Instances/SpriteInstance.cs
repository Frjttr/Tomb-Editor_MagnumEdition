﻿using System.Drawing;
using System.Numerics;
using TombLib.Graphics;
using TombLib.Wad;
using TombLib.Wad.Catalog;

namespace TombLib.LevelData
{
    public class SpriteInstance : PositionBasedObjectInstance
    {
        public ushort SpriteID { get; set; }

        public Rectangle2 GetSpriteViewportRect(WadSprite sprite, Size viewportSize, Camera camera, out float depth)
        {
            var heightRatio = ((float)viewportSize.Height / viewportSize.Width) * 1024.0f;
            var distance = Vector3.Distance(Position + Room.WorldPos, camera.GetPosition());
            var scale = 1024.0f / (distance != 0 ? distance : 1.0f);
            var pos = (WorldPositionMatrix * camera.GetViewProjectionMatrix(viewportSize.Width, viewportSize.Height)).TransformPerspectively(new Vector3());
            var screenPos = pos.To2();
            var start = scale * new Vector2(sprite.Alignment.Start.X / 1024.0f, sprite.Alignment.Start.Y / heightRatio);
            var end = scale * new Vector2(sprite.Alignment.End.X / 1024.0f, sprite.Alignment.End.Y / heightRatio);

            depth = pos.Z;
            return new Rectangle2(screenPos - end, screenPos - start);
        }

        private string GetSequenceName()
        {
            uint index = 0;

            foreach (var seq in Room.Level.Settings.WadGetAllSpriteSequences())
                foreach (var spr in seq.Value.Sprites)
                {
                    if (index == SpriteID)
                        return TrCatalog.GetSpriteSequenceName(Room.Level.Settings.GameVersion, seq.Value.Id.TypeId);
                    index++;
                }

            return "Missing sequence";
        }

        public override string ToString()
        {
            return "Sprite ID = " + SpriteID + " (" + GetSequenceName() + ")" +
                ", Room = " + (Room?.ToString() ?? "NULL") +
                ", X = " + SectorPosition.X +
                ", Z = " + SectorPosition.Y;
        }

        public string ShortName() => "Sprite ID = " + SpriteID + " (" + GetSequenceName() + ")";
    }
}