using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using BSLedShow.Lighting;

namespace BSLedShow.Utils
{
    public class LightGroupsProvider
    {
        public Dictionary<string, int[]> LedGroups { get; set; }
        public Dictionary<string, Dictionary<int, Dictionary<int, string[]>>> LightGroupsMatching { get; set; }

        public void AppendByteArrayWithLedIndexes(string environmentName, int lightType, int lightId, ConcurrentDictionary<int, LightsProcessor.ExtractedEventData> allLightIdsEvents, byte[] RGBColor, ref byte[] byteArray, ref int bitPosition)
        {
            if (LedGroups == null || LightGroupsMatching == null)
            {
                return;
            }

            Dictionary<int, Dictionary<int, string[]>> lightTypesDict;

            if (!LightGroupsMatching.TryGetValue(environmentName, out lightTypesDict))
            {
                if (!LightGroupsMatching.TryGetValue("Default", out lightTypesDict))
                {
                    lightTypesDict = LightGroupsMatching.First().Value;
                }
            }

            if (lightTypesDict != null && lightTypesDict.TryGetValue(lightType, out var lightIdDict))
            {
                if (lightId == -1)
                {
                    foreach (var id in lightIdDict)
                    {
                        if (allLightIdsEvents.TryGetValue(id.Key, out var eventData) && eventData.isActive == true)
                        {
                            continue;
                        }

                        foreach (var name in id.Value)
                        {
                            if (LedGroups.TryGetValue(name, out var indexes))
                            {
                                TCPPacketSender.AppendByteArray(indexes[0], indexes[1], RGBColor, ref byteArray, ref bitPosition);
                            }
                        }
                    }
                }
                else if (lightIdDict.TryGetValue(lightId, out var lightGroupNames))
                {
                    foreach (var name in lightGroupNames)
                    {
                        if (LedGroups.TryGetValue(name, out var indexes))
                        {
                            TCPPacketSender.AppendByteArray(indexes[0], indexes[1], RGBColor, ref byteArray, ref bitPosition);
                        }
                    }
                }
            }
        }

        public void GetLedIndexes(string environmentName, int lightType, int lightId, ref List<int> ledIndexes)
        {
            Dictionary<int, Dictionary<int, string[]>> lightTypesDict;
            if (!LightGroupsMatching.TryGetValue(environmentName, out lightTypesDict))
            {
                lightTypesDict = LightGroupsMatching.First().Value;
            }
            if (lightTypesDict != null && lightTypesDict.TryGetValue(lightType, out var lightIdDict) && lightIdDict.TryGetValue(lightId, out var lightGroupNames))
            {
                foreach (var name in lightGroupNames)
                {
                    if (LedGroups.TryGetValue(name, out var indexes))
                    {
                        foreach (var index in indexes)
                        ledIndexes.Add(index);
                    }
                }
                //var resultArray = ledIndexes.ToArray();

                //return ledIndexes.Count > 0 ? resultArray : null;
            }
            //return null;
        }
    }
}
