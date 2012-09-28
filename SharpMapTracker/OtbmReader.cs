using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SharpTibiaProxy.Domain;

namespace SharpMapTracker
{
    public class OtbmReader
    {
        public OtMap Open(string filename, Otb items)
        {
            OtMap map = new OtMap();
            using (var fileStream = new FileStream(filename, FileMode.Open))
            {
                using (FileLoader reader = new FileLoader(fileStream))
                {
                    //get root node
                    BinaryReader nodeRoot = reader.GetRootNode();
                    if (nodeRoot == null)
                        throw new Exception("Null root node.");

                    if ((OtMap.OtMapNodeTypes)nodeRoot.ReadByte() != OtMap.OtMapNodeTypes.ROOTV1)
                        throw new Exception("Invalid root node.");

                    map.Version = nodeRoot.ReadUInt32();
                    map.Width = nodeRoot.ReadUInt16();
                    map.Height = nodeRoot.ReadUInt16();
                    map.MajorVersionItems = nodeRoot.ReadUInt32();
                    map.MinorVersionItems = nodeRoot.ReadUInt32();

                    if (map.Version <= 0)
                        throw new Exception("This map needs to be upgraded by using the latest map editor version to be able to load correctly.");
                    if (map.Version > 3)
                        throw new Exception("Unknown OTBM version detected.");
                    if (map.MajorVersionItems < 3)
                        throw new Exception("This map needs to be upgraded by using the latest map editor version to be able to load correctly.");
                    if (map.MajorVersionItems > items.MajorVersion)
                        throw new Exception("The map was saved with a different items.otb version, an upgraded items.otb is required.");

                    BinaryReader nodeMap = reader.GetChildNode();
                    if (nodeMap == null)
                        throw new Exception("Null child node.");

                    if ((OtMap.OtMapNodeTypes)nodeMap.ReadByte() != OtMap.OtMapNodeTypes.MAP_DATA)
                        throw new Exception("Invalid map data.");

                    while (nodeMap.PeekChar() != -1)
                    {
                        var mapAttr = (OtMap.OtMapAttrTypes)nodeMap.ReadByte();
                        switch (mapAttr)
                        {
                            case OtMap.OtMapAttrTypes.DESCRIPTION:
                                map.Descriptions.Add(nodeMap.ReadString());
                                break;
                            case OtMap.OtMapAttrTypes.EXT_SPAWN_FILE:
                                map.SpawnFile = nodeMap.ReadString();
                                break;
                            case OtMap.OtMapAttrTypes.EXT_HOUSE_FILE:
                                map.HouseFile = nodeMap.ReadString();
                                break;
                            default:
                                throw new Exception("Unknown header node.");
                        }
                    }

                    var nodeMapData = reader.GetChildNode();
                    while (nodeMapData != null)
                    {
                        var mapAttr = (OtMap.OtMapNodeTypes)nodeMapData.ReadByte();

                        switch (mapAttr)
                        {
                            case OtMap.OtMapNodeTypes.TILE_AREA:

                                var baseLocation = new Location(nodeMapData.ReadUInt16(), nodeMapData.ReadUInt16(), nodeMapData.ReadByte());
                                var nodeTile = reader.GetChildNode();
                                while (nodeTile != null)
                                {
                                    var tileType = (OtMap.OtMapNodeTypes)nodeTile.ReadByte();
                                    if (tileType == OtMap.OtMapNodeTypes.TILE || tileType == OtMap.OtMapNodeTypes.HOUSETILE)
                                    {
                                        var tileLocation = new Location(baseLocation.X + nodeTile.ReadUInt16(),
                                            baseLocation.Y + nodeTile.ReadUInt16(), baseLocation.Z + nodeTile.ReadByte());

                                        if (tileType == OtMap.OtMapNodeTypes.HOUSETILE)
                                        {
                                            var houseId = nodeTile.ReadUInt32();
                                        }

                                        while (nodeTile.PeekChar() != -1)
                                        {
                                            var tileAttr = (OtMap.OtMapAttrTypes)nodeTile.ReadByte();
                                            if (tileAttr == OtMap.OtMapAttrTypes.TILE_FLAGS)
                                            {
                                                var flags = nodeTile.ReadUInt32();
                                            }
                                            else if (tileAttr == OtMap.OtMapAttrTypes.ITEM)
                                            {
                                                var itemId = nodeTile.ReadUInt16();
                                            }
                                        }
                                    }

                                    var nodeItem = reader.GetChildNode();
                                    while (nodeItem != null)
                                    {
                                        var type = (OtMap.OtMapAttrTypes)nodeItem.ReadByte();
                                        if (type == OtMap.OtMapAttrTypes.ITEM)
                                        {
                                        }
                                    }

                                }



                                break;
                            case OtMap.OtMapNodeTypes.TOWNS:
                                break;
                            case OtMap.OtMapNodeTypes.WAYPOINTS:
                                break;
                            default:
                                throw new Exception("Unknown map node.");
                        }

                        nodeMapData = reader.GetChildNode();
                    }

                }
            }


            return map;
        }

    }
}
