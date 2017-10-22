﻿using NLog;
using SharpDX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TombLib.IO;
using TombLib.Utils;

namespace TombEditor.Geometry.IO
{
    public static class Prj2Loader
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public static Level LoadFromPrj2(string filename, IProgressReporter progressReporter)
        {
            using (var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var chunkIO = new ChunkReader(Prj2Chunks.MagicNumber, fileStream))
                return LoadLevel(chunkIO, filename);
        }

        private class LevelSettingsIds
        {
            public Dictionary<long, ImportedGeometry> ImportedGeometries { get; set; } = new Dictionary<long, ImportedGeometry>();
            public Dictionary<long, LevelTexture> LevelTextures { get; set; } = new Dictionary<long, LevelTexture>();
        }

        private static Level LoadLevel(ChunkReader chunkIO, string thisPath)
        {
            LevelSettingsIds levelSettingsIds = new LevelSettingsIds();
            Level level = new Level();
            chunkIO.ReadChunks((id, chunkSize) =>
            {
                if (LoadLevelSettings(chunkIO, id, level, thisPath, ref levelSettingsIds))
                    return true;
                else if (LoadRooms(chunkIO, id, level, levelSettingsIds))
                    return true;
                return false;
            });
            return level;
        }

        private static bool LoadLevelSettings(ChunkReader chunkIO, ChunkId idOuter, Level level, string thisPath, ref LevelSettingsIds levelSettingsIdsOuter)
        {
            if (idOuter != Prj2Chunks.Settings)
                return false;

            var settings = new LevelSettings { LevelFilePath = thisPath };
            var levelSettingsIds = new LevelSettingsIds();
            var ImportedGeometriesToLoad = new Dictionary<ImportedGeometry, ImportedGeometryInfo>();
            var LevelTexturesToLoad = new Dictionary<LevelTexture, string>();

            chunkIO.ReadChunks((id, chunkSize) =>
            {
                if (id == Prj2Chunks.WadFilePath)
                    settings.WadFilePath = chunkIO.ReadChunkString(chunkSize);
                else if (id == Prj2Chunks.FontTextureFilePath)
                    settings.FontTextureFilePath = chunkIO.ReadChunkString(chunkSize);
                else if (id == Prj2Chunks.SkyTextureFilePath)
                    settings.SkyTextureFilePath = chunkIO.ReadChunkString(chunkSize);
                else if (id == Prj2Chunks.OldWadSoundPaths)
                {
                    var oldWadSoundPaths = new List<OldWadSoundPath>();
                    chunkIO.ReadChunks((id2, chunkSize2) =>
                    {
                        if (id2 != Prj2Chunks.OldWadSoundPath)
                            return false;

                        var oldWadSoundPath = new OldWadSoundPath("");
                        chunkIO.ReadChunks((id3, chunkSize3) =>
                        {
                            if (id3 == Prj2Chunks.OldWadSoundPathPath)
                                oldWadSoundPath.Path = chunkIO.ReadChunkString(chunkSize3);
                            else
                                return false;
                            return true;
                        });
                        oldWadSoundPaths.Add(oldWadSoundPath);
                        return true;
                    });
                    settings.OldWadSoundPaths = oldWadSoundPaths;
                }
                else if (id == Prj2Chunks.GameDirectory)
                    settings.GameDirectory = chunkIO.ReadChunkString(chunkSize);
                else if (id == Prj2Chunks.GameLevelFilePath)
                    settings.GameLevelFilePath = chunkIO.ReadChunkString(chunkSize);
                else if (id == Prj2Chunks.GameExecutableFilePath)
                    settings.GameExecutableFilePath = chunkIO.ReadChunkString(chunkSize);
                else if (id == Prj2Chunks.GameExecutableSuppressAskingForOptions)
                    settings.GameExecutableSuppressAskingForOptions = chunkIO.ReadChunkBool(chunkSize);
                else if (id == Prj2Chunks.Textures)
                {
                    var toLoad = new Dictionary<LevelTexture, string>();
                    var levelTextures = new Dictionary<long, LevelTexture>();
                    chunkIO.ReadChunks((id2, chunkSize2) =>
                    {
                        if (id2 != Prj2Chunks.LevelTexture)
                            return false;

                        string path = "";
                        LevelTexture levelTexture = new LevelTexture();
                        long levelTextureIndex = long.MinValue;
                        chunkIO.ReadChunks((id3, chunkSize3) =>
                        {
                            if (id3 == Prj2Chunks.LevelTextureIndex)
                                levelTextureIndex = chunkIO.ReadChunkLong(chunkSize3);
                            else if (id3 == Prj2Chunks.LevelTexturePath)
                                path = chunkIO.ReadChunkString(chunkSize3); // Don't set the path right away, to not load the texture until all information is available.
                            else if (id3 == Prj2Chunks.LevelTextureConvert512PixelsToDoubleRows)
                                levelTexture.SetConvert512PixelsToDoubleRows(settings, chunkIO.ReadChunkBool(chunkSize3));
                            else if (id3 == Prj2Chunks.LevelTextureReplaceMagentaWithTransparency)
                                levelTexture.SetReplaceWithTransparency(settings, chunkIO.ReadChunkBool(chunkSize3));
                            else if (id3 == Prj2Chunks.LevelTextureSounds)
                            {
                                int width = chunkIO.Raw.ReadInt32();
                                int height = chunkIO.Raw.ReadInt32();
                                levelTexture.ResizeTextureSounds(width, height);
                                for (int y = 0; y < levelTexture.TextureSoundHeight; ++y)
                                    for (int x = 0; x < levelTexture.TextureSoundWidth; ++x)
                                    {
                                        byte textureSoundByte = chunkIO.Raw.ReadByte();
                                        if (textureSoundByte > 15)
                                            textureSoundByte = 15;
                                        levelTexture.SetTextureSound(x, y, (TextureSound)textureSoundByte);
                                    }
                            }
                            else
                                return false;
                            return true;
                        });
                        levelTextures.Add(levelTextureIndex, levelTexture);
                        toLoad.Add(levelTexture, path);
                        return true;
                    });
                    settings.Textures = levelTextures.Values.ToList();
                    levelSettingsIds.LevelTextures = levelTextures;
                    LevelTexturesToLoad = toLoad;
                }
                else if (id == Prj2Chunks.ImportedGeometries)
                {
                    var toLoad = new Dictionary<ImportedGeometry, ImportedGeometryInfo>();
                    var importedGeometries = new Dictionary<long, ImportedGeometry>();
                    chunkIO.ReadChunks((id2, chunkSize2) =>
                    {
                        if (id2 != Prj2Chunks.ImportedGeometry)
                            return false;

                        ImportedGeometry importedGeometry = new ImportedGeometry();
                        ImportedGeometryInfo importedGeometryInfo = new ImportedGeometryInfo();
                        long importedGeometryIndex = long.MinValue;
                        chunkIO.ReadChunks((id3, chunkSize3) =>
                        {
                            if (id3 == Prj2Chunks.ImportedGeometryIndex)
                                importedGeometryIndex = chunkIO.ReadChunkLong(chunkSize3);
                            else if (id3 == Prj2Chunks.ImportedGeometryPath)
                                importedGeometryInfo.Path = chunkIO.ReadChunkString(chunkSize3);
                            else if (id3 == Prj2Chunks.ImportedGeometryName)
                                importedGeometryInfo.Name = chunkIO.ReadChunkString(chunkSize3);
                            else if (id3 == Prj2Chunks.ImportedGeometryScale)
                                importedGeometryInfo.Scale = chunkIO.ReadChunkFloat(chunkSize3);
                            else
                                return false;
                            return true;
                        });

                        importedGeometries.Add(importedGeometryIndex, importedGeometry);
                        toLoad.Add(importedGeometry, importedGeometryInfo);
                        return true;
                    });
                    settings.ImportedGeometries = importedGeometries.Values.ToList();
                    levelSettingsIds.ImportedGeometries = importedGeometries;
                    ImportedGeometriesToLoad = toLoad;
                }
                else if (id == Prj2Chunks.AnimatedTextureSets)
                {
                    var animatedTextureSets = new List<AnimatedTextureSet>();
                    chunkIO.ReadChunks((id2, chunkSize2) =>
                    {
                        if (id2 != Prj2Chunks.AnimatedTextureSet)
                            return false;

                        var set = new AnimatedTextureSet();
                        chunkIO.ReadChunks((id3, chunkSize3) =>
                        {
                            if (id3 == Prj2Chunks.AnimatedTextureFrames)
                            {
                                var frames = new List<AnimatedTextureFrame>();
                                chunkIO.ReadChunks((id4, chunkSize4) =>
                                {
                                    if (id4 != Prj2Chunks.AnimatedTextureFrame)
                                        return false;

                                    frames.Add(new AnimatedTextureFrame
                                    {
                                        Texture = levelSettingsIds.LevelTextures[LEB128.ReadLong(chunkIO.Raw)],
                                        TexCoord0 = chunkIO.Raw.ReadVector2(),
                                        TexCoord1 = chunkIO.Raw.ReadVector2(),
                                        TexCoord2 = chunkIO.Raw.ReadVector2(),
                                        TexCoord3 = chunkIO.Raw.ReadVector2()
                                    });
                                    return true;
                                });

                                set.Frames = frames;
                            }
                            else
                                return false;
                            return true;
                        });
                        animatedTextureSets.Add(set);
                        return true;
                    });
                    settings.AnimatedTextureSets = animatedTextureSets;
                }
                else
                    return false;
                return true;
            });


            // Load level textures
            foreach (var levelTexture in LevelTexturesToLoad)
                levelTexture.Key.SetPath(settings, levelTexture.Value);

            // Load imported geoemtries
            settings.ImportedGeometryUpdate(ImportedGeometriesToLoad);

            // Apply settings
            levelSettingsIdsOuter = levelSettingsIds;
            level.ApplyNewLevelSettings(settings);
            return true;
        }

