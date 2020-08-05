using SimpleJSON;
using MelonLoader;
namespace AudicaModding
{
    public class Encoder
    {

        public static string GetConfig(Config config)
        {
            var configJSON = new JSONObject();

            configJSON["activated"] = config.activated;
            configJSON["positionSmoothing"] = config.positionSmoothing;
            configJSON["rotationSmoothing"] = config.rotationSmoothing;
            configJSON["camHeight"] = config.camHeight;
            configJSON["camDistance"] = config.camDistance;
            configJSON["camRotation"] = config.camRotation;
            configJSON["camOffset"] = config.camOffset;


            return configJSON.ToString(4);
        }

        public static void SetConfig(Config config, string data)
        {
            var configJSON = JSON.Parse(data);

            //Old version support
            try
            {
                config.activated = configJSON["activated"];
            }
            catch
            {
                MelonModLogger.Log("Save file from V 1.0.0 loaded");
            }

            config.positionSmoothing = configJSON["positionSmoothing"];
            config.rotationSmoothing = configJSON["rotationSmoothing"];
            config.camHeight = configJSON["camHeight"];
            config.camDistance = configJSON["camDistance"];
            config.camRotation = configJSON["camRotation"];
            config.camOffset = configJSON["camOffset"];
        }
    }
}
