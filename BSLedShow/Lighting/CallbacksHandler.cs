using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using static AlphabetScrollInfo;
using BSLedShow.Utils;
using BSLedShow.Lighting;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;
using System.Runtime.Remoting.Messaging;

namespace BSLedShow.Lighting
{
    class CallbacksHandler
    {
        public static void HandleBeatmapLightEventCallback(BasicBeatmapEventData eventData)
        {
            Plugin.Instance.LightsProcessor.AddEvent(eventData);
        }

        public static void HandleBeatmapBoostLightEventCallback(ColorBoostBeatmapEventData eventData)
        {
            if (eventData.boostColorsAreOn)
            {
                Plugin.Instance.boostColorsOffset = 3;
            }
            else
            {
                Plugin.Instance.boostColorsOffset = 0;
            }
        }

        public static void HandleBeatmapBPMChangingCallback(BPMChangeBeatmapEventData bpmChangeEventData)
        {
            Plugin.Instance.currentBPM = bpmChangeEventData.bpm;
        }

    }
}