        private static bool LoadRooms(ChunkReader chunkIO, ChunkId idOuter, Level level, LevelSettingsIds levelSettingsIds)
        {
            if (idOuter != Prj2Chunks.Rooms)
                return false;

            List<KeyValuePair<long, Action<Room>>> roomLinkActions = new List<KeyValuePair<long, Action<Room>>>();
            Dictionary<long, Room> newRooms = new Dictionary<long, Room>();

            List<KeyValuePair<long, Action<ObjectInstance>>> objectLinkActions = new List<KeyValuePair<long, Action<ObjectInstance>>>();
            Dictionary<long, ObjectInstance> newObjects = new Dictionary<long, ObjectInstance>();

            chunkIO.ReadChunks((id, chunkSize) =>
            {
                if (id != Prj2Chunks.Room)
                    return false;

                // Read room
                Room room = new Room(level, LEB128.ReadInt(chunkIO.Raw), LEB128.ReadInt(chunkIO.Raw));
                long roomIndex = long.MinValue;
                chunkIO.ReadChunks((id2, chunkSize2) =>
                {
                    // Read basic room properties
                    if (id2 == Prj2Chunks.RoomIndex)
                        roomIndex = chunkIO.ReadChunkLong(chunkSize2);
                    else if (id2 == Prj2Chunks.RoomName)
                        room.Name = chunkIO.ReadChunkString(chunkSize2);
                    else if (id2 == Prj2Chunks.RoomPosition)
                        room.Position = chunkIO.ReadChunkVector3(chunkSize2);

                    // Read sectors
                    else if (id2 == Prj2Chunks.RoomSectors)
                        chunkIO.ReadChunks((id3, chunkSize3) =>
                        {
                            if (id3 != Prj2Chunks.Sector)
                                return false;

                            int ReadPos = chunkIO.Raw.ReadInt32();
                            int x = ReadPos % room.NumXSectors;
                            int z = ReadPos / room.NumXSectors;
                            Block block = room.Blocks[x, z];

                            chunkIO.ReadChunks((id4, chunkSize4) =>
                                {
                                    if (id4 == Prj2Chunks.SectorProperties)
                                    {
                                        long flag = chunkIO.ReadChunkLong(chunkSize4);
                                        if (((flag & 1) != 0) && block.Type != BlockType.BorderWall)
                                            block.Type = BlockType.Wall;
                                        block.Flags = (BlockFlags)(flag >> 2);
                                        block.ForceFloorSolid = (flag & 2) != 0;
                                    }
                                    else if (id4 == Prj2Chunks.SectorFloor)
                                    {
                                        long flag = LEB128.ReadLong(chunkIO.Raw);
                                        for (int j = 0; j < 4; j++)
                                            block.QAFaces[j] = LEB128.ReadShort(chunkIO.Raw);
                                        for (int j = 0; j < 4; j++)
                                            block.EDFaces[j] = LEB128.ReadShort(chunkIO.Raw);
                                        block.FloorSplitDirectionIsXEqualsZ = (flag & 1) != 0;
                                        block.FloorDiagonalSplit = (DiagonalSplit)(flag >> 1);
                                    }
                                    else if (id4 == Prj2Chunks.SectorCeiling)
                                    {
                                        long flag = LEB128.ReadLong(chunkIO.Raw);
                                        for (int j = 0; j < 4; j++)
                                            block.WSFaces[j] = LEB128.ReadShort(chunkIO.Raw);
                                        for (int j = 0; j < 4; j++)
                                            block.RFFaces[j] = LEB128.ReadShort(chunkIO.Raw);
                                        block.CeilingSplitDirectionIsXEqualsZ = (flag & 1) != 0;
                                        block.CeilingDiagonalSplit = (DiagonalSplit)(flag >> 1);
                                    }
                                    else if (id4 == Prj2Chunks.TextureLevelTexture)
                                    {
                                        BlockFace face = (BlockFace)LEB128.ReadLong(chunkIO.Raw);

                                        var textureArea = new TextureArea();
                                        textureArea.TexCoord0 = chunkIO.Raw.ReadVector2();
                                        textureArea.TexCoord1 = chunkIO.Raw.ReadVector2();
                                        textureArea.TexCoord2 = chunkIO.Raw.ReadVector2();
                                        textureArea.TexCoord3 = chunkIO.Raw.ReadVector2();
                                        long blendFlag = LEB128.ReadLong(chunkIO.Raw);
                                        textureArea.BlendMode = (BlendMode)(blendFlag >> 1);
                                        textureArea.DoubleSided = (blendFlag & 1) != 0;
                                        textureArea.Texture = levelSettingsIds.LevelTextures.TryGetOrDefault(LEB128.ReadLong(chunkIO.Raw));

                                        block.SetFaceTexture(face, textureArea);
                                    }
                                    else if (id4 == Prj2Chunks.TextureInvisible)
                                    {
                                        BlockFace face = (BlockFace)LEB128.ReadLong(chunkIO.Raw);
                                        block.SetFaceTexture(face, new TextureArea { Texture = TextureInvisible.Instance });
                                    }
                                    else
                                        return false;
                                    return true;
                                });
                            return true;
                        });

                    // Read room properties
                    else if (id2 == Prj2Chunks.RoomAmbientLight)
                        room.AmbientLight = chunkIO.ReadChunkVector4(chunkSize2);
                    else if (id2 == Prj2Chunks.RoomFlagCold)
                        room.FlagCold = chunkIO.ReadChunkBool(chunkSize2);
                    else if (id2 == Prj2Chunks.RoomFlagDamage)
                        room.FlagDamage = chunkIO.ReadChunkBool(chunkSize2);
                    else if (id2 == Prj2Chunks.RoomFlagHorizon)
                        room.FlagHorizon = chunkIO.ReadChunkBool(chunkSize2);
                    else if (id2 == Prj2Chunks.RoomFlagOutside)
                        room.FlagOutside = chunkIO.ReadChunkBool(chunkSize2);
                    else if (id2 == Prj2Chunks.RoomFlagNoLensflare)
                        room.FlagNoLensflare = chunkIO.ReadChunkBool(chunkSize2);
                    else if (id2 == Prj2Chunks.RoomFlagRain)
                        room.FlagRain = chunkIO.ReadChunkBool(chunkSize2);
                    else if (id2 == Prj2Chunks.RoomFlagSnow)
                        room.FlagSnow = chunkIO.ReadChunkBool(chunkSize2);
                    else if (id2 == Prj2Chunks.RoomFlagQuickSand)
                        room.FlagQuickSand = chunkIO.ReadChunkBool(chunkSize2);
                    else if (id2 == Prj2Chunks.RoomFlagExcludeFromPathFinding)
                        room.FlagExcludeFromPathFinding = chunkIO.ReadChunkBool(chunkSize2);
                    else if (id2 == Prj2Chunks.RoomWaterLevel)
                        room.WaterLevel = chunkIO.ReadChunkByte(chunkSize2);
                    else if (id2 == Prj2Chunks.RoomMistLevel)
                        room.MistLevel = chunkIO.ReadChunkByte(chunkSize2);
                    else if (id2 == Prj2Chunks.RoomReflectionLevel)
                        room.ReflectionLevel = chunkIO.ReadChunkByte(chunkSize2);
                    else if (id2 == Prj2Chunks.RoomReverberation)
                        room.Reverberation = (Reverberation)chunkIO.ReadChunkByte(chunkSize2);
                    else if (id2 == Prj2Chunks.RoomAlternate)
                    {
                        short alternateGroup = 1;
                        long alternateRoomIndex = -1;
                        chunkIO.ReadChunks((id3, chunkSize3) =>
                        {
                            if (id3 == Prj2Chunks.AlternateGroup)
                                alternateGroup = chunkIO.ReadChunkShort(chunkSize3);
                            else if (id3 == Prj2Chunks.AlternateRoom)
                                alternateRoomIndex = chunkIO.ReadChunkLong(chunkSize3);
                            else
                                return false;
                            return true;
                        });
                        roomLinkActions.Add(new KeyValuePair<long, Action<Room>>(alternateRoomIndex, (alternateRoom) =>
                            {
                                if (room.AlternateRoom != null)
                                    logger.Error("The room " + room + " has more than 1 flip room.");
                                else if (alternateRoom.AlternateBaseRoom != null)
                                    logger.Error("Room  " + alternateRoom + " is used for more than 1 flip room.");
                                else
                                {
                                    room.AlternateRoom = alternateRoom;
                                    room.AlternateGroup = alternateGroup;
                                    alternateRoom.AlternateBaseRoom = room;
                                    alternateRoom.AlternateGroup = alternateGroup;
                                }
                            }));
                    }

                    // Read objects
                    else if (id2 == Prj2Chunks.Objects)
                    {
                        chunkIO.ReadChunks((id3, chunkSize3) =>
                        {
                            var objectID = LEB128.ReadLong(chunkIO.Raw);
                            if (id3 == Prj2Chunks.ObjectMovable)
                            {
                                var instance = new MoveableInstance();
                                instance.Position = chunkIO.Raw.ReadVector3();
                                instance.RotationY = chunkIO.Raw.ReadSingle();
                                instance.ScriptId = ReadOptionalLEB128Ushort(chunkIO.Raw);
                                instance.WadObjectId = chunkIO.Raw.ReadUInt32();
                                instance.Ocb = chunkIO.Raw.ReadInt16();
                                instance.Invisible = chunkIO.Raw.ReadBoolean();
                                instance.ClearBody = chunkIO.Raw.ReadBoolean();
                                instance.CodeBits = chunkIO.Raw.ReadByte();
                                instance.Color = chunkIO.Raw.ReadVector4();
                                room.AddObjectAndSingularPortal(level, instance);
                                newObjects.Add(objectID, instance);
                            }
                            else if (id3 == Prj2Chunks.ObjectStatic)
                            {
                                var instance = new StaticInstance();
                                newObjects.Add(objectID, instance);
                                instance.Position = chunkIO.Raw.ReadVector3();
                                instance.RotationY = chunkIO.Raw.ReadSingle();
                                instance.ScriptId = ReadOptionalLEB128Ushort(chunkIO.Raw);
                                instance.WadObjectId = chunkIO.Raw.ReadUInt32();
                                instance.Color = chunkIO.Raw.ReadVector4();
                                instance.Ocb = chunkIO.Raw.ReadUInt16();
                                room.AddObjectAndSingularPortal(level, instance);
                            }
                            else if (id3 == Prj2Chunks.ObjectCamera)
                            {
                                var instance = new CameraInstance();
                                instance.Position = chunkIO.Raw.ReadVector3();
                                instance.ScriptId = ReadOptionalLEB128Ushort(chunkIO.Raw);
                                instance.Fixed = chunkIO.Raw.ReadBoolean();
                                room.AddObjectAndSingularPortal(level, instance);
                                newObjects.Add(objectID, instance);
                            }
                            else if (id3 == Prj2Chunks.ObjectFlyBy)
                            {
                                var instance = new FlybyCameraInstance();
                                instance.Position = chunkIO.Raw.ReadVector3();
                                instance.SetArbitaryRotationsYX(chunkIO.Raw.ReadSingle(), chunkIO.Raw.ReadSingle());
                                instance.Roll = chunkIO.Raw.ReadSingle();
                                instance.ScriptId = ReadOptionalLEB128Ushort(chunkIO.Raw);
                                instance.Speed = chunkIO.Raw.ReadSingle();
                                instance.Fov = chunkIO.Raw.ReadSingle();
                                instance.Flags = LEB128.ReadUShort(chunkIO.Raw);
                                instance.Number = LEB128.ReadUShort(chunkIO.Raw);
                                instance.Sequence = LEB128.ReadUShort(chunkIO.Raw);
                                instance.Timer = LEB128.ReadShort(chunkIO.Raw);
                                room.AddObjectAndSingularPortal(level, instance);
                                newObjects.Add(objectID, instance);
                            }
                            else if (id3 == Prj2Chunks.ObjectSink)
                            {
                                var instance = new SinkInstance();
                                instance.Position = chunkIO.Raw.ReadVector3();
                                instance.ScriptId = ReadOptionalLEB128Ushort(chunkIO.Raw);
                                instance.Strength = chunkIO.Raw.ReadInt16();
                                room.AddObjectAndSingularPortal(level, instance);
                                newObjects.Add(objectID, instance);
                            }
                            else if (id3 == Prj2Chunks.ObjectSoundSource)
                            {
                                var instance = new SoundSourceInstance();
                                instance.Position = chunkIO.Raw.ReadVector3();
                                instance.SoundId = chunkIO.Raw.ReadUInt16();
                                instance.Flags = chunkIO.Raw.ReadInt16();
                                instance.CodeBits = chunkIO.Raw.ReadByte();
                                room.AddObjectAndSingularPortal(level, instance);
                                newObjects.Add(objectID, instance);
                            }
                            else if (id3 == Prj2Chunks.ObjectImportedGeometry)
                            {
                                var instance = new ImportedGeometryInstance();
                                instance.Position = chunkIO.Raw.ReadVector3();
                                instance.SetArbitaryRotationsYX(chunkIO.Raw.ReadSingle(), chunkIO.Raw.ReadSingle());
                                instance.Roll = chunkIO.Raw.ReadSingle();
                                instance.Scale = chunkIO.Raw.ReadSingle();
                                instance.Model = levelSettingsIds.ImportedGeometries.TryGetOrDefault(LEB128.ReadLong(chunkIO.Raw));
                                room.AddObjectAndSingularPortal(level, instance);
                                newObjects.Add(objectID, instance);
                            }
                            else if (id3 == Prj2Chunks.ObjectLight)
                            {
                                var instance = new LightInstance((LightType)LEB128.ReadLong(chunkIO.Raw));
                                instance.Position = chunkIO.Raw.ReadVector3();
                                instance.SetArbitaryRotationsYX(chunkIO.Raw.ReadSingle(), chunkIO.Raw.ReadSingle());
                                instance.Intensity = chunkIO.Raw.ReadSingle();
                                instance.Color = chunkIO.Raw.ReadVector3();
                                instance.InnerRange = chunkIO.Raw.ReadSingle();
                                instance.OuterRange = chunkIO.Raw.ReadSingle();
                                instance.InnerAngle = chunkIO.Raw.ReadSingle();
                                instance.OuterAngle = chunkIO.Raw.ReadSingle();
                                instance.Enabled = chunkIO.Raw.ReadBoolean();
                                instance.CastsShadows = chunkIO.Raw.ReadBoolean();
                                instance.IsDynamicallyUsed = chunkIO.Raw.ReadBoolean();
                                instance.IsStaticallyUsed = chunkIO.Raw.ReadBoolean();
                                room.AddObjectAndSingularPortal(level, instance);
                                newObjects.Add(objectID, instance);
                            }
                            else if (id3 == Prj2Chunks.ObjectPortal)
                            {
                                var area = new Rectangle(LEB128.ReadInt(chunkIO.Raw), LEB128.ReadInt(chunkIO.Raw), LEB128.ReadInt(chunkIO.Raw), LEB128.ReadInt(chunkIO.Raw));
                                var adjoiningRoomIndex = LEB128.ReadLong(chunkIO.Raw);
                                var direction = (PortalDirection)chunkIO.Raw.ReadByte();

                                // Create a replacement portal that uses the source room as a temporary placeholder
                                // If an issue comes up that prevents loading the second room, this placeholder will be used permanently.
                                var instance = new PortalInstance(area, direction, room);
                                instance.Opacity = (PortalOpacity)chunkIO.Raw.ReadByte();
                                roomLinkActions.Add(new KeyValuePair<long, Action<Room>>(adjoiningRoomIndex, (adjoiningRoom) => instance.AdjoiningRoom = adjoiningRoom ?? room));

                                room.AddObjectAndSingularPortal(level, instance);
                                newObjects.Add(objectID, instance);
                            }
                            else if (id3 == Prj2Chunks.ObjectTrigger)
                            {
                                var area = new Rectangle(LEB128.ReadInt(chunkIO.Raw), LEB128.ReadInt(chunkIO.Raw), LEB128.ReadInt(chunkIO.Raw), LEB128.ReadInt(chunkIO.Raw));
                                var instance = new TriggerInstance(area);
                                instance.TriggerType = (TriggerType)LEB128.ReadLong(chunkIO.Raw);
                                instance.TargetType = (TriggerTargetType)LEB128.ReadLong(chunkIO.Raw);
                                instance.TargetData = LEB128.ReadShort(chunkIO.Raw);
                                long targetObjectId = LEB128.ReadLong(chunkIO.Raw);
                                instance.Timer = LEB128.ReadShort(chunkIO.Raw);
                                instance.CodeBits = (byte)(LEB128.ReadLong(chunkIO.Raw) & 0x1f);
                                instance.OneShot = chunkIO.Raw.ReadBoolean();
                                objectLinkActions.Add(new KeyValuePair<long, Action<ObjectInstance>>(targetObjectId, (targetObj) => instance.TargetObj = targetObj));

                                room.AddObjectAndSingularPortal(level, instance);
                                newObjects.Add(objectID, instance);
                            }
                            else
                                return false;
                            return true;
                        });
                    }
                    else
                        return false;
                    return true;
                });

                // Add room
                if ((roomIndex > 0) && (roomIndex < level.Rooms.Length) && (level.Rooms[roomIndex] == null))
                    level.Rooms[roomIndex] = room;
                else
                    level.AssignRoomToFree(room);

                if (!newRooms.ContainsKey(roomIndex))
                    newRooms.Add(roomIndex, room);
                return true;
            });

            // Link rooms
            foreach (var roomLinkAction in roomLinkActions)
                try
                {
                    roomLinkAction.Value(newRooms.TryGetOrDefault(roomLinkAction.Key));
                }
                catch (Exception exc)
                {
                    logger.Error(exc, "An exception was raised while trying to perform room link action.");
                }

            // Link objects
            foreach (var objectLinkAction in objectLinkActions)
                try
                {
                    objectLinkAction.Value(newObjects.TryGetOrDefault(objectLinkAction.Key));
                }
                catch (Exception exc)
                {
                    logger.Error(exc, "An exception was raised while trying to perform room link objects.");
                }

            // Now build the real geometry and update geometry buffers
            Parallel.ForEach(level.Rooms.Where(room => room != null), (room) => room.UpdateCompletely());

            return true;
        }

        private static ushort? ReadOptionalLEB128Ushort(BinaryReaderEx reader)
        {
            long read = LEB128.ReadLong(reader);
            if (read < 0)
                return null;
            else if (read > ushort.MaxValue)
                return ushort.MaxValue;
            else
                return (ushort)read;
        }
    }
}
