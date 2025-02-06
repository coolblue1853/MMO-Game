﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.IO;



#if UNITY_EDITOR
using UnityEditor;
#endif
public class MapEditor
{

#if UNITY_EDITOR

    [MenuItem("Tools/GenerateMap")]
    private static void GenerateMap()
    {
        GameObject[] gameObjects = Resources.LoadAll<GameObject>("Prefabs/Map");

        foreach (GameObject go in gameObjects)
        {
            Tilemap tmBase = Util.FindChild<Tilemap>(go, "TileMap_Base", true);
            Tilemap tm = Util.FindChild<Tilemap>(go, "TileMap_Collision", true);

            using (var writer = File.CreateText($"Assets/Resources/Map/{go.name}.txt"))
            {
                writer.WriteLine(tmBase.cellBounds.xMin);
                writer.WriteLine(tmBase.cellBounds.xMax);
                writer.WriteLine(tmBase.cellBounds.yMin);
                writer.WriteLine(tmBase.cellBounds.yMax);


                for (int y = tmBase.cellBounds.yMax; y >= tmBase.cellBounds.yMin; y--)
                {
                    for (int x = tmBase.cellBounds.xMin; x <= tmBase.cellBounds.xMax; x++)
                    {
                        TileBase tile = tm.GetTile(new Vector3Int(x, y, 0));
                        if (tile != null)
                            writer.Write(1);
                        else
                            writer.Write(0);

                    }
                    writer.WriteLine();
                }
            }
        }
    }


#endif

}
