using NUnit.Framework;
using UnityEngine;
using DungeonSystem.Core;
using DungeonSystem.Settings;

public class DungeonGenerationTests
{
    [Test]
    public void DungeonGraphIsConnected()
    {
        var settings = ScriptableObject.CreateInstance<GenerationSettings>();
        var (data, root) = BSPGenerator.GenerateDungeon(settings);
        RoomConnector.ConnectRooms(root, data, settings);
        Assert.IsTrue(data.AreAllRoomsConnected(), "Generated dungeon should be fully connected");
    }

    [Test]
    public void RoomsDoNotOverlap()
    {
        var settings = ScriptableObject.CreateInstance<GenerationSettings>();
        var (data, _) = BSPGenerator.GenerateDungeon(settings);
        for (int i = 0; i < data.rooms.Count; i++)
        {
            for (int j = i + 1; j < data.rooms.Count; j++)
            {
                Assert.IsFalse(data.rooms[i].bounds.Overlaps(data.rooms[j].bounds),
                    $"Rooms {i} and {j} overlap");
            }
        }
    }

    [Test]
    public void EntranceExists()
    {
        var settings = ScriptableObject.CreateInstance<GenerationSettings>();
        var (data, root) = BSPGenerator.GenerateDungeon(settings);
        RoomConnector.ConnectRooms(root, data, settings);
        var entrance = data.doors.Find(d => d.isEntrance);
        Assert.IsNotNull(entrance, "Dungeon should have an entrance door");
    }
}
