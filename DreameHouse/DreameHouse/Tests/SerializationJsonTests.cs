using Domain.Entities;
using DreameHouse.Domain.Entities;
using DreameHouse.Infrastructure.Serialization;
using Xunit;

namespace DreameHouse.Tests
{
    public class UniversalSerializationJsonTests
    {
        private List<Level> GetSampleLevels()
        {
            return new List<Level>
            {
                new Level
                {
                    Number = 1,
                    RewardBitcoin = 100,
                    MaxMoves = 20,
                    TargetType = TileType.BOMB,
                    TargetCount = 5
                },
                new Level
                {
                    Number = 2,
                    RewardBitcoin = 200,
                    MaxMoves = 15,
                    TargetType = TileType.ROCKET,
                    TargetCount = 10
                }
            };
        }

        [Fact]
        public void Serialize_ShouldProduceValidJsonString()
        {
            var levels = GetSampleLevels();

            // Универсальная сериализация
            var json = JsonSerializer.Serialize(levels);

            Assert.NotNull(json);
            Assert.Contains("\"Number\": 1", json);
            Assert.Contains("\"RewardBitcoin\": 100", json);
            Assert.Contains("\"MaxMoves\": 20", json);
            Assert.Contains("\"TargetType\": \"BOMB\"", json);
            Assert.Contains("\"TargetCount\": 5", json);

            Assert.Contains("\"Number\": 2", json);
            Assert.Contains("\"RewardBitcoin\": 200", json);
            Assert.Contains("\"MaxMoves\": 15", json);
            Assert.Contains("\"TargetType\": \"ROCKET\"", json);
            Assert.Contains("\"TargetCount\": 10", json);
        }

        [Fact]
        public void Deserialize_ShouldParseJsonStringCorrectly()
        {
            string json = @"
[
  {
    ""Number"": 1,
    ""RewardBitcoin"": 100,
    ""MaxMoves"": 20,
    ""TargetType"": ""BOMB"",
    ""TargetCount"": 5
  },
  {
    ""Number"": 2,
    ""RewardBitcoin"": 200,
    ""MaxMoves"": 15,
    ""TargetType"": ""ROCKET"",
    ""TargetCount"": 10
  }
]";

            var levels = JsonDeserializer.Deserialize<Level>(json);

            Assert.NotNull(levels);
            Assert.Equal(2, levels.Count);

            Assert.Equal(1, levels[0].Number);
            Assert.Equal(100, levels[0].RewardBitcoin);
            Assert.Equal(20, levels[0].MaxMoves);
            Assert.Equal(TileType.BOMB, levels[0].TargetType);
            Assert.Equal(5, levels[0].TargetCount);

            Assert.Equal(2, levels[1].Number);
            Assert.Equal(200, levels[1].RewardBitcoin);
            Assert.Equal(15, levels[1].MaxMoves);
            Assert.Equal(TileType.ROCKET, levels[1].TargetType);
            Assert.Equal(10, levels[1].TargetCount);
        }

        [Fact]
        public void SerializeAndDeserialize_ShouldPreserveData()
        {
            var originalLevels = GetSampleLevels();

            var json = JsonSerializer.Serialize(originalLevels);
            var deserializedLevels = JsonDeserializer.Deserialize<Level>(json);

            Assert.Equal(originalLevels.Count, deserializedLevels.Count);

            for (int i = 0; i < originalLevels.Count; i++)
            {
                Assert.Equal(originalLevels[i].Number, deserializedLevels[i].Number);
                Assert.Equal(originalLevels[i].RewardBitcoin, deserializedLevels[i].RewardBitcoin);
                Assert.Equal(originalLevels[i].MaxMoves, deserializedLevels[i].MaxMoves);
                Assert.Equal(originalLevels[i].TargetType, deserializedLevels[i].TargetType);
                Assert.Equal(originalLevels[i].TargetCount, deserializedLevels[i].TargetCount);
            }
        }
    }
}
